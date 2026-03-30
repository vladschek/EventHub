# Story 4.1: GET /api/events with filter, sort, and pagination

Status: done

> **BMAD note:** Retrospective story file.

## Story

As an **engineer**,  
I want **to query events with filters and paging**,  
So that **I can narrow down incidents quickly**.

## Acceptance Criteria

1. **Given** existing events **When** `GET /api/events` is called with optional `userId`, `type`, `fromDate`, `toDate`, `sortBy`, `sortDescending`, `page`, `pageSize` **Then** response is `PagedResult<EventDto>` with correct metadata (FR-16–FR-23).
2. **And** queries use indexed columns and `AsNoTracking` for reads.

## Tasks / Subtasks

- [x] `ListEventsQuery` + `ToFilter()` → `EventFilterDto`.
- [x] `EventsController.List` injects `IEventRepository`.
- [x] `GetById` with `[HttpGet("{id:guid}", Name = nameof(GetById))]` for `CreatedAtAction` from POST.

## Dev Notes

- Enum query values: `CreatedAt`, `UserId`, `Type` (PascalCase) to match ASP.NET model binding.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 4.1]

## Dev Agent Record

### File List

- `backend/EventHub.Api/Contracts/ListEventsQuery.cs`
- `backend/EventHub.Api/Controllers/EventsController.cs` (GET list + GetById)
