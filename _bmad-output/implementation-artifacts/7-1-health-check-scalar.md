# Story 7.1: Health check and development API documentation

Status: done

> **BMAD note:** Retrospective story file.

## Story

As **operations**,  
I want **liveness/readiness and interactive API docs**,  
So that **deployments and onboarding stay straightforward**.

## Acceptance Criteria

1. `GET /health` reflects database connectivity (FR-33); in Development, Scalar and OpenAPI JSON expose POST/GET contracts and models (FR-32).

## Tasks / Subtasks

- [x] `AddHealthChecks()` + custom `DatabaseHealthCheck` (EF connectivity).
- [x] `MapHealthChecks("/health", …)` — 200 when healthy/degraded, 503 when unhealthy.
- [x] Development: `MapOpenApi` + `MapScalarApiReference` (`Scalar.AspNetCore`).

## Dev Notes

- Scalar consumes generated OpenAPI; no Swagger UI in this stack.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 7.1]
- [Source: `_bmad-output/planning-artifacts/architecture.md`]

## Dev Agent Record

### File List

- `backend/EventHub.Api/Program.cs`
- `backend/EventHub.Api/EventHub.Api.csproj`
- `backend/EventHub.Api/Health/DatabaseHealthCheck.cs`
