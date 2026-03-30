# Story 6.2: Angular SignalR client and list refresh

Status: done

> **BMAD note:** Retrospective story file.

## Story

As an **engineer**,  
I want **the event list to subscribe to the hub**,  
So that **new events appear automatically**.

## Acceptance Criteria

1. On app load, SignalR connects and `EventCreated` refreshes or prepends the list (FR-26, FR-27); `withAutomaticReconnect` covers drops (FR-28); CORS allows credentials from the configured Angular origin (NFR-S02).

## Tasks / Subtasks

- [x] `@microsoft/signalr` dependency.
- [x] `EventsRealtimeService`: `HubConnectionBuilder` with `${apiUrl}/hubs/events`, `withCredentials`, WebSockets | SSE | LongPolling, `withAutomaticReconnect`, `EventCreated` handler → `eventCreated$`.
- [x] Root `App` component constructor calls `realtime.start()` so the hub connects at startup.
- [x] `EventListComponent` subscribes to `eventCreated$` and merges new events (dedupe by `id`).

## Dev Notes

- Hub URL is built from `environment.apiUrl` plus `/hubs/events`.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 6.2]

## Dev Agent Record

### File List

- `frontend/src/app/core/services/events-realtime.service.ts`
- `frontend/src/app/features/events/event-list/event-list.ts`
- `frontend/src/app/app.ts` (starts hub in constructor)
- `frontend/package.json`
