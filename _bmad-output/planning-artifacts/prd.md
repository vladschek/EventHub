---
stepsCompleted:
  [
    init,
    discovery,
    vision,
    executive-summary,
    success,
    journeys,
    domain-skipped,
    innovation-skipped,
    project-type,
    scoping,
    functional,
    nonfunctional,
    polish,
    complete,
  ]
inputDocuments: []
workflowType: 'prd'
prdVersion: '1.0'
prdStatus: complete
classification:
  projectType: full-stack-web-app
  domain: general-developer-tooling
  complexity: medium
  projectContext: greenfield
---

# Product Requirements Document — EventHub

**Author:** Vlad  
**Date:** 2026-03-27  
**Status:** Approved for downstream work (Architecture → Epics → Implementation)

---

## Executive Summary

**EventHub** is a production-grade event observability platform for engineering teams: a live, filterable debug console to inject test events, monitor traffic, and query history — backed by a resilient, non-blocking write pipeline (API → message queue → worker → durable store).

**Target users:** Engineering teams running production software who need fast incident debugging and confidence in event data — not analytics generalists.

**Problem:** (1) Synchronous DB writes from APIs slow or block under load. (2) When something breaks, there is no quick, structured way to find what happened. Alternatives are either heavyweight analytics stacks or fragile direct-to-DB patterns.

**Solution:** Decouple writes: the API acknowledges after enqueueing; a worker persists asynchronously. The UI combines filterable history with real-time updates so engineers see new events without refresh and can narrow by user, type, and time during incidents.

### What Makes This Special

**Resilience and debuggability are one problem:** unreliable writes undermine trust in data; unreadable data undermines debugging. EventHub addresses both in one system:

- **Non-blocking write path:** Queue absorbs spikes; retries and dead-lettering avoid silent loss.
- **Live debug surface:** Real-time push plus filters on `userId`, `type`, and date range.
- **Lean ops:** Consumption workers, standard messaging, modest SQL — no dedicated platform team required.

### Project Classification

| Dimension | Value |
|---|---|
| **Project type** | Full-stack web app (Angular 20 SPA, .NET 10 API, Azure Functions v4) |
| **Domain** | Developer tooling / event observability |
| **Complexity** | Medium (async pipeline, real-time UI, cloud dependencies) |
| **Context** | Greenfield |

---

## Success Criteria

### User Success

- Engineer finds the relevant event sequence **within 30 seconds** using `userId`, `type`, and date filters during an incident.
- Developer sees a submitted test event **in the table within ~1 second** without manual refresh.
- New teammate completes first successful submit **within ~5 minutes** using README + UI alone.
- Table never appears blank without explanation (skeleton, empty state, or error).

### Business Success

- Another .NET + Angular developer can **fork, configure secrets, run locally in &lt; 30 minutes**.
- Codebase demonstrates production patterns (decoupled pipeline, layering, logging, health, IaC, CI/CD) as **portfolio or internal seed**.
- `POST /api/events` and `GET /api/events` (filter, sort, pagination) are **fully implemented**.

### Technical Success

| Outcome | Target |
|---|---|
| POST p95 | &lt; 200ms |
| GET p95 | &lt; 500ms (up to ~100k rows, indexed) |
| API accept → row in DB | &lt; 5s typical |
| Angular initial bundle | &lt; 500kB |
| .NET build | 0 errors, 0 warnings |
| `/health` | `Healthy` when DB reachable |

**Scope and phasing:** MVP through future vision, risk mitigations, and time-box cut order are in **Project Scoping & Phased Development** (below).

---

## User Journeys

### Journey 1 — Alex (backend engineer, happy path)

Alex verifies checkout instrumentation: filters `Purchase` for yesterday, sees data, submits a synthetic `PageView`, sees **live** row appear without refresh — end-to-end confidence in minutes.

**Capabilities:** Form feedback, filtered GET, real-time push, success/error UX.

### Journey 2 — Maria (QA, edge cases)

Maria triggers validation errors, length limits, offline API — always gets **clear messages** and **preserved form data** on failure.

**Capabilities:** Reactive validation, character limit UX, disabled/loading submit, resilient error display.

### Journey 3 — Dmitri (ops, incident)

Volume drops; Dmitri correlates UI gap with queue/Function via portal tooling, recovers producer, uses **date filters** and **`/health`** to confirm recovery timeline.

**Capabilities:** Date-range filter, health endpoint, telemetry and structured logs (Azure-side).

### Journey 4 — Priya (integrator, API)

Priya uses **interactive API docs** to exercise POST/GET and pagination envelope without waiting on the API team.

**Capabilities:** Documented contract, `PagedResult`-style pagination metadata.

### Journey → capability map

| Journey | Capabilities |
|---|---|
| Alex | Filter + sort + form + live updates |
| Maria | Validation, limits, errors, no data loss on failure |
| Dmitri | Date filter, health, observability hooks |
| Priya | OpenAPI-backed docs, stable REST contract |

---

## Full-Stack Web App Specific Requirements

**Shape:** SPA (Angular 20 standalone, OnPush, Signals), single dashboard view, reactive forms, modern control-flow syntax. **Backend:** REST API + isolated Functions app in one repo, separate deploy units. **Browsers:** Evergreen desktop (Chrome/Firefox/Edge/Safari current). **SEO:** Not required (internal tool).

**Real-time:** Persistent connection at app start; server pushes **EventCreated**; client reconnects with backoff. **CORS:** Configured for explicit origins with **credentials** where required for the real-time transport.

**Performance (UI):** Initial bundle &lt; 500kB; TTI target ~2s on a typical dev machine. **API surface:** `POST`/`GET` `/api/events`, SignalR hub `/hubs/events`, `/health`, dev-only `/openapi/v1.json` and `/scalar/v1`.

**Accessibility:** WCAG 2.1 AA baseline for form + table (semantics, keyboard, contrast); full formal audit out of scope for v1.

---

## Project Scoping & Phased Development

### MVP strategy

**Experience MVP:** Ship the full intended reference in one phase — credible only if all four journeys work. “Bonus” items (live updates, IaC, CI/CD, telemetry) are **in v1** because they define production-grade reference value.

**Constraints:** ~8 hours, one full-stack engineer. **Cut order if needed:** (1) Bicep, (2) GitHub Actions, (3) App Insights, (4) SignalR — **never** the core pipeline (form → API → queue → worker → DB → table).

### Phase 1 — v1.0 (must ship)

| Capability | Why |
|---|---|
| Event form + validation | Entry point |
| POST → queue → Function → SQL | Decoupled write path |
| GET + filter + sort + page | Debug + integration |
| Live push on create | “Watch events” promise |
| `/health` + App Insights | Ops confidence |
| Scalar (or equivalent) API UI in dev | Self-service contract |
| Bicep + GitHub Actions | One-command / push-to-main deploy story |
| README | &lt; 30 min local onboarding |

### Phase 2 — growth

Auth / tenant isolation, pipeline visibility in UI (depth/lag), bulk POST, retention policies, CSV export.

### Phase 3 — vision

Custom schemas, stronger multi-tenancy, aggregations/funnels, outbound webhooks, mobile-friendly layout.

### Risks

- **Local Azure messaging:** Real namespace + secrets in local config; never commit keys.
- **SignalR + CORS:** Strict origin + credentials; document in project context.

---

## Functional Requirements

### Event submission

- **FR-01:** Engineers submit events via web form (user ID, type, description).
- **FR-02–FR-04:** System enforces presence and max lengths (user ID ≤256, description ≤2000) and allowed types (PageView, Click, Purchase).
- **FR-05–FR-10:** Inline validation, character hinting, loading state, success/error feedback, form reset, **no data loss** on failed submit.

### Event pipeline

- **FR-11:** System assigns unique ID and UTC timestamp per accepted event.
- **FR-12:** API enqueues without waiting for durable persistence before response.
- **FR-13–FR-15:** Worker persists from queue; retries to limit; dead-letter path; structured success logs with event id.

### Event discovery

- **FR-16–FR-23:** Paginated list, default newest-first, filter by user (partial), type (exact), date range; sort by time/user/type; page size and navigation; pagination metadata.
- **FR-24–FR-25:** Loading skeleton and explicit empty state.

### Real-time monitoring

- **FR-26–FR-28:** New events appear without full page reload; connection at startup; automatic reconnect after loss.

### API & integration

- **FR-29–FR-32:** Programmatic POST/GET with same query model as UI; validation problem details; interactive API documentation in development.

### Observability & health

- **FR-33–FR-35:** Health reflects DB reachability; centralized telemetry for API and worker; error logs with investigable context.

### Infrastructure & deployment

- **FR-36–FR-39:** One-shot IaC deploy story; CI/CD on `main` for API/worker and SPA; local run documented with README + secrets.

---

## Non-Functional Requirements

### Performance

- **NFR-P01:** POST p95 &lt; 200ms under normal load (excluding client network).
- **NFR-P02:** GET p95 &lt; 500ms up to ~100k rows with appropriate indexes.
- **NFR-P03:** Accept → durable row typically &lt; 5s.
- **NFR-P04:** Production SPA initial JS within agreed bundle budget (&lt; 500kB).
- **NFR-P05:** Filter inputs debounce before API calls (e.g. ~400ms) to limit churn.

### Reliability

- **NFR-R01:** At-least-once delivery; idempotent persistence by primary key.
- **NFR-R02:** Dead-letter after max deliveries (e.g. 10).
- **NFR-R03:** API errors: safe client response + structured server log.

### Security

- **NFR-S01:** HTTPS in production.
- **NFR-S02:** Restricted CORS origins; compatible with credential-based browser features used by real-time stack.
- **NFR-S03:** DB TLS; no secrets in repo.
- **NFR-S04:** v1 has **no** end-user auth — perimeter or future phase owns access control.

### Maintainability & operability

- **NFR-M01:** Enforced layering (domain, infrastructure, hosts).
- **NFR-M02:** Versioned schema migrations.
- **NFR-M03:** Public operations documented in dev API UI.

### Accessibility

- **NFR-A01:** WCAG 2.1 AA baseline for core flows; formal audit out of scope for v1.

**Omitted:** Hyperscale SLOs, formal compliance programs (SOC2, etc.), multi-region DR — not in v1 scope.
