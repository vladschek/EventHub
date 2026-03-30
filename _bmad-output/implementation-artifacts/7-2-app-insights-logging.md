# Story 7.2: Application Insights and structured error logging

Status: done

> **BMAD note:** Retrospective story file.

## Story

As **operations**,  
I want **telemetry and rich logs on API and Functions**,  
So that **incidents are diagnosable**.

## Acceptance Criteria

1. With instrumentation connection strings configured, API and Functions emit Application Insights telemetry (FR-34); unhandled failures log structured context and production clients do not receive raw stack traces (`UseExceptionHandler`, FR-35, NFR-R03).

## Tasks / Subtasks

- [x] API: `AddApplicationInsightsTelemetry` when `APPLICATIONINSIGHTS_CONNECTION_STRING` or `ApplicationInsights:ConnectionString` is set; warning log when unset.
- [x] Functions: `AddApplicationInsightsTelemetryWorkerService` + `ConfigureFunctionsApplicationInsights`.
- [x] Persist function: structured `ILogger` entries on success and malformed payload (supports FR-15 / operations traceability).

## Dev Notes

- Local dev runs without Insights until a connection string is provided.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 7.2]

## Dev Agent Record

### File List

- `backend/EventHub.Api/Program.cs`
- `backend/EventHub.Functions/Program.cs`
- `backend/EventHub.Functions/PersistEventsFunction.cs`
