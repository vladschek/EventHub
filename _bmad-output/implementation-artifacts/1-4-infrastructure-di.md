# Story 1.4: Infrastructure DI and Service Bus client wiring

Status: done

> **BMAD note:** Retrospective story file.

## Story

As a **developer**,  
I want **`AddEventHubInfrastructure` / `AddEventHubInfrastructureForFunctions` registering DbContext, repository, and Service Bus publisher**,  
So that **API and Functions resolve the same abstractions**.

## Acceptance Criteria

1. **Given** valid connection strings in configuration **When** API and Functions hosts start **Then** `IEventRepository` and `IEventPublisher` resolve without runtime errors.
2. **And** queue name `events` matches architecture (`ServiceBusOptions.EventsQueueName`).

## Tasks / Subtasks

- [x] `DependencyInjection.cs`: `AddDbContextFactory<AppDbContext>`, `IEventRepository` → `EventRepository`, `ServiceBusClient` + `IEventPublisher`.
- [x] `ServiceBusEventPublisher` serializes `EventDto` JSON to queue `events`.
- [x] `NoOpEventPublisher` + `ServiceBus:DisablePublishing` for local API without a real namespace.
- [x] Wire `Program.cs` in Api and Functions.

## Dev Notes

- Functions trigger uses separate app setting `ServiceBusConnection` (added in Epic 3 local.settings pattern).

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 1.4]

## Dev Agent Record

### File List

- `backend/EventHub.Infrastructure/DependencyInjection.cs`
- `backend/EventHub.Infrastructure/Repositories/EventRepository.cs`
- `backend/EventHub.Infrastructure/Messaging/ServiceBusEventPublisher.cs`
- `backend/EventHub.Infrastructure/Messaging/NoOpEventPublisher.cs`
- `backend/EventHub.Infrastructure/Messaging/ServiceBusOptions.cs`
- `backend/EventHub.Api/Program.cs` (initial wire-up)
- `backend/EventHub.Functions/Program.cs`
