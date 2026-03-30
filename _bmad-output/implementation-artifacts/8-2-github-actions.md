# Story 8.2: GitHub Actions build and deploy

Status: done

> **BMAD note:** Retrospective story file.

## Story

As **DevOps**,  
I want **CI/CD for API, Functions, and frontend**,  
So that **main branch stays deployable**.

## Acceptance Criteria

1. On pushes to `main` (and PRs), workflows run restore/build steps for touched areas; secrets documented for any future deploy jobs (FR-37, FR-38).

## Tasks / Subtasks

- [x] `ci.yml`: job `bicep` — install Bicep CLI, `bicep build infrastructure/main.bicep`.
- [x] Job `backend`: `dotnet build backend/EventHub.sln -c Release` on .NET 10.
- [x] Job `frontend`: `npm ci` + `npm run build` in `frontend/` (Node 22).

## Dev Notes

- Current pipeline is **CI** (build/validate); add deploy jobs separately if you extend to full CD.

### References

- [Source: `_bmad-output/planning-artifacts/epics.md` — Story 8.2]

## Dev Agent Record

### File List

- `.github/workflows/ci.yml`
