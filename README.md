# EventHub

Reference implementation: Angular SPA + .NET 10 Web API + Azure Functions (isolated) + Azure SQL + Azure Service Bus + SignalR. Planning artifacts live under `_bmad-output/planning-artifacts/` (PRD, architecture, epics).

## Prerequisites (local)

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (LTS; project tested with Node 22)
- SQL Server **LocalDB** or another SQL Server reachable with a connection string
- Optional: [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) v4 for running the worker locally
- Optional: [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) + Bicep (`az bicep install`) for infrastructure

## Architecture decisions (short)

| Topic | Decision |
|--------|-----------|
| Write path | API enqueues to Service Bus; Function persists to SQL (no SQL on POST hot path). |
| Real-time | After enqueue, API broadcasts `EventCreated` on SignalR hub `/hubs/events` (payload matches list DTOs). |
| API docs (dev) | Built-in OpenAPI + Scalar (`/scalar/v1`). |
| Queue name | Fixed: `events`. |
| `EventType` in SQL | Stored as string, not int. |
| Frontend | Angular 20, standalone components, signals, reactive forms for the event form. |

Full detail: `_bmad-output/planning-artifacts/architecture.md`.

## Local configuration

### API (`backend/EventHub.Api`)

- **SQL:** `ConnectionStrings:DefaultConnection` in `appsettings.json` / `appsettings.Development.json`.
- **Service Bus:** `ServiceBus:ConnectionString`. For local UI-only runs, `ServiceBus:DisablePublishing` can be `true` in **Development** (POST returns 201 without sending to Azure).
- **CORS (Development):** `http(s)://localhost:4200` with credentials enabled (needed for SignalR from the SPA).
- **Telemetry:** Set `APPLICATIONINSIGHTS_CONNECTION_STRING` or `ApplicationInsights:ConnectionString` to enable Application Insights.

Run (HTTPS, port **7176** by default):

```bash
dotnet run --project backend/EventHub.Api/EventHub.Api.csproj --launch-profile https
```

Health: `GET https://localhost:7176/health` (includes database connectivity).

### Database migrations

From the repo root (or `backend/`):

```bash
dotnet ef database update --project backend/EventHub.Infrastructure/EventHub.Infrastructure.csproj --startup-project backend/EventHub.Api/EventHub.Api.csproj
```

Override SQL at design-time with env var `EVENTHUB_SQL` if needed.

### Functions (`backend/EventHub.Functions`)

`local.settings.json` → `Values`:

- `ConnectionStrings:DefaultConnection` — same database as the API
- `ServiceBus:ConnectionString` and `ServiceBusConnection` — full Service Bus connection string (trigger binding uses `ServiceBusConnection`)

```bash
cd backend/EventHub.Functions
func start
```

Without a real namespace, the host may fail to connect; use an Azure Service Bus namespace or a team emulator if you have one.

### Frontend (`frontend`)

```bash
cd frontend
npm install
npm start
```

- Default dev build uses `src/environments/environment.development.ts` → `apiUrl: 'https://localhost:7176'`.
- Trust the dev certificate: `dotnet dev-certs https --trust`.
- SignalR uses `withCredentials: true`; the API must allow your exact SPA origin (see CORS above).

## Azure deployment (Bicep)

`infrastructure/main.bicep` deploys into an **existing** resource group:

- Log Analytics + Application Insights  
- SQL Server + database (`Basic` tier) + firewall rule for Azure services  
- Service Bus (Standard) + queue **`events`** (max delivery count **10**)  
- Storage account (Functions runtime)  
- Linux App Service plan (**B1**) + Web App for the API (**.NET 10** stack)  
- Linux Consumption plan + Function App (**.NET isolated** / Functions v4)  

Example:

```bash
az group create -n rg-eventhub-dev -l eastus
az deployment group create \
  -g rg-eventhub-dev \
  -f infrastructure/main.bicep \
  -p sqlAdministratorPassword='YourComplexPasswordHere!'
```

Outputs include `apiBaseUrl`, `functionAppBaseUrl`, and SQL FQDN. After deploy:

1. Run **EF migrations** against the Azure SQL database (same admin user/password as the template).  
2. **Publish** API and Functions assemblies to the Web App and Function App (GitHub Actions, `az webapp deploy`, Visual Studio, etc.).  
3. Point the SPA **`apiUrl`** (production `environment.ts`) at the deployed API HTTPS URL.

If a region does not yet offer **`DOTNETCORE|10.0`** or **`DOTNET-ISOLATED|10.0`** on App Service, change `linuxFxVersion` in `infrastructure/modules/appservice-api.bicep` and `functionapp.bicep` to a supported runtime and align the project target framework if required.

### Name collisions

Defaults use `uniqueString(resourceGroup().id)` for several names. If deployment fails for uniqueness, pass explicit values for `sqlServerName`, `serviceBusNamespaceName`, or `storageAccountName`.

## CI

`.github/workflows/ci.yml` builds the .NET solution and the Angular app (development configuration). Validate Bicep locally:

```bash
az bicep build --file infrastructure/main.bicep
```

## Troubleshooting

| Symptom | Things to check |
|--------|------------------|
| SPA cannot call API | HTTPS cert trusted; API running; CORS origin exactly `http://localhost:4200` or `https://localhost:4200`. |
| SignalR never connects | Same as CORS + credentials; API URL in `environment.development.ts` matches the API origin (including port). |
| POST 500 when publishing | Real Service Bus connection string, or set `ServiceBus:DisablePublishing` to `true` in Development. |
| Function does not persist | `ServiceBusConnection` and SQL connection in `local.settings.json`; queue name must be `events`. |
| Azure SQL from laptop | Add your client IP to the SQL server firewall (template only allows Azure services + your rules). |

## Repository layout

| Path | Purpose |
|------|---------|
| `backend/` | .NET solution (Core, Infrastructure, Api, Functions) |
| `frontend/` | Angular SPA |
| `infrastructure/` | Bicep modules + `main.bicep` |
| `_bmad-output/` | BMAD planning outputs |
