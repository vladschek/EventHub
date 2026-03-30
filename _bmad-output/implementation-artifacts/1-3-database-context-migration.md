# Story 1.3: Database context and initial migration

Status: done

> **BMAD note:** Retrospective story file.

## Story

As a **developer**,  
I want **`AppDbContext` and an EF Core migration for `Events`**,  
So that **local and Azure SQL schemas stay versioned**.

## Acceptance Criteria

1. **Given** connection string configuration **When** `dotnet ef database update` runs from documented commands **Then** the `Events` table exists with indexes for filter/sort columns.
2. **And** migration files live under `EventHub.Infrastructure/Data/Migrations`.

## Tasks / Subtasks

- [x] Add EF Core 10 packages to Infrastructure; `AppDbContext` + fluent config for `Events`.
- [x] `EventType` stored as string; indexes on `CreatedAt`, `UserId`, `Type`.
- [x] `IDesignTimeDbContextFactory` for tooling; optional `EVENTHUB_SQL` env override.
- [x] Initial migration `InitialCreate` under `Data/Migrations`.

## Dev Notes

- Commands documented in root `README.md` (Epic 8.3).

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 1.3]
- [Source: `_bmad-output/planning-artifacts/architecture.md` — EF migrations path]

## Dev Agent Record

### File List

- `backend/EventHub.Infrastructure/Data/AppDbContext.cs`
- `backend/EventHub.Infrastructure/Data/AppDbContextFactory.cs`
- `backend/EventHub.Infrastructure/Data/Migrations/20260327143007_InitialCreate.cs`
- `backend/EventHub.Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs`
- `backend/EventHub.Infrastructure/EventHub.Infrastructure.csproj` (EF packages)
- `backend/EventHub.Api/EventHub.Api.csproj` (EF Design for startup project)
