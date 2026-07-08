# Presentation Summary

A one-page overview of this project for a portfolio review, interview walkthrough, or stakeholder demo.

## What it is

An **IT Help Desk & Ticketing Management System** — employees submit IT support requests; IT Support Agents, Managers, and Admins triage, assign, resolve, and report on them through a centralized dashboard. Built end-to-end across 8 incremental phases, each fully tested and documented before the next began.

## Elevator pitch

> A production-shaped, containerized full-stack ticketing system demonstrating Clean Architecture, defense-in-depth security, and a 149-test automated safety net — built the way a real engineering team would build it, not a tutorial shortcut.

## Tech stack

| Layer | Technology |
|---|---|
| Backend | .NET 8, ASP.NET Core Web API, EF Core, ASP.NET Identity, JWT auth |
| Backend cross-cutting | AutoMapper, FluentValidation, Serilog, Swagger/OpenAPI, API versioning |
| Database | SQL Server (LocalDB for dev, containerized SQL Server in Docker Compose) |
| Reporting | QuestPDF (PDF export), ClosedXML (Excel export) |
| Frontend | React 18 + TypeScript, Vite, Tailwind CSS, shadcn/ui |
| Frontend data/forms | TanStack Query, React Hook Form + Zod, Axios |
| Charts | Recharts |
| Testing | xUnit, Moq, ASP.NET `WebApplicationFactory` integration tests |
| Deployment | Docker, Docker Compose, nginx (static frontend serving) |

## Architecture highlights

- **Clean Architecture** on the backend: `Domain` → `Application` → `Infrastructure` → `Api`, dependency direction enforced throughout (e.g. `IFormFile` never crosses into `Application`; a dedicated `UploadAttachmentRequest` DTO keeps ASP.NET Core types out of the use-case layer).
- **Generic services where entities are genuinely identical in shape** — one `AdminLookupService<TEntity>` backs Category/Priority/Status CRUD instead of three near-duplicate services.
- **Derived state over stored state** — round-robin assignment rotation is computed from the assignment history table itself, not a separate counter that could drift out of sync.
- **Soft delete + full audit trail** — every entity carries `CreatedAt/By`, `UpdatedAt/By`, and a non-destructive delete flag, enforced globally via EF Core query filters and a `SaveChanges` override, not per-service boilerplate.

## Security posture

- JWT auth with refresh-token rotation, fail-fast validation of the signing key at startup (rejects anything under 32 bytes)
- Role- **and** policy-based authorization on every endpoint, plus resource-ownership checks (an Employee cannot view another Employee's ticket, even with a valid token)
- Upload validation in three layers: extension allowlist, size limit, and magic-byte content verification (a renamed `.exe` masquerading as `.pdf` is rejected)
- Security response headers, HSTS, CORS allowlisting, parameterized queries throughout (no SQL injection surface), failed-login audit logging

## Quality metrics

- **149 automated tests** passing (114 unit + 35 integration/API), covering auth flow, authorization boundaries, CORS/SQL-injection/XSS defenses, and full ticket CRUD against a real database — not mocks
- **65.1% branch coverage / 81% method coverage** on the backend (full breakdown in `docs/PHASE7_COVERAGE_REPORT.md`, including an honest accounting of what's *not* covered and why)
- Complete **Postman collection** (every endpoint, auto-populating auth tokens/ids) and exported **Swagger/OpenAPI document**
- Zero ESLint errors, zero build warnings introduced by application code (one long-standing, documented, low-risk `NU1903` advisory on a pinned AutoMapper version)

## What a reviewer can verify in 5 minutes

```bash
git clone <repo> && cd HelpDeskSystem
cp .env.example .env   # set MSSQL_SA_PASSWORD and JWT_SECRET_KEY
docker compose up --build
# → http://localhost:3000 (app), http://localhost:5019/swagger (API docs)
```

Or, to see the test suite run: `cd backend && dotnet test HelpDesk.sln`.

## Documentation map

| Doc | Purpose |
|---|---|
| `README.md` | Project overview, features, tech stack, how to run |
| `docs/ARCHITECTURE.md` | Clean Architecture layering, frontend architecture, coding standards |
| `docs/database-design.md` / `docs/DATABASE.md` | Entity relationships, ER diagram, design decisions |
| `docs/api-guide.md` | Endpoint reference with example requests/responses |
| `docs/SETUP_GUIDE.md` | Native (non-Docker) local dev setup |
| `docs/DEPLOYMENT_GUIDE.md` | Docker Compose deployment, verification, troubleshooting |
| `docs/USER_GUIDE.md` | Feature walkthrough per role |
| `docs/PHASE1`–`PHASE8_*.md` | What each phase added and how it was verified, in order |
| `docs/PITFALLS.md` | Every non-obvious technical problem hit and how it was solved |
| `docs/PRODUCTION_CHECKLIST.md` | What changes before a real production launch |
| `RELEASE_NOTES.md` | Changelog by phase, this release's known limitations |

## Honest gaps (said out loud, not hidden)

This is a portfolio-scale project, not a claim of enterprise-grade completeness. `docs/PRODUCTION_CHECKLIST.md` lists what's deliberately deferred: secrets belong in a managed vault (not `.env`) before a real launch, refresh tokens should be hashed at rest, there's no CI/CD pipeline defined, and the frontend has no automated test suite yet. Naming these explicitly is itself part of the engineering practice this project is meant to demonstrate.
