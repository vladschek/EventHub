---
stepsCompleted: [validate-prerequisites, design-epics, create-stories, final-validation]
inputDocuments: [prd.md, architecture.md]
workflowType: epics-and-stories
status: complete
completedAt: '2026-03-27'
---

# EventHub — Epic Breakdown

## Overview

Decomposition of the [PRD](prd.md) and [Architecture](architecture.md) into epics and user stories. Each epic delivers **standalone user or team value**; stories build in order without forward dependencies.

## Requirements inventory

### Functional requirements

| ID | Capability |
|---|---|
| FR-01 | Submit event via web form (user ID, type, description) |
| FR-02 | Validate user ID present, max 256 |
| FR-03 | Validate description present, max 2000 |
| FR-04 | Validate type ∈ {PageView, Click, Purchase} |
| FR-05 | Inline field validation before submit |
| FR-06 | Description character guidance |
| FR-07 | Loading state while submitting |
| FR-08 | Success confirmation after submit |
| FR-09 | Error on failure without losing form data |
| FR-10 | Reset form after success |
| FR-11 | Assign unique Id + UTC timestamp on accept |
| FR-12 | Enqueue without waiting for DB before response |
| FR-13 | Worker persists from queue to durable store |
| FR-14 | Retries to max, then dead-letter |
| FR-15 | Structured log on successful processing |
| FR-16 | Paginated list, default newest-first |
| FR-17 | Filter by user ID (partial) |
| FR-18 | Filter by type (exact) |
| FR-19 | Filter by date range |
| FR-20 | Sort by createdAt / userId / type, asc/desc |
| FR-21 | Configurable page size |
| FR-22 | Navigate pages |
| FR-23 | Pagination metadata in response |
| FR-24 | Skeleton while loading list |
| FR-25 | Empty state when no rows |
| FR-26 | New events appear without full reload |
| FR-27 | Real-time connection at app load |
| FR-28 | Reconnect after connection loss |
| FR-29 | Programmatic POST |
| FR-30 | Programmatic GET with same query model as UI |
| FR-31 | Structured validation errors (problem details) |
| FR-32 | Interactive API docs in development |
| FR-33 | Health endpoint reflects DB connectivity |
| FR-34 | Centralized telemetry (API + worker) |
| FR-35 | Error logs with investigable context |
| FR-36 | Provision cloud resources via IaC command |
| FR-37 | CI/CD backend on merge to `main` |
| FR-38 | CI/CD frontend on merge to `main` |
| FR-39 | Local run documented (README + secrets) |

### Non-functional requirements (story hooks)

- **NFR-P01–P05:** Performance — addressed in API/list stories and filter debounce in UI.
- **NFR-R01–R03:** Reliability — idempotent save, DLQ, safe API errors.
- **NFR-S01–S04:** Security — HTTPS, CORS, TLS, no secrets in repo.
- **NFR-M01–M03:** Layering, migrations, documented operations.
- **NFR-A01:** WCAG 2.1 AA baseline — form + table stories.

### Additional requirements (from architecture)

- Solution layout: `EventHub.Core`, `EventHub.Infrastructure`, `EventHub.Api`, `EventHub.Functions`; queue name **`events`**; `EventType` stored as **string** in SQL.
- SignalR hub **`/hubs/events`**, method **`EventCreated`**; CORS with credentials for negotiation.
- OpenAPI + Scalar dev routes; EF migrations under `Infrastructure/Data/Migrations` with `--output-dir Data/Migrations`.
- `global.json` / target **net10.0**; Angular **20** standalone.

### UX design requirements

_No separate UX spec._ PRD + NFR-A01 drive layout, keyboard access, contrast, and semantics in UI stories.

### FR coverage map

| FRs | Epic |
|---|---|
| FR-01–FR-10 | Epic 5 (primary); Epic 2 enables API path for tests |
| FR-11, FR-12, FR-29, FR-31 (partial) | Epic 2 |
| FR-13–FR-15 | Epic 3 |
| FR-16–FR-25, FR-30 | Epic 4 |
| FR-26–FR-28 | Epic 6 |
| FR-31 (docs/errors), FR-32, FR-33–FR-35 | Epic 7 |
| FR-36–FR-39 | Epic 8 |

## Epic list

### Epic 1: Shared foundation for the event platform

**Outcome:** The repo builds; domain contracts and database schema for `Events` exist; infrastructure can talk to SQL and Service Bus configuration is wired for hosts.

**FRs:** Enables FR-11–FR-15, FR-16–FR-23 (data layer), NFR-M01–M02, architecture starter.

### Epic 2: Submit events through the API (acknowledge and enqueue)

**Outcome:** Clients can `POST /api/events` and receive `201` with assigned id and timestamp; message is on the bus without waiting for SQL insert.

**FRs:** FR-11, FR-12, FR-29, FR-31 (validation errors).

### Epic 3: Durably persist events from the queue

**Outcome:** Messages from `events` become rows in SQL; failures retry and dead-letter; successes are logged.

**FRs:** FR-13, FR-14, FR-15; NFR-R01–R03.

### Epic 4: Search and browse event history (API + list UI)

**Outcome:** Engineers use `GET /api/events` and the SPA table to filter, sort, and paginate with clear loading and empty states.

**FRs:** FR-16–FR-25, FR-30; NFR-P02, NFR-A01 (table).

### Epic 5: Guided event submission in the browser

**Outcome:** Engineers use the reactive form with validation, feedback, and reset — without losing data on errors.

**FRs:** FR-01–FR-10; NFR-P05 (debounce where applicable), NFR-A01 (form).

### Epic 6: Live event stream in the dashboard

**Outcome:** New events appear in the list automatically; connection restores after drops.

**FRs:** FR-26, FR-27, FR-28; NFR-P01 path (perceived freshness).

### Epic 7: Operability and integrator experience

**Outcome:** Operators check health; integrators use Scalar; both hosts emit telemetry and structured errors.

**FRs:** FR-32, FR-33, FR-34, FR-35; NFR-S02 (CORS documented with SignalR).

### Epic 8: Repeatable cloud delivery

**Outcome:** One-shot Bicep deploy story; CI/CD on `main`; README gets a new developer running locally.

**FRs:** FR-36, FR-37, FR-38, FR-39; NFR-S01, S03.

---

## Epic 1: Shared foundation for the event platform

**Goal:** Establish solution structure, domain model, EF schema for `Events`, and DI extensions so API and Functions can be built in later epics.

### Story 1.1: Scaffold backend solution and project references

As a **developer**,  
I want **a multi-project .NET solution matching the architecture doc**,  
So that **all later stories plug into a consistent structure**.

**Acceptance criteria:**

**Given** a clean `backend/` folder  
**When** the solution and projects are created per architecture (Core, Infrastructure, Api, Functions) with correct references  
**Then** `dotnet build` succeeds on all projects  
**And** `global.json` pins .NET 10 SDK if used by the team  

### Story 1.2: Event domain model and API contracts

As a **developer**,  
I want **`Event`, `EventType`, DTOs, and repository/publisher interfaces in Core**,  
So that **API and Infrastructure share one contract**.

**Acceptance criteria:**

**Given** `EventHub.Core`  
**When** models and DTOs are implemented with validation attributes matching the PRD limits  
**Then** `CreateEventDto`, `EventDto`, `EventFilterDto`, and `PagedResult<T>` are usable by API and tests  
**And** `EventType` maps to stored string values agreed in architecture  

### Story 1.3: Database context and initial migration

As a **developer**,  
I want **`AppDbContext` and an EF Core migration for `Events`**,  
So that **local and Azure SQL schemas stay versioned**.

**Acceptance criteria:**

**Given** connection string configuration  
**When** `dotnet ef database update` runs from documented commands  
**Then** the `Events` table exists with indexes for filter/sort columns  
**And** migration files live under `EventHub.Infrastructure/Data/Migrations`  

### Story 1.4: Infrastructure DI and Service Bus client wiring

As a **developer**,  
I want **`AddInfrastructure` / `AddInfrastructureForFunctions` registering DbContext, repository, and Service Bus publisher**,  
So that **API and Functions resolve the same abstractions**.

**Acceptance criteria:**

**Given** valid connection strings in configuration  
**When** API and Functions hosts start  
**Then** `IEventRepository` and `IEventPublisher` resolve without runtime errors  
**And** queue name `events` matches architecture (single constant or config key documented)  

---

## Epic 2: Submit events through the API (acknowledge and enqueue)

**Goal:** Expose POST that validates input, assigns identity, publishes to Service Bus, returns 201.

### Story 2.1: POST /api/events with validation and enqueue

As an **integrator**,  
I want **to create an event via HTTP POST**,  
So that **events enter the pipeline without blocking on the database**.

**Acceptance criteria:**

**Given** a valid `CreateEventDto` JSON body  
**When** `POST /api/events` is called  
**Then** response is `201` with `EventDto` including new `id` and `createdAt` UTC  
**And** a message is published to the `events` queue containing the event payload  
**And** FR-12 holds: response is not delayed by SQL insert  

**Given** an invalid body (missing fields or over max length)  
**When** `POST /api/events` is called  
**Then** response is `400` with validation problem details (FR-31)  

---

## Epic 3: Durably persist events from the queue

**Goal:** Isolated Function consumes queue and writes to SQL with logging and broker retry/DLQ semantics.

### Story 3.1: Service Bus–triggered function persists events

As **operations**,  
I want **queue messages to become database rows automatically**,  
So that **events are durable after the API returns**.

**Acceptance criteria:**

**Given** a valid JSON message on `events`  
**When** the Function runs  
**Then** a row is inserted with the same `Id` as in the message (idempotent PK)  
**And** structured log includes event id (FR-15)  

**Given** malformed JSON  
**When** the Function runs  
**Then** error is logged with context and exception propagates so Service Bus can retry / dead-letter (FR-14)  

---

## Epic 4: Search and browse event history (API + list UI)

**Goal:** Paged, filterable, sortable read API and Angular table UX.

### Story 4.1: GET /api/events with filter, sort, and pagination

As an **engineer**,  
I want **to query events with filters and paging**,  
So that **I can narrow down incidents quickly**.

**Acceptance criteria:**

**Given** existing events in the database  
**When** `GET /api/events` is called with optional `userId`, `type`, `fromDate`, `toDate`, `sortBy`, `sortDescending`, `page`, `pageSize`  
**Then** response matches `PagedResult<EventDto>` with correct metadata (FR-16–FR-23)  
**And** queries use indexed columns and `AsNoTracking` for reads  

### Story 4.2: Event list UI with filters, sort, pagination, skeleton, empty state

As an **engineer**,  
I want **a table UI wired to the GET API**,  
So that **I can explore history without Postman**.

**Acceptance criteria:**

**Given** the SPA is running against a configured `apiUrl`  
**When** the list loads  
**Then** skeleton placeholders show during fetch (FR-24)  
**And** filters/sort/pagination controls call the API with debounced text filter (NFR-P05)  
**And** empty result shows an explicit empty state (FR-25)  
**And** table uses semantic markup and keyboard-focusable controls (NFR-A01 baseline)  

---

## Epic 5: Guided event submission in the browser

**Goal:** Reactive form matching POST contract with full UX from FR-01–FR-10.

### Story 5.1: Event creation form (reactive, accessible)

As an **engineer**,  
I want **to submit events from the browser with clear validation**,  
So that **I can inject test traffic safely**.

**Acceptance criteria:**

**Given** the event form is displayed  
**When** required fields are empty or invalid  
**Then** inline messages appear without a round-trip (FR-05)  
**And** description shows character guidance relative to max length (FR-06)  

**Given** the user submits  
**When** the request is in flight  
**Then** submit shows loading/disabled state (FR-07)  

**Given** successful `201`  
**When** the response returns  
**Then** success feedback is shown and form resets (FR-08, FR-10)  

**Given** a network or server error  
**When** submit fails  
**Then** a clear error is shown and field values remain (FR-09)  

---

## Epic 6: Live event stream in the dashboard

**Goal:** SignalR push after POST; Angular subscribes and refreshes list.

### Story 6.1: SignalR hub and server push on create

As an **engineer**,  
I want **the API to broadcast new events to connected clients**,  
So that **the dashboard stays current**.

**Acceptance criteria:**

**Given** a successful POST handling path  
**When** an event is accepted and enqueued  
**Then** all clients receive `EventCreated` with the same payload shape as list items  
**And** hub route is `/hubs/events` per architecture  

### Story 6.2: Angular SignalR client and list integration

As an **engineer**,  
I want **the SPA to connect on startup and update the list on push**,  
So that **I do not manually refresh**.

**Acceptance criteria:**

**Given** the app loads  
**When** SignalR connects  
**Then** `EventCreated` triggers list refresh or prepend (FR-26, FR-27)  
**And** connection uses automatic reconnect (FR-28)  
**And** CORS allows credentials from configured Angular origin (NFR-S02)  

---

## Epic 7: Operability and integrator experience

**Goal:** Health, docs, telemetry, production-safe errors.

### Story 7.1: Health check and development API documentation

As **operations**,  
I want **`/health` and interactive API docs in Development**,  
So that **we can verify dependencies and onboard integrators**.

**Acceptance criteria:**

**Given** the API is running with a healthy database  
**When** `GET /health` is called  
**Then** status reflects DB connectivity (FR-33)  

**Given** Development environment  
**When** navigating to Scalar UI and OpenAPI JSON routes  
**Then** POST and GET contracts are visible including models (FR-32)  

### Story 7.2: Application Insights and structured error logging

As **operations**,  
I want **telemetry and rich logs on API and Functions**,  
So that **incidents are diagnosable**.

**Acceptance criteria:**

**Given** configured instrumentation keys / connection strings  
**When** API and Functions handle requests and messages  
**Then** requests and dependencies are visible in Application Insights (FR-34)  
**And** unhandled failures log structured context without leaking stacks to clients in production (FR-35, NFR-R03)  

---

## Epic 8: Repeatable cloud delivery

**Goal:** Bicep, GitHub Actions, README for local + Azure.

### Story 8.1: Bicep modules and main orchestration

As **DevOps**,  
I want **one deploy entrypoint for Azure resources**,  
So that **environments are reproducible**.

**Acceptance criteria:**

**Given** parameter file or CLI parameters for secrets  
**When** `az deployment group create` runs against `infrastructure/main.bicep`  
**Then** resources for SQL, Service Bus (queue `events`), API, Functions, monitoring are created per architecture (FR-36)  

### Story 8.2: GitHub Actions workflows

As **DevOps**,  
I want **CI/CD on push to `main`**,  
So that **builds and deploys stay automated**.

**Acceptance criteria:**

**Given** pushes touching `backend/**` or `frontend/**`  
**When** workflows run  
**Then** restore, build, test execute; deploy jobs use secrets as documented (FR-37, FR-38)  

### Story 8.3: README — architecture, decisions, local setup

As a **new developer**,  
I want **a single guide to run API, Functions, and Angular locally**,  
So that **I can contribute within the PRD onboarding target**.

**Acceptance criteria:**

**Given** the repository  
**When** I follow README prerequisites and steps  
**Then** I can apply migrations, configure Service Bus + SQL connection strings, and run all three processes (FR-39)  
**And** document records key ADR summaries and troubleshooting (SignalR/CORS, local Service Bus)  

---

## Final validation

| Check | Result |
|---|---|
| Every FR mapped to ≥1 story | Yes |
| Stories avoid forward dependencies within epic | Yes |
| Epics ordered by natural pipeline dependency | Yes |
| Architecture starter / structure reflected in Epic 1 | Yes |
| Tables/migrations introduced when first needed | Yes (Epic 1 migration before persistence stories) |
| NFR-A01 reflected in Epic 4–5 | Yes |

**Workflow status:** Complete. `epics.md` is ready for **`bmad-dev-story`** / sprint planning and implementation.
