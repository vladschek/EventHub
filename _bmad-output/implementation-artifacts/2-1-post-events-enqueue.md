# Story 2.1: POST /api/events with validation and enqueue

Status: done

> **BMAD note:** Retrospective story file.

## Story

As an **integrator**,  
I want **to create an event via HTTP POST**,  
So that **events enter the pipeline without blocking on the database**.

## Acceptance Criteria

1. **Given** a valid `CreateEventDto` JSON body **When** `POST /api/events` is called **Then** response is `201` with `EventDto` including new `id` and `createdAt` UTC **And** a message is published to the `events` queue **And** FR-12: no SQL on POST path.
2. **Given** an invalid body **When** POST is called **Then** `400` with validation problem details (FR-31).

## Tasks / Subtasks

- [x] `EventsController` POST action; inject `IEventPublisher`.
- [x] JSON camelCase + `JsonStringEnumConverter` in API.
- [x] `Microsoft.AspNetCore.OpenApi` + Scalar in Development (`Program.cs`).
- [x] `EventHub.Api` project reference to `EventHub.Core`.
- [x] `CreatedAtAction` to named `GetById` route (added with Epic 4.1).

## Dev Notes

- `ServiceBus:DisablePublishing` in Development avoids 500 without real Service Bus.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Epic 2, Story 2.1]

## Dev Agent Record

### File List

- `backend/EventHub.Api/Controllers/EventsController.cs` (POST; later extended)
- `backend/EventHub.Api/Program.cs`
- `backend/EventHub.Api/EventHub.Api.csproj` (OpenAPI, Scalar)
- `backend/EventHub.Api/appsettings.json`, `appsettings.Development.json`
