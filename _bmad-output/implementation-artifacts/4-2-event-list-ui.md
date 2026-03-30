# Story 4.2: Event list UI with filters, sort, pagination, skeleton, empty state

Status: done

> **BMAD note:** Retrospective story file.

## Story

As an **engineer**,  
I want **a table UI wired to the GET API**,  
So that **I can explore history without Postman**.

## Acceptance Criteria

1. Skeleton during load (FR-24); debounced userId filter (NFR-P05); empty state (FR-25); semantic table + keyboard-friendly controls (NFR-A01).

## Tasks / Subtasks

- [x] Angular app scaffold (`frontend/`); `EventApiService.list()`.
- [x] `EventListComponent`: signals, `effect` + `allowSignalWrites`, debounced `userId`, `refreshTick` placeholder for Epic 6.
- [x] Environments: `environment.development.ts` → `apiUrl` https://localhost:7176.
- [x] Routes: default `/events`; nav shell in `App`.
- [x] Template fixes: no `as string` in templates; `toSignal` import; `page.set(1)` on userId change.

## Dev Notes

- API CORS Dev policy includes `AllowCredentials` for later SignalR.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 4.2]
- [Source: `_bmad-output/planning-artifacts/architecture.md` — Angular patterns]

## Dev Agent Record

### File List

- `frontend/src/app/features/events/event-list/*`
- `frontend/src/app/core/services/event-api.service.ts`
- `frontend/src/app/core/models/event.models.ts`
- `frontend/src/app/app.routes.ts`, `app.ts`, `app.html`, `app.scss`
- `frontend/src/environments/*`
- `frontend/angular.json` (development fileReplacements)
