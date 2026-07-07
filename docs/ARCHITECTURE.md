# Architecture

## Clean Architecture (Backend)

The backend is split into four projects with a strict one-way dependency rule: outer layers depend on inner layers, never the reverse.

```
HelpDesk.Api  ──depends on──▶  HelpDesk.Infrastructure  ──depends on──▶  HelpDesk.Application  ──depends on──▶  HelpDesk.Domain
     │                                                                                                              ▲
     └──────────────────────────────────────────────────────────────────────────────────────────────────────────────┘
                                          (Api also references Application and Domain directly)
```

### HelpDesk.Domain
The innermost layer. Contains entities (`Ticket`, `Category`, `Priority`, `Status`, `TicketComment`, `TicketAttachment`, `TicketHistory`, `Notification`, `ActivityLog`), the Identity types (`ApplicationUser`, `ApplicationRole`), enums, and the `BaseEntity`/`IAuditableEntity`/`ISoftDelete` contracts. It has no dependency on EF Core, ASP.NET Core, or any other project — it only depends on `Microsoft.Extensions.Identity.Stores` for the `IdentityUser<TKey>`/`IdentityRole<TKey>` base classes, which is a minimal abstractions package, not the full ASP.NET Core framework.

### HelpDesk.Application
Use-case-facing contracts and cross-cutting interfaces that the rest of the system depends on but that don't belong to any specific delivery mechanism: `ICurrentUserService`, `IDateTimeProvider`, `INotificationService`, and the AutoMapper/FluentValidation composition (`DependencyInjection.AddApplicationServices()`). It depends only on `HelpDesk.Domain`. As real use cases (register, create ticket, etc.) are added in later phases, their DTOs, validators, and mapping profiles live here.

### HelpDesk.Infrastructure
Implements the technical concerns: `AppDbContext` (EF Core + ASP.NET Identity stores), entity type configurations, migrations, lookup/role seed data, and concrete implementations of the Application interfaces (`DateTimeProvider`, `NoOpNotificationService`, and — since Phase 2 — `TokenService`, `AuthService`, `ActivityLogService`, `LoggingEmailSender`). Depends on `Application` and `Domain`.

**Deliberate exception:** ASP.NET Identity's `AddIdentity<TUser, TRole>()` call is *not* registered here — it's registered in `HelpDesk.Api` instead. `AddIdentity` requires the ASP.NET Core shared framework (`Microsoft.AspNetCore.App`), which is only implicitly referenced by projects using `Microsoft.NET.Sdk.Web`. `HelpDesk.Infrastructure` is a plain class library (`Microsoft.NET.Sdk`) so it doesn't carry that framework reference. Everything else Identity-related (the EF Core stores, the `AppDbContext` inheriting `IdentityDbContext<...>`, and — confirmed by a build spike in Phase 2 — `UserManager<T>`/`RoleManager<T>` themselves) still work fine in Infrastructure. Only `SignInManager<T>` shares `AddIdentity`'s constraint, and this API doesn't need it (see `docs/PHASE2_AUTHENTICATION.md`).

### HelpDesk.Api
The composition root and only project that talks to the network. Wires up Serilog, Swagger, API versioning, CORS, the JWT bearer scheme, Identity, the global exception middleware, and health checks in `Program.cs`. Contains versioned controllers (`Controllers/V1`) and any service implementation that specifically needs ASP.NET Core types (e.g. `CurrentUserService`, which needs `IHttpContextAccessor`).

### Why this split
- **Testability** — Domain and Application have no infrastructure dependencies, so they can be unit tested without a database or an HTTP context.
- **Swappability** — Infrastructure could be replaced (different database, different email provider) without touching Domain or Application.
- **Enforced direction** — the project reference graph itself prevents accidental dependency inversions (e.g. Domain cannot reference EF Core, because it has no package reference to it).

## Technology Stack

| Concern | Choice |
|---|---|
| API framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (LocalDB for local dev — see `PITFALLS.md`) |
| Identity | ASP.NET Identity (`IdentityUser<Guid>`, `IdentityRole<Guid>`) |
| Auth token format | JWT access tokens (15 min) + persisted, rotating refresh tokens (7 days) — issued since Phase 2 |
| Object mapping | AutoMapper 13 |
| Validation | FluentValidation |
| Logging | Serilog (console + rolling file sinks) |
| API docs | Swashbuckle (Swagger/OpenAPI), versioned via Asp.Versioning |
| Testing | xUnit, Moq, EF Core InMemory, `Microsoft.AspNetCore.Mvc.Testing` |
| Frontend framework | React 19 + TypeScript, built with Vite |
| Styling | Tailwind CSS v4 |
| Component library | shadcn/ui (Base UI primitives) |
| Routing | React Router v7 (data router / `createBrowserRouter`) |
| Server state | TanStack Query |
| Forms | React Hook Form + Zod resolvers |
| Charts | Recharts |
| HTTP client | Axios |

## Frontend Architecture

The frontend is a single Vite React+TS application organized by technical concern (not yet by feature, since no feature has business logic in Phase 1):

- **`api/`** — the shared Axios instance. A request interceptor attaches a bearer token from `localStorage` if present; a response interceptor is a placeholder for future 401 → refresh-token handling.
- **`providers/`** — app-wide React context providers composed once in `App.tsx`: `ThemeProvider` (light/dark/system, persisted to `localStorage`, toggles the `dark` class Tailwind/shadcn expect) and `QueryProvider` (wraps `@tanstack/react-query`'s `QueryClientProvider`).
- **`routes/`** — `AppRoutes.tsx` defines the route tree with React Router's data router (`createBrowserRouter`), which gives each route branch its own `errorElement` distinct from the catch-all 404 route. `ProtectedRoute.tsx` is a pass-through placeholder today; it becomes a real auth guard once login exists.
- **`layouts/`** — `AuthLayout` (centered card, used by `/login` and `/register`) and `AppLayout` (top nav + content area, used by the authenticated-area pages).
- **`pages/`** — one component per route: `LoginPage`, `RegisterPage`, `DashboardPage`, `TicketsPage`, `AdminPage`, `ProfilePage`, plus `ErrorPage` (route-level runtime errors) and `NotFoundPage` (unmatched routes).
- **`components/`** — `ui/` holds shadcn/ui primitives (generated, not hand-written); `layout/` holds structural pieces like `Navbar`.
- **`features/`** and **`hooks/`** — intentionally empty in Phase 1 (see the `README.md` placeholders in each). Feature modules and shared hooks (starting with `useAuth`) are added as each capability is implemented.
- **`types/`** and **`utils/`** — shared TypeScript types (`auth.ts`) and constants (`constants.ts`: API base URL, storage keys).

## Coding Standards

- **C#**: nullable reference types enabled everywhere; one class per file; `IEntityTypeConfiguration<T>` per entity rather than configuring everything in `OnModelCreating`; DI composition exposed via `AddXServices()` extension methods per project, called from `Program.cs`.
- **TypeScript**: strict compiler options inherited from the Vite template (`noUnusedLocals`, `noUnusedParameters`, `noFallthroughCasesInSwitch`); path alias `@/*` → `src/*`; components are named exports, not default exports (except `App.tsx`, which Vite's template convention expects as default).
- **No business logic yet**: Phase 1 deliberately contains no ticket/authentication business rules — only entities, configuration, and placeholder seams (interfaces, `ProtectedRoute`, disabled buttons on auth forms) that later phases fill in.

## Configuration Instructions

- **Backend**: connection string, JWT settings, and CORS origins are read from `appsettings.json` / `appsettings.Development.json`. The JWT signing key is **not** committed — it's stored via `dotnet user-secrets` (see `PITFALLS.md`). To point at a different database, change `ConnectionStrings:DefaultConnection`.
- **Frontend**: `VITE_API_BASE_URL` in `.env.development` controls the Axios base URL (defaults to `http://localhost:5019/api/v1`, matching the Api project's default `dotnet run` HTTP profile).
