---
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8]
inputDocuments: [prd.md]
workflowType: architecture
project_name: EventHub
user_name: Vlad
date: '2026-03-27'
lastStep: 8
status: complete
completedAt: '2026-03-27'
---

# Architecture Decision Document — EventHub

_This document is the technical single source of truth for EventHub. Implementation (human or AI) should align with ADRs, patterns, and structure below._

---

## 1. Project context analysis

### Requirements overview (from PRD)

**Functional:** 39 FRs across seven areas — event submission, async pipeline, discovery (filter/sort/page), real-time UI updates, REST + docs, observability, IaC/CI/CD/local dev.

**Non-functional:** POST/GET latency budgets, pipeline lag, bundle size, at-least-once + idempotent persistence, HTTPS/TLS, CORS/credentials for SignalR, layered maintainability, WCAG 2.1 AA baseline for core UI.

**Scale:** Medium complexity — one SPA, one API host, one queue consumer, one relational store. No multi-tenant or regulated-domain requirements in v1.

**Primary domain:** Full-stack web (Angular SPA + .NET HTTP API + isolated Azure Functions worker).

**Estimated logical components:** SPA; API; Functions worker; message broker; SQL; IaC; CI pipelines; shared domain/infrastructure libraries.

### Technical constraints

- Azure-first (Service Bus, Functions, App Service, SQL, Application Insights).
- PRD mandates decoupled write path and real-time read UX.
- Local dev needs a real Service Bus namespace (no full emulator parity).

### Cross-cutting concerns

- **Consistency:** Event identity and enum storage must match across API, message JSON, and DB.
- **CORS + SignalR:** Origins and credentials must be configured once and documented.
- **Secrets:** Connection strings and keys only in configuration / Key Vault patterns — never in repo.
- **Observability:** Structured logs + App Insights on API and Functions.

---

## 2. Starter template evaluation

### Primary technology domain

Full-stack: **Angular SPA** + **.NET Web API** + **Azure Functions (isolated worker)**.

### Options considered

| Approach | Pros | Cons |
|---|---|---|
| **Official CLIs** (`dotnet new`, `ng new`) | Supported defaults, clear upgrade path | Manual wiring of multi-project solution |
| **T3 / Next full-stack** | Fast web iteration | Conflicts with PRD (.NET + Angular split) |
| **Single monolith host** | One deploy unit | Weak fit for Functions + App Service split in PRD |

### Selected approach: official scaffolding (no third-party mega-starter)

**Rationale:** PRD locks .NET 10, Angular 20, Functions v4 isolated, and clean layering. Official templates give minimal opinionated structure; we add solution layout, Core/Infrastructure split, and Azure wiring explicitly — which matches the reference-implementation goal.

**Initialization (reference commands — use current CLI help for flags):**

```bash
# Backend solution (example layout)
dotnet new sln -n EventHub
dotnet new classlib -n EventHub.Core -f net10.0
dotnet new classlib -n EventHub.Infrastructure -f net10.0
dotnet new webapi -n EventHub.Api -f net10.0
dotnet new worker -n EventHub.Functions -f net10.0   # then add Azure Functions worker packages

# Frontend
ng new frontend --style=scss --ssr=false   # standalone defaults per Angular 20 CLI
```

**Note:** First implementation story should establish the repo layout and project references exactly as in **§6 Project structure**.

**Runtime versions (verify at build time):**

- **.NET:** Target `net10.0`; SDK aligned with installed **.NET 10** (e.g. 10.0.201+ per machine).
- **Angular:** **20.x** (per PRD).
- **Node:** LTS compatible with Angular 20 (see Angular docs).

---

## 3. Core architectural decisions

### Decision priority

| Priority | Topic | Decision |
|---|---|---|
| Critical | Write path | API enqueues to Service Bus; Function persists to SQL |
| Critical | Data store | Azure SQL + EF Core migrations |
| Critical | Real-time | SignalR from API; `EventCreated` push on successful POST handling |
| Important | API docs | `Microsoft.AspNetCore.OpenApi` + Scalar in Development |
| Important | IaC | Bicep modules + `main.bicep` orchestration |
| Important | CI/CD | GitHub Actions: separate workflows for backend / frontend+infra |
| Deferred | Auth | None in v1 (network/perimeter); Phase 2 in PRD |

### ADR-001: Asynchronous persistence via Service Bus

**Context:** NFRs require API not to block on DB insert; FR-12 requires enqueue before response.

**Decision:** `POST /api/events` validates, assigns id/timestamp, publishes JSON to queue `events`, returns 201. Azure Function `ServiceBusTrigger` deserializes and calls repository `AddAsync`.

**Consequences:** (+) Meets latency target; (+) back-pressure and retries via broker; (−) eventual consistency until Function completes; (−) requires idempotency (PK on `Id`).

### ADR-002: Azure SQL + EF Core (not Cosmos for v1)

**Context:** PRD allows SQL or Cosmos; relational filters and team familiarity favour SQL.

**Decision:** Azure SQL (or LocalDB for dev) with EF Core 10.x, code-first migrations checked in.

**Consequences:** (+) LINQ filtering/sorting; (+) straightforward local dev; (−) not infinite horizontal read scale (acceptable for v1 targets).

### ADR-003: When SignalR fires

**Context:** FR-26 expects UI update quickly; pipeline is async.

**Decision:** After successful enqueue (and DTO ready), API pushes `EventCreated` to SignalR **immediately** — same payload shape as GET item. Function persistence remains independent.

**Consequences:** (+) Best perceived latency; (−) rare mismatch if Function fails (UI may show event before row exists — acceptable for debug tool per PRD).

### ADR-004: Clean architecture layers

**Decision:**

```
EventHub.Core          — entities, DTOs, repository + publisher interfaces (no infra refs)
EventHub.Infrastructure — DbContext, EF repositories, ServiceBus publisher, DI extensions
EventHub.Api           — controllers, SignalR hub, Program.cs
EventHub.Functions     — trigger + host wiring only; reuses Infrastructure DI
```

**Consequences:** (+) testable core; (+) one place to swap bus or DB; (−) slightly more projects than a single assembly.

### ADR-005: OpenAPI + Scalar (not Swashbuckle on .NET 10)

**Context:** Built-in OpenAPI in .NET 10; avoid Swashbuckle/OpenApi 2.x namespace friction.

**Decision:** `AddOpenApi` / `MapOpenApi` + `Scalar.AspNetCore` for dev UI (`/scalar/v1`).

### ADR-006: Infrastructure as Code — Bicep

**Decision:** Bicep modules for SQL, Service Bus (queue `events`), App Service (API), Function App + storage, Log Analytics + Application Insights. Parameters for secrets via deployment, not committed.

---

## 4. Implementation patterns & consistency rules

### Naming

| Area | Rule | Example |
|---|---|---|
| C# types | PascalCase | `Event`, `EventRepository` |
| JSON API | camelCase | `userId`, `createdAt` |
| DB table | PascalCase singular | `Events` |
| Queue | fixed name | `events` |
| SignalR method | PascalCase string | `EventCreated` |
| Angular files | kebab-case | `event-list.ts` |

### API

- REST resources plural: `/api/events`.
- Success: `201` + body for POST; `200` + `PagedResult` for GET.
- Validation failures: `400` + `application/problem+json` (ASP.NET validation problem details).
- Date/time: ISO 8601 UTC in JSON.

### Angular

- Standalone components only; `ChangeDetectionStrategy.OnPush`.
- State: signals in components; `HttpClient` for API; `@microsoft/signalr` for hub.
- Environment: `apiUrl` points to API origin; hub URL `${apiUrl}/hubs/events`.

### Errors & loading

- API: log exceptions with structured properties; no stack traces to clients in production.
- UI: skeleton for list load; inline errors for form; preserve form on network failure.

### Enforcement

- All agents **must** use the queue name `events` in publisher and Function trigger.
- All agents **must** store `EventType` as **string** in SQL (enum converted), not int.
- All async public methods accept `CancellationToken` where applicable.

---

## 5. Project structure & boundaries

### Repository tree (target)

```
EventHub/
├── .github/workflows/          # backend.yml, frontend.yml
├── backend/
│   ├── EventHub.sln
│   ├── EventHub.Core/
│   ├── EventHub.Infrastructure/
│   │   └── Data/Migrations/    # EF migrations (--output-dir Data/Migrations)
│   ├── EventHub.Api/
│   └── EventHub.Functions/
├── frontend/                   # Angular CLI root
│   └── src/app/
│       ├── core/               # models, services (event, signalr)
│       └── features/events/    # event-form, event-list
├── infrastructure/
│   ├── main.bicep
│   └── modules/
├── docs/                       # product-brief, project-context (optional)
├── _bmad-output/
└── README.md
```

### Boundary map

| FR category | Primary location |
|---|---|
| Submission | `frontend/.../event-form`, `EventHub.Api/Controllers` |
| Pipeline | `EventHub.Infrastructure/Services`, `EventHub.Functions` |
| Discovery | `EventHub.Infrastructure/Repositories`, `event-list`, GET controller |
| Real-time | `EventHub.Api/Hubs`, `core/services/signalr.ts` |
| Observability | `Program.cs` (API + Functions), App Insights packages |
| IaC/CI | `infrastructure/`, `.github/workflows/` |

### Data flow

1. Browser → `POST /api/events` → validate → publish → optionally SignalR broadcast → 201.
2. Queue → Function → deserialize → `AddAsync` → SQL.
3. Browser → `GET /api/events` → repository query → paged DTOs.
4. Browser ↔ SignalR hub for live inserts.

---

## 6. Architecture validation

### Coherence

Stack choices are compatible: .NET 10 + EF Core 10 + isolated Functions v4 + Angular 20 + Service Bus SDK. SignalR and CORS are explicitly called out in ADR-003 and patterns.

### Requirements coverage

| PRD area | Supported by |
|---|---|
| FR-01–FR-10 | API + Angular form |
| FR-11–FR-15 | API + Infrastructure + Functions |
| FR-16–FR-25 | Repository + Angular list |
| FR-26–FR-28 | SignalR hub + client service |
| FR-29–FR-32 | Controllers + OpenAPI + Scalar |
| FR-33–FR-35 | Health checks + App Insights + logging |
| FR-36–FR-39 | Bicep + GitHub Actions + README |

NFRs: latency targets align with async write + indexed reads; security baseline met by TLS, CORS, secrets policy; reliability by at-least-once + PK idempotency.

### Gaps (non-blocking)

- Automated load tests not specified — optional post-v1.
- E2E test folder optional in v1 — can add under `frontend/e2e` later.

### Readiness

**Status:** READY FOR IMPLEMENTATION  
**Confidence:** High for v1 scope defined in PRD.

### First implementation priorities

1. Create solution layout and project references per §5.
2. Define `Event` entity, DTOs, `AppDbContext`, initial migration.
3. Implement `ServiceBusPublisher` and `EventsController` POST.
4. Implement Function trigger and persistence.
5. Implement GET with filters and SignalR push.
6. Scaffold Angular form + list + SignalR service.
7. Add Bicep + workflows + README.

---

## 7. AI agent handoff

- Follow ADRs and tables in this document for any ambiguous choice.
- Prefer extending existing patterns over inventing parallel ones.
- After major code changes, update **`docs/project-context.md`** (create if missing) with stack pins and “do not change” list (queue name, hub path, method name).

**Next BMAD workflows:** `bmad-create-epics-and-stories` (or Scrum Master **CE**), then `bmad-check-implementation-readiness`, then development stories.
