# Phase 5 — Dashboards & Reporting

## What this phase adds

Analytics over the ticket data built up in Phases 3–4, plus exportable PDF/Excel reports of the same data. No new entities or migration — everything is computed from `Ticket` fields that already existed (`CreatedAt`, `ResolvedAt`, `ClosedAt`, `DueDate`, `CategoryId`, `PriorityId`, `StatusId`).

### Backend

**`Features/Dashboard/`** — `IDashboardService` / `DashboardService`, six read-only endpoints under `GET /api/v1/dashboard/`:
- **`kpi-summary`** — total/open/in-progress/pending/resolved/closed counts, overdue count (`DueDate` in the past and not yet Resolved/Closed), unassigned count (no `AssignedToUserId` and not yet Closed), and average resolution time in hours.
- **`tickets-by-category`** / **`tickets-by-priority`** — grouped counts, ordered by each lookup's `DisplayOrder` (not alphabetically) — the same field `TicketService.ApplySorting` already relies on for priority/status ordering.
- **`monthly-tickets`** — tickets created and tickets resolved, grouped by calendar month, chronological.
- **`resolution-time`** — overall average resolution time plus a breakdown by priority.
- **`sla-report`** — see below.
- Every endpoint accepts an optional `DateFrom`/`DateTo` query filter (applied to `CreatedAt`) and is scoped exactly like ticket search: an Employee only sees metrics for tickets they created, Agent+ sees metrics across every ticket. Reused `TicketAccessGuard` from Phase 4 rather than re-deriving the rule a third time.

**SLA design decision**: rather than inventing a configurable per-priority SLA policy table (a real admin-phase feature this project doesn't have yet), SLA compliance is measured against each ticket's own `DueDate` — a field that already existed since Phase 1 and that ticket creators/agents already set per ticket. A ticket with no due date isn't "SLA-tracked" at all (there's nothing to measure it against). A tracked ticket is "met" if it was resolved/closed by its due date, or if it's still open and not yet past due; it's "breached" if it was resolved/closed late, or is still open and now past due. This is grounded entirely in data the system already collects, instead of fabricating a policy the project was never asked to build.

**`Features/Reports/`** — `IReportService` / `ReportService` renders the exact same six data sets `IDashboardService` produces into two downloadable formats:
- **PDF** via QuestPDF (Community license — free under $1M annual revenue, set via `QuestPDF.Settings.License = LicenseType.Community` in `Program.cs`; the test project has to set this too since it never runs `Program.cs`).
- **Excel** via ClosedXML (MIT licensed), one worksheet per data set (KPI Summary, Tickets by Category, Tickets by Priority, Monthly Tickets, Resolution Time, SLA Report).
- `GET /api/v1/dashboard/reports/pdf` and `GET /api/v1/dashboard/reports/excel` return the file directly (`Content-Disposition: attachment`), both honoring the same optional date-range filter as the data endpoints.

### Frontend

- **`DashboardPage`** (full rewrite of the Phase 1 placeholder) — an optional date-range filter, a KPI card grid, two pie charts (category/priority breakdown), a line chart (monthly created-vs-resolved trend), a bar chart (average resolution time by priority), and an SLA Dashboard section (compliance percentage + a table of breached tickets linking back to each ticket).
- **`ReportsPage`** (new) — the same date-range filter plus "Export PDF" / "Export Excel" buttons that fetch the file as a blob and trigger a browser download.
- Six new Recharts-backed components under `components/dashboard/`: `KpiCard`, `BreakdownPieChart` (reused for both category and priority), `MonthlyTicketsLineChart`, `ResolutionTimeBarChart`, `SlaDashboard`, `DateRangeFilter` (shared by both pages).
- New `/reports` route and Navbar link.

## Verification performed

- **67 backend tests pass** (64 unit + 3 integration), including **17 new tests**: `DashboardServiceTests` (KPI counts and averages, Employee-vs-Agent scoping, category/priority breakdown ordering by `DisplayOrder` rather than insertion order, monthly grouping across calendar months, resolution time overall/by-priority, SLA classification of met/breached/still-open-overdue/not-tracked tickets, date-range filtering) and `ReportServiceTests` (PDF output starts with the `%PDF` magic header; Excel output starts with the ZIP `PK` magic header and — reopened via ClosedXML — has all six expected worksheet names and correct cell values).
- **Live verification via `curl`** against the real API + LocalDB: every dashboard endpoint returns correct, real numbers computed from actual seeded/tested tickets; an Employee account's KPI summary is correctly scoped to only their own tickets (4 total) while an Agent account sees the whole system (9 total); the PDF export returns valid `%PDF-1.4` bytes with a correct `Content-Disposition` header; the Excel export returns a valid ZIP archive with the correct `.xlsx` content type.
- **Frontend**: `npm run build` (0 TypeScript errors) and `npm run lint` (0 errors, same 3 pre-existing benign fast-refresh warnings from earlier phases).
- **Not verified in a real browser**: as in Phase 4, no browser-automation tool was available in this environment, so the charts/date filter/export buttons were not clicked through visually — only compiled, linted, and exercised via their API layer.

## Known limitations (by design, for this phase)

- SLA compliance is measured against each ticket's own `DueDate`, not a configurable per-priority SLA policy — there's no admin UI yet to define target response/resolution windows by priority, and adding one would be an administration-phase concern.
- Monthly ticket data only returns months that actually have at least one ticket (no zero-filled gap months) — kept simple rather than synthesizing empty months across an arbitrary date range.
- Report exports render the *current* dashboard data snapshot; there's no report history/scheduling — this phase asked for on-demand PDF/Excel export, not a reporting pipeline.
