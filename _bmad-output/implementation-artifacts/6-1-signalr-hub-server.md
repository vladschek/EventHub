# Story 6.1: SignalR hub and server push on create

Status: done

> **BMAD note:** Retrospective story file.

## Story

As an **engineer**,  
I want **the list to refresh when new events are accepted**,  
So that **I see near-real-time updates without manual refresh**.

## Acceptance Criteria

1. After a successful POST accept-and-enqueue path, connected clients receive `EventCreated` with the same payload shape as list items; hub route is `/hubs/events` (Story 6.1 in `planning-artifacts/epics.md`).

## Tasks / Subtasks

- [x] `AddSignalR()` + `MapHub<EventsHub>("/hubs/events")`.
- [x] `EventsHub` with `OnConnectedAsync` logging.
- [x] `IEventPublisher` implementation publishes to Service Bus **and** `IHubContext<EventsHub>.Clients.All.SendAsync("EventCreated", dto)`.
- [x] CORS: `AllowCredentials` + explicit origins in Development.

## Dev Notes

- Angular client in Story 6.2.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 6.1]

## Dev Agent Record

### File List

- `backend/EventHub.Api/Hubs/EventsHub.cs`
- `backend/EventHub.Api/Program.cs` (SignalR, CORS)
- `backend/EventHub.Infrastructure/Messaging/ServiceBusEventPublisher.cs` (hub broadcast)
