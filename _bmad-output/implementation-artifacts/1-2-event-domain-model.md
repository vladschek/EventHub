# Story 1.2: Event domain model and API contracts

Status: done

> **BMAD note:** Retrospective story file.

## Story

As a **developer**,  
I want **`Event`, `EventType`, DTOs, and repository/publisher interfaces in Core**,  
So that **API and Infrastructure share one contract**.

## Acceptance Criteria

1. **Given** `EventHub.Core` **When** models and DTOs are implemented with validation attributes matching the PRD limits **Then** `CreateEventDto`, `EventDto`, `EventFilterDto`, and `PagedResult<T>` are usable by API and tests.
2. **And** `EventType` maps to stored string values agreed in architecture.

## Tasks / Subtasks

- [x] Add `Entities/Event`, `Entities/EventType` (enum → string in SQL later).
- [x] Add DTOs: `CreateEventDto`, `EventDto`, `EventFilterDto`, `EventSortField`, `PagedResult<T>`.
- [x] Add `IEventRepository`, `IEventPublisher` in `Abstractions/`.
- [x] PRD limits: userId 256, description 2000; filter page/pageSize bounds.

## Dev Notes

- `IEventRepository.AddAsync` later extended to `Task<bool>` for idempotency (Epic 3); document cross-story change in Epic 3 story file.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 1.2]
- [Source: `_bmad-output/planning-artifacts/prd.md` — Event submission limits]

## Dev Agent Record

### File List

- `backend/EventHub.Core/Entities/Event.cs`
- `backend/EventHub.Core/Entities/EventType.cs`
- `backend/EventHub.Core/DTOs/*.cs`
- `backend/EventHub.Core/Abstractions/IEventRepository.cs`
- `backend/EventHub.Core/Abstractions/IEventPublisher.cs`
