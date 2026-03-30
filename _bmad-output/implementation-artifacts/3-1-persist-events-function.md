# Story 3.1: Service Bus–triggered function persists events

Status: done

> **BMAD note:** Retrospective story file.

## Story

As **operations**,  
I want **queue messages to become database rows automatically**,  
So that **events are durable after the API returns**.

## Acceptance Criteria

1. **Given** valid JSON on `events` **When** the Function runs **Then** row inserted with same `Id` (idempotent PK) **And** structured log includes event id (FR-15).
2. **Given** malformed JSON **When** the Function runs **Then** log + rethrow for broker retry/DLQ (FR-14).

## Tasks / Subtasks

- [x] Package `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus`.
- [x] `PersistEventsFunction` with `[ServiceBusTrigger("events", Connection = "ServiceBusConnection")]`.
- [x] Deserialize `EventDto`; validate non-empty id/userId/description; map to `Event`.
- [x] `IEventRepository.AddAsync` returns `false` on SQL duplicate key (2601/2627); function logs idempotent path.
- [x] `local.settings.json`: `ServiceBusConnection` + SQL + `ServiceBus:ConnectionString`.
- [x] `EventHub.Core` reference from Functions project.

## Dev Notes

- Binding connection name `ServiceBusConnection` is required by the trigger attribute.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Epic 3, Story 3.1]
- [Source: `backend/EventHub.Core/Abstractions/IEventRepository.cs` — `Task<bool> AddAsync`]

## Dev Agent Record

### File List

- `backend/EventHub.Functions/PersistEventsFunction.cs`
- `backend/EventHub.Functions/EventHub.Functions.csproj`
- `backend/EventHub.Functions/local.settings.json`
- `backend/EventHub.Infrastructure/Repositories/EventRepository.cs` (idempotent AddAsync)
