# Story 5.1: Event creation form (reactive, accessible)

Status: done

> **BMAD note:** Retrospective story file.

## Story

As an **engineer**,  
I want **to submit events from the browser with clear validation**,  
So that **I can inject test traffic safely**.

## Acceptance Criteria

1. Inline validation without round-trip (FR-05); description character guidance (FR-06).
2. Loading/disabled submit (FR-07); success + reset (FR-08, FR-10); error without losing values (FR-09).

## Tasks / Subtasks

- [x] `EventFormComponent`: reactive forms, validators, `EventApiService.create()`.
- [x] Map server `400` `errors` to field `server` errors; `clearServerFieldErrors` on resubmit.
- [x] `queueMicrotask` for success banner after `reset` (valueChanges vs success race).
- [x] Route `/events/new`; nav link “New event”.
- [x] `provideHttpClient` in `app.config.ts`.

## Dev Notes

- `CreateEventDto` interface in `event.models.ts`.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 5.1]

## Dev Agent Record

### File List

- `frontend/src/app/features/events/event-form/*`
- `frontend/src/app/core/services/event-api.service.ts` (`create`)
- `frontend/src/app/app.routes.ts`
