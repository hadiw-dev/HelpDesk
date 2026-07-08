# Release Notes

## v1.0.0 — Initial production-ready release

The full 8-phase build: an IT Help Desk & Ticketing Management System with Clean Architecture on the backend (.NET 8), a Vite/React/TypeScript frontend, and a containerized deployment.

### Phase 1 — Foundation
Clean Architecture solution (Domain/Application/Infrastructure/Api), EF Core + SQL Server LocalDB, ASP.NET Identity (configuration only), Serilog, Swagger, API versioning, global exception handling, CORS, health checks. Vite + React + TypeScript frontend scaffold with routing, layouts, and placeholder pages. First EF Core migration and seed data (Categories, Priorities, Statuses, Identity roles).

### Phase 2 — Authentication
JWT-based auth: register, login, logout, refresh tokens, forgot/reset/change password, profile management. Role- and policy-based authorization. Frontend auth context, protected routes, Axios refresh-token interceptor.

### Phase 3 — Ticket Management
Full ticket CRUD with soft delete/restore, search/filter/sort/pagination, automatic history tracking. Frontend ticket list, details, create, and edit pages.

### Phase 4 — Ticket Workflow
Manual and round-robin assignment with history, public comments and Agent-only internal notes, @mentions, in-app notifications. Assignment panel, comments/notes UI, mention autocomplete, notification center, unified ticket timeline.

### Phase 5 — Dashboards & Reporting
KPI summary, category/priority breakdowns, monthly trend, resolution time, SLA compliance (measured against each ticket's own due date). PDF (QuestPDF) and Excel (ClosedXML) export. Dashboard and Reports pages with Recharts visualizations.

### Phase 6 — Administration
Secure file upload/download (storage outside the web root, extension/size validation), Admin APIs for Users, Roles, Categories, Priorities, Statuses, Activity Logs, and System Settings. Full admin frontend: user management, lookup management, activity log viewer, system settings.

### Phase 7 — Hardening
Security response headers + HSTS, fail-fast JWT secret validation, failed-login audit logging, magic-byte upload content validation. Test suite expanded to 149 tests (114 unit + 35 integration/API — auth flow, authorization, CORS/SQL-injection/XSS checks, full ticket CRUD). Frontend `ErrorBoundary`, route-based code splitting, accessibility labeling pass. Complete Postman collection, testing report, coverage report.

### Phase 8 — Production Readiness
Dockerfiles for API and frontend (multi-stage builds, non-root runtime user, container health checks), `docker-compose.yml` orchestrating API + frontend + SQL Server with a persistent-volume, health-check-gated startup sequence. Swagger now available in every environment (not just Development) and exported as a static `swagger.json`. Full documentation set: Setup Guide, Deployment Guide, User Guide, Deployment Checklist, Production Checklist, Presentation Summary — alongside the Architecture Guide, API Guide, and ER diagram already produced in earlier phases.

## Known limitations (see `docs/PRODUCTION_CHECKLIST.md` for the full list)

- Refresh tokens stored as plaintext at rest (not hashed)
- No rate-limiting beyond ASP.NET Identity's built-in account lockout
- Email sending is a logging stub (`LoggingEmailSender`), not a real provider
- SQL Server Express edition in the Docker Compose stack (10GB/limited-memory caps) — fine for a demo, not for real production ticket volume
- No CI/CD pipeline defined
- No automated frontend test suite (Vitest/Testing Library)

## Upgrade notes

This is the first tagged release — no upgrade path from a prior version applies. Fresh installs: follow `docs/SETUP_GUIDE.md` (native) or `docs/DEPLOYMENT_GUIDE.md` (Docker Compose).
