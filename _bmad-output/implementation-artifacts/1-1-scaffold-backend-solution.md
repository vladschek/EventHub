# Story 1.1: Scaffold backend solution and project references

Status: done

> **BMAD note:** Story file created retrospectively to document work already merged; aligns `sprint-status.yaml` with implementation artifacts.

## Story

As a **developer**,  
I want **a multi-project .NET solution matching the architecture doc**,  
So that **all later stories plug into a consistent structure**.

## Acceptance Criteria

1. **Given** a clean `backend/` folder **When** the solution and projects are created per architecture (Core, Infrastructure, Api, Functions) with correct references **Then** `dotnet build` succeeds on all projects.
2. **And** `global.json` pins .NET 10 SDK if used by the team.

## Tasks / Subtasks

- [x] Create `backend/EventHub.sln` (classic `.sln`; SDK default `.slnx` was replaced for doc parity).
- [x] Add projects: `EventHub.Core`, `EventHub.Infrastructure`, `EventHub.Api`, `EventHub.Functions` (isolated worker, .NET 10).
- [x] Wire references: Infrastructure → Core; Api → Infrastructure; Functions → Infrastructure.
- [x] Add `backend/global.json` with SDK `10.0.201` and `rollForward`.
- [x] Install Azure Functions worker templates (`Microsoft.Azure.Functions.Worker.ProjectTemplates`) where needed for `dotnet new func`.

## Dev Notes

- Architecture: [Source: `_bmad-output/planning-artifacts/architecture.md` — §5 Project structure, §2 Initialization]
- Queue name and layering enforced in later stories; this story is structure-only.

### Project Structure Notes

- Matches target tree: `backend/EventHub.sln`, four projects under `backend/`.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Epic 1, Story 1.1]
- [Source: `_bmad-output/planning-artifacts/architecture.md` — Repository tree]

## Dev Agent Record

### Completion Notes List

- Retrospective documentation; implementation verified via `dotnet build backend/EventHub.sln`.

### File List

- `backend/EventHub.sln`
- `backend/global.json`
- `backend/EventHub.Core/EventHub.Core.csproj`
- `backend/EventHub.Infrastructure/EventHub.Infrastructure.csproj`
- `backend/EventHub.Api/EventHub.Api.csproj`
- `backend/EventHub.Functions/EventHub.Functions.csproj`
