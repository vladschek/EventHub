# Story 8.1: Bicep modules for App Service, SQL, Service Bus, Function, Insights

Status: done

> **BMAD note:** Retrospective story file.

## Story

As **DevOps**,  
I want **repeatable infrastructure as code**,  
So that **environments are consistent and deployable**.

## Acceptance Criteria

1. `az deployment group create` against `infrastructure/main.bicep` provisions SQL, Service Bus (queue `events`), API, Functions, and monitoring per architecture (FR-36).

## Tasks / Subtasks

- [x] `infrastructure/main.bicep` orchestrates modules under `infrastructure/modules/`.
- [x] Modules present: e.g. `sql`, `servicebus`, `appservice-api`, `functionapp`, `storage`, `loganalytics` (as in repo).
- [x] Parameters/outputs for environment naming and downstream configuration.

## Dev Notes

- CI validates templates with `bicep build infrastructure/main.bicep`.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 8.1]

## Dev Agent Record

### File List

- `infrastructure/main.bicep`
- `infrastructure/modules/*.bicep`
