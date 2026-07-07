# IT Help Desk & Ticketing Management System

A full-stack enterprise IT Help Desk & Ticketing Management System built with a .NET 8 Clean Architecture backend and a React + TypeScript frontend. Built incrementally, phase by phase, per [`PROJECT_SPEC.md`](PROJECT_SPEC.md).

## Project Overview

Organizations need a reliable way for employees to report IT problems and for support staff to track, prioritize, and resolve them without things falling through the cracks. This system provides that: employees submit tickets, IT Support Agents and Managers triage and work them through a defined status workflow, and Admins/Managers get visibility into team performance through a dashboard and exportable reports.

**Target users:**
- **Employees** — submit and track their own support requests
- **IT Support Agents** — pick up, work, and resolve tickets assigned to them
- **Managers** — oversee team workload, reassign tickets, monitor SLA compliance
- **Admins** — full system oversight

**Business purpose:** replace ad-hoc email/chat-based IT support with a structured, auditable ticketing workflow — every status change, assignment, and comment is tracked in a permanent history, and managers get real-time KPI/SLA visibility instead of having to ask around for status updates.

## Main Features

### Authentication
- JWT-based authentication (access + refresh tokens)
- Role-based authorization (Admin, Manager, IT Support Agent, Employee)
- Registration, login, logout, forgot/reset/change password, profile management
- Persistent login with automatic token refresh

### Ticket Management
- Ticket creation with category, priority, and due date
- Full CRUD with soft delete/restore
- Search, filter (category/priority/status/assignee), sort, and pagination
- Automatic, immutable ticket history — every field change is recorded

### Workflow
- Manual assignment, reassignment, and round-robin auto-assignment
- Dedicated assignment history (who assigned what, to whom, when)
- Public comments and Agent-only internal notes
- @Mentions with highlighted rendering
- In-app notifications (assignment, comments, mentions) with an email stub
- Unified ticket timeline merging history, assignments, and comments

### Dashboard
- KPI summary cards (total/open/in-progress/resolved/closed/overdue/unassigned, average resolution time)
- Pie charts (tickets by category, tickets by priority)
- Line chart (monthly ticket volume — created vs. resolved)
- Bar chart (resolution time by priority)
- SLA compliance dashboard with a breached-ticket table
- PDF and Excel report export of the same data

## Technology Stack

**Frontend**
- React 19 + TypeScript
- Vite
- Tailwind CSS v4
- shadcn/ui
- TanStack Query
- Axios
- React Hook Form + Zod
- Recharts

**Backend**
- ASP.NET Core 8 Web API
- Clean Architecture (Domain / Application / Infrastructure / Api)
- Entity Framework Core 8
- ASP.NET Identity
- JWT Bearer authentication
- FluentValidation, AutoMapper, Serilog
- Swagger / OpenAPI
- QuestPDF (PDF reports), ClosedXML (Excel reports)

**Database**
- SQL Server (LocalDB for local development)

**Testing**
- xUnit + Moq (unit tests)
- `WebApplicationFactory`-based integration tests

## Architecture

The backend follows Clean Architecture: dependencies only ever point inward, toward `Domain`.

```mermaid
flowchart TD
    subgraph Frontend["Frontend (React + TypeScript)"]
        Pages["pages/"] --> Features["features/ (api + queries)"]
        Components["components/"] --> Features
        Features --> Axios["Axios instance"]
    end

    Axios -->|HTTPS / JWT| Api

    subgraph Backend["Backend (.NET 8 Clean Architecture)"]
        Api["HelpDesk.Api\n(Controllers, Middleware, DI composition)"]
        Application["HelpDesk.Application\n(DTOs, service interfaces, validators)"]
        Infrastructure["HelpDesk.Infrastructure\n(EF Core, service implementations)"]
        Domain["HelpDesk.Domain\n(Entities, enums)"]

        Api --> Application
        Api --> Infrastructure
        Infrastructure --> Application
        Application --> Domain
        Infrastructure --> Domain
    end

    Infrastructure -->|EF Core| DB[("SQL Server")]
```

- **`HelpDesk.Domain`** — entities and enums only, no dependencies on any other layer.
- **`HelpDesk.Application`** — DTOs, service interfaces, FluentValidation validators, AutoMapper profiles. Depends only on `Domain`.
- **`HelpDesk.Infrastructure`** — EF Core `DbContext`, service implementations, external integrations (email stub, PDF/Excel generation). Depends on `Application` + `Domain`.
- **`HelpDesk.Api`** — controllers, middleware, DI composition root. Depends on all layers.

**Frontend structure:**
- `components/` — reusable UI building blocks, organized by feature area (`tickets/`, `dashboard/`, `notifications/`)
- `features/` — per-feature `api.ts` (Axios calls) + `queries.ts` (TanStack Query hooks) + `schemas.ts` (Zod)
- `pages/` — route-level components
- `routes/` — router configuration and route guards

Full explanation: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

## Database Design

| Entity | Purpose |
|---|---|
| `ApplicationUser` | System users (Identity-backed), with role assignments |
| `Category` / `Priority` / `Status` | Lookup tables for ticket classification and workflow state |
| `Ticket` | Core ticket record — title, description, category, priority, status, assignee, due date, resolution/close timestamps |
| `TicketHistory` | Immutable audit trail of every field change on a ticket |
| `TicketAssignment` | Assignment-specific audit trail (manual vs. round-robin, previous/new assignee, assigned-by) |
| `TicketComment` | Public comments and Agent-only internal notes (`IsInternal` flag) |
| `TicketAttachment` | File attachments on a ticket |
| `Notification` | In-app notifications (assignment, comment, mention) per user |
| `ActivityLog` | System-wide activity/audit log |
| `RefreshToken` | JWT refresh token store |

Key relationships: a `Ticket` belongs to one `Category`, one `Priority`, one `Status`, is created by one `ApplicationUser`, and may be assigned to another. `TicketHistory`, `TicketAssignment`, `TicketComment`, and `Notification` all reference their parent `Ticket`. Soft delete (`IsDeleted`/`DeletedAt`) is applied globally via an EF Core query filter.

Full schema, constraints, and indexes: [`docs/database-design.md`](docs/database-design.md) and [`docs/DATABASE.md`](docs/DATABASE.md).

## How to Run the Project

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20+ and npm
- SQL Server LocalDB (or a real SQL Server/Express instance)
- The `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef`

### Backend

```bash
cd backend
dotnet restore
dotnet build HelpDesk.sln

# Configure the JWT signing key (kept out of source control via dotnet user-secrets)
cd src/HelpDesk.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:SecretKey" "<a-random-string-32-chars-or-longer>"
cd ../..

# Apply migrations (creates HelpDeskSystemDb on LocalDB and seeds lookup data + roles)
dotnet ef database update --project src/HelpDesk.Infrastructure --startup-project src/HelpDesk.Api

dotnet run --project src/HelpDesk.Api
```

The API listens on `http://localhost:5019` by default. Run the backend test suite with:

```bash
cd backend
dotnet test
```

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Opens at `http://localhost:5173`, talking to the API URL configured in `frontend/.env.development` (`VITE_API_BASE_URL`).

```bash
npm run build   # type-check + production build
npm run lint    # ESLint
```

### Database setup notes

Migrations live in `backend/src/HelpDesk.Infrastructure/Persistence/Migrations/`. `dotnet ef database update` applies all pending migrations and seeds lookup data (categories/priorities/statuses) and Identity roles. An idempotent SQL script covering every migration to date is also checked in at `backend/database/InitialCreate.sql`, for environments without the `dotnet-ef` tool.

## API Documentation

Swagger UI is available at `http://localhost:5019/swagger` whenever the API is running in Development mode — it documents every endpoint, request/response schema, and lets you authorize with a JWT bearer token to try requests interactively. See [`docs/api-guide.md`](docs/api-guide.md) for a written walkthrough of the main endpoint groups with example requests/responses.

## Development Progress

| Phase | Description | Status |
|---|---|---|
| Phase 1 | Foundation (Clean Architecture, EF Core, frontend scaffold) | ✅ Completed |
| Phase 2 | Authentication (JWT, Identity, roles, protected routes) | ✅ Completed |
| Phase 3 | Ticket Management (CRUD, search, filtering, history) | ✅ Completed |
| Phase 4 | Workflow (assignment, comments, notifications) | ✅ Completed |
| Phase 5 | Dashboard & Reports (KPIs, charts, PDF/Excel export) | ✅ Completed |
| Phase 6 | Administration (user/role management, lookup CRUD) | ⏳ Planned |

Full roadmap: [`docs/ROADMAP.md`](docs/ROADMAP.md).

## Future Improvements

- AI-assisted ticket classification (auto-suggest category/priority)
- Chat assistant for common IT issues
- Docker Compose deployment (API + SQL Server + frontend)
- CI/CD pipeline (build, test, and deploy on push)
- Real-time notification delivery (SignalR/WebSockets) instead of polling
- Configurable per-priority SLA policies
- User/role management UI (Phase 6)

## Repository Layout

```
HelpDeskSystem/
├── backend/       # .NET 8 Clean Architecture solution (Domain / Application / Infrastructure / Api / tests)
├── frontend/      # Vite + React + TypeScript client
├── docs/          # Architecture, database, API, and process documentation
├── screenshots/   # Application screenshots for this README/portfolio use
└── PROJECT_SPEC.md
```

Full tree: [`docs/FOLDER_STRUCTURE.md`](docs/FOLDER_STRUCTURE.md).

## Documentation

- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — Clean Architecture layering, frontend architecture, tech stack, coding standards, configuration
- [`docs/database-design.md`](docs/database-design.md) — entities, relationships, and design decisions (see also [`docs/DATABASE.md`](docs/DATABASE.md) for full schema/index detail)
- [`docs/api-guide.md`](docs/api-guide.md) — main endpoint groups with example requests/responses
- [`docs/development-notes.md`](docs/development-notes.md) — problems encountered, solutions, and lessons learned building this project
- [`docs/PHASE2_AUTHENTICATION.md`](docs/PHASE2_AUTHENTICATION.md) · [`PHASE3_TICKET_MANAGEMENT.md`](docs/PHASE3_TICKET_MANAGEMENT.md) · [`PHASE4_TICKET_WORKFLOW.md`](docs/PHASE4_TICKET_WORKFLOW.md) · [`PHASE5_DASHBOARDS_REPORTING.md`](docs/PHASE5_DASHBOARDS_REPORTING.md) — what each phase added and how it was verified
- [`docs/PITFALLS.md`](docs/PITFALLS.md) — the detailed technical issue/fix log this project's `development-notes.md` summarizes
- [`docs/ROADMAP.md`](docs/ROADMAP.md) — remaining phases
