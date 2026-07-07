# Future Development Phases

Per `PROJECT_SPEC.md`: each phase must be fully completed, tested, and documented before the next begins.

- **Phase 1 — Foundation** ✅ — Clean Architecture solution, all entities, EF Core migration + seed data, Swagger/health/versioning/logging/CORS/JWT-config/Identity-config, frontend scaffold with routing/layouts/placeholder pages, baseline tests.
- **Phase 2 — Authentication** ✅ — Register, login, logout, refresh tokens, forgot/reset/change password, profile update, JWT issuance, role- and policy-based authorization, activity logging; frontend auth context, `useAuth`, real `ProtectedRoute` guard, Axios refresh-token interceptor, persistent login. See `PHASE2_AUTHENTICATION.md`.
- **Phase 3 — Ticket Management** ✅ — Create/Get/Update/Soft Delete/Restore, search/filter/sort/pagination, automatic ticket history; frontend Ticket List/Details/Create/Edit with RHF+Zod+TanStack Query. See `PHASE3_TICKET_MANAGEMENT.md`.
- **Phase 4 — Ticket Workflow** ✅ *(this phase)* — Manual/round-robin assignment with dedicated history, public comments and Agent+-only internal notes, @mentions, in-app notifications (replacing `NoOpNotificationService`) with an email stub; frontend Assignment Panel, Comments/Internal Notes, mention autocomplete + highlighting, Notification Center, unified ticket timeline. See `PHASE4_TICKET_WORKFLOW.md`.
- **Phase 5 — Dashboards & Reporting** ✅ *(this phase)* — KPI summary, category/priority breakdowns, monthly trend, resolution time, SLA report (measured against each ticket's own due date); PDF (QuestPDF) and Excel (ClosedXML) export of the same data; frontend Dashboard (KPI cards, pie/line/bar charts, SLA section) and Reports page. See `PHASE5_DASHBOARDS_REPORTING.md`.
- **Phase 6 — Administration** *(next)* — User/role management UI, lookup-data (category/priority/status) CRUD management UI, ticket attachments.
- **(Later)** — Deployment hardening: real SQL Server (not LocalDB), secrets via a vault/CI secret store instead of `dotnet user-secrets`, production CORS/JWT configuration review, hash refresh tokens at rest, real email provider instead of `LoggingEmailSender`, ticket number generation via a DB sequence (race-condition-safe), real-time notification delivery (SignalR/WebSockets) instead of polling, configurable per-priority SLA policies.

Nothing beyond Phase 5 is implemented yet.
