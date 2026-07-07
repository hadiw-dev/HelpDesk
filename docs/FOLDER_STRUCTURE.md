# Folder Structure

Complete tree as of the end of Phase 5 (Dashboards & Reporting). Build artifacts (`bin/`, `obj/`, `node_modules/`, `dist/`) are omitted.

```
HelpDeskSystem/
├── PROJECT_SPEC.md
├── docs/
│   ├── FOLDER_STRUCTURE.md
│   ├── ARCHITECTURE.md
│   ├── DATABASE.md
│   ├── PHASE2_AUTHENTICATION.md
│   ├── PHASE3_TICKET_MANAGEMENT.md
│   ├── PHASE4_TICKET_WORKFLOW.md
│   ├── PHASE5_DASHBOARDS_REPORTING.md
│   ├── PITFALLS.md
│   └── ROADMAP.md
├── backend/
│   ├── HelpDesk.sln
│   ├── database/
│   │   └── InitialCreate.sql              # Idempotent SQL script, covers all migrations to date
│   └── src/
│       ├── HelpDesk.Domain/                # Entities, enums, base types — no dependencies on other layers
│       │   ├── Common/
│       │   │   ├── BaseEntity.cs
│       │   │   ├── IAuditableEntity.cs
│       │   │   └── ISoftDelete.cs
│       │   ├── Entities/
│       │   │   ├── LookupEntity.cs
│       │   │   ├── Category.cs
│       │   │   ├── Priority.cs
│       │   │   ├── Status.cs
│       │   │   ├── Ticket.cs
│       │   │   ├── TicketComment.cs
│       │   │   ├── TicketAttachment.cs
│       │   │   ├── TicketHistory.cs
│       │   │   ├── TicketAssignment.cs     # Phase 4
│       │   │   ├── Notification.cs
│       │   │   ├── ActivityLog.cs
│       │   │   └── RefreshToken.cs         # Phase 2
│       │   ├── Enums/
│       │   │   ├── NotificationType.cs     # + Mention (Phase 4)
│       │   │   └── AssignmentType.cs       # Phase 4 — Manual | RoundRobin
│       │   └── Identity/
│       │       ├── ApplicationUser.cs
│       │       └── ApplicationRole.cs
│       │
│       ├── HelpDesk.Application/           # Use-case contracts, cross-cutting interfaces — depends on Domain only
│       │   ├── Common/
│       │   │   ├── Exceptions/
│       │   │   │   └── AppException.cs     # NotFound/Unauthorized/Forbidden/Conflict/Validation
│       │   │   ├── Interfaces/
│       │   │   │   ├── ICurrentUserService.cs
│       │   │   │   ├── IDateTimeProvider.cs
│       │   │   │   ├── IActivityLogService.cs  # Phase 2
│       │   │   │   └── IEmailSender.cs         # Phase 2 + SendNotificationEmailAsync (Phase 4)
│       │   │   ├── Options/
│       │   │   │   └── JwtOptions.cs
│       │   │   ├── Models/
│       │   │   │   └── PagedResult.cs      # Phase 3 — generic paging envelope, reused by Notifications (Phase 4)
│       │   │   ├── Utils/
│       │   │   │   └── MentionParser.cs    # Phase 4 — extracts @[Name](userId) tokens from comment text
│       │   │   └── Mappings/
│       │   │       └── MappingProfile.cs
│       │   ├── Features/
│       │   │   ├── Auth/                   # Phase 2 (Dtos/Interfaces/Validators/Mappings)
│       │   │   ├── Lookups/                # Phase 3 + GetAssignableAgentsAsync (Phase 4)
│       │   │   │   ├── Dtos/LookupItemDto.cs
│       │   │   │   └── Interfaces/ILookupService.cs
│       │   │   ├── Tickets/                # Phase 3 — AssignedToUserId removed from UpdateTicketRequest (Phase 4)
│       │   │   │   ├── Dtos/
│       │   │   │   ├── Interfaces/ITicketService.cs
│       │   │   │   ├── Validators/
│       │   │   │   └── Mappings/TicketMappingProfile.cs
│       │   │   ├── Assignments/            # Phase 4
│       │   │   │   ├── Dtos/
│       │   │   │   │   ├── AssignTicketRequest.cs
│       │   │   │   │   └── AssignmentHistoryEntryDto.cs
│       │   │   │   ├── Interfaces/IAssignmentService.cs
│       │   │   │   └── Validators/AssignTicketRequestValidator.cs
│       │   │   ├── Comments/                # Phase 4
│       │   │   │   ├── Dtos/
│       │   │   │   │   ├── CommentDto.cs
│       │   │   │   │   └── CreateCommentRequest.cs
│       │   │   │   ├── Interfaces/ICommentService.cs
│       │   │   │   └── Validators/CreateCommentRequestValidator.cs
│       │   │   ├── Notifications/            # Phase 4
│       │   │   │   ├── Dtos/NotificationDto.cs
│       │   │   │   └── Interfaces/INotificationService.cs   # dispatch + the caller's own feed
│       │   │   ├── Dashboard/                # Phase 5
│       │   │   │   ├── Dtos/                 # KpiSummaryDto, CategoryBreakdownDto, PriorityBreakdownDto,
│       │   │   │   │                         # MonthlyTicketsDto, ResolutionTimeDto, SlaReportDto, DashboardQueryParameters
│       │   │   │   ├── Interfaces/IDashboardService.cs
│       │   │   │   └── Validators/DashboardQueryParametersValidator.cs
│       │   │   └── Reports/                  # Phase 5
│       │   │       └── Interfaces/IReportService.cs
│       │   └── DependencyInjection.cs
│       │
│       ├── HelpDesk.Infrastructure/        # EF Core, persistence, external services — depends on Application + Domain
│       │   ├── Persistence/
│       │   │   ├── AppDbContext.cs         # + TicketAssignments DbSet (Phase 4)
│       │   │   ├── Configurations/         # + TicketAssignmentConfiguration.cs (Phase 4)
│       │   │   ├── Migrations/             # InitialCreate, AddRefreshTokens, AddTicketAssignments (Phase 4)
│       │   │   └── Seed/SeedIds.cs
│       │   ├── Services/
│       │   │   ├── DateTimeProvider.cs
│       │   │   ├── TokenService.cs         # Phase 2
│       │   │   ├── AuthService.cs          # Phase 2
│       │   │   ├── ActivityLogService.cs   # Phase 2
│       │   │   ├── LoggingEmailSender.cs   # Phase 2 + SendNotificationEmailAsync (Phase 4)
│       │   │   ├── LookupService.cs        # Phase 3 + GetAssignableAgentsAsync (Phase 4)
│       │   │   ├── TicketService.cs        # Phase 3 — refactored to use Shared/ helpers (Phase 4)
│       │   │   ├── AssignmentService.cs    # Phase 4 — manual + round-robin assignment
│       │   │   ├── CommentService.cs       # Phase 4 — public comments + internal notes, mentions
│       │   │   ├── NotificationService.cs  # Phase 4 — replaces NoOpNotificationService
│       │   │   ├── DashboardService.cs     # Phase 5 — KPI/breakdown/monthly/resolution-time/SLA queries
│       │   │   ├── ReportService.cs        # Phase 5 — QuestPDF (PDF) + ClosedXML (Excel) rendering
│       │   │   └── Shared/                 # Phase 4 — cross-cutting helpers used by 3+ services
│       │   │       ├── TicketAccessGuard.cs
│       │   │       ├── UserDisplayNameResolver.cs
│       │   │       └── TicketHistoryRecorder.cs
│       │   └── DependencyInjection.cs
│       │
│       └── HelpDesk.Api/                   # Composition root, HTTP concerns — depends on all layers
│           ├── Controllers/V1/
│           │   ├── PingController.cs
│           │   ├── AuthController.cs       # Phase 2
│           │   ├── LookupsController.cs    # Phase 3 + GET agents (Phase 4)
│           │   ├── TicketsController.cs    # Phase 3 + assign/auto-assign/assignments/comments (Phase 4)
│           │   ├── NotificationsController.cs  # Phase 4
│           │   └── DashboardController.cs  # Phase 5 — data endpoints + PDF/Excel report export
│           ├── Filters/ValidationFilter.cs
│           ├── Middleware/
│           │   ├── ExceptionHandlingMiddleware.cs
│           │   └── HealthCheckResponseWriter.cs
│           ├── Services/CurrentUserService.cs
│           ├── Program.cs
│           ├── appsettings.json
│           └── appsettings.Development.json
│   └── tests/
│       ├── HelpDesk.Tests/                 # Unit tests (xUnit + Moq)
│       │   ├── Common/
│       │   ├── Persistence/
│       │   ├── Auth/                       # Phase 2
│       │   ├── Tickets/TicketServiceTests.cs
│       │   ├── Assignments/AssignmentServiceTests.cs   # Phase 4 — 7 tests
│       │   ├── Comments/CommentServiceTests.cs         # Phase 4 — 8 tests
│       │   ├── Notifications/NotificationServiceTests.cs  # Phase 4 — 8 tests
│       │   └── Dashboard/                              # Phase 5
│       │       ├── DashboardServiceTests.cs            # 9 tests
│       │       └── ReportServiceTests.cs                # 2 tests
│       └── HelpDesk.IntegrationTests/      # WebApplicationFactory-based integration tests
│           └── Infrastructure/
│
└── frontend/
    ├── index.html
    ├── vite.config.ts
    ├── tsconfig.json / tsconfig.app.json / tsconfig.node.json
    ├── eslint.config.js
    ├── components.json                     # shadcn/ui config
    ├── .env.development
    └── src/
        ├── api/
        │   └── axiosInstance.ts            # 401 refresh-and-retry interceptor (Phase 2)
        ├── components/
        │   ├── ui/                         # shadcn/ui primitives (button.tsx, ...)
        │   ├── layout/Navbar.tsx           # + NotificationCenter (Phase 4), + Reports nav link (Phase 5)
        │   ├── notifications/              # Phase 4
        │   │   └── NotificationCenter.tsx
        │   ├── tickets/                     # Phase 4
        │   │   ├── AssignmentPanel.tsx
        │   │   ├── CommentsPanel.tsx       # reused for both public comments and internal notes
        │   │   ├── MentionTextarea.tsx     # @-mention autocomplete
        │   │   ├── MentionText.tsx         # renders stored @[Name](id) tokens as highlighted pills
        │   │   └── TicketTimeline.tsx       # merges history + assignments + comments, sorted by time
        │   └── dashboard/                    # Phase 5
        │       ├── KpiCard.tsx
        │       ├── BreakdownPieChart.tsx   # reused for category and priority breakdowns
        │       ├── MonthlyTicketsLineChart.tsx
        │       ├── ResolutionTimeBarChart.tsx
        │       ├── SlaDashboard.tsx        # compliance % + breached-ticket table
        │       └── DateRangeFilter.tsx     # shared by DashboardPage and ReportsPage
        ├── features/
        │   ├── auth/                       # Phase 2
        │   ├── lookups/                     # Phase 3 + getAgents/useAgentsQuery (Phase 4)
        │   │   ├── api.ts
        │   │   └── queries.ts
        │   ├── tickets/                     # Phase 3 — assignedToUserId removed from UpdateTicketInput (Phase 4)
        │   │   ├── api.ts
        │   │   ├── schemas.ts
        │   │   └── queries.ts
        │   ├── assignments/                 # Phase 4
        │   │   ├── api.ts
        │   │   └── queries.ts
        │   ├── comments/                     # Phase 4
        │   │   ├── api.ts
        │   │   ├── schemas.ts
        │   │   └── queries.ts
        │   ├── notifications/                # Phase 4
        │   │   ├── api.ts
        │   │   └── queries.ts
        │   └── dashboard/                     # Phase 5
        │       ├── api.ts                    # + getPdfReport/getExcelReport (blob downloads)
        │       └── queries.ts
        ├── hooks/useAuth.ts
        ├── layouts/
        │   ├── AppLayout.tsx
        │   └── AuthLayout.tsx
        ├── lib/utils.ts
        ├── pages/
        │   ├── LoginPage.tsx
        │   ├── RegisterPage.tsx
        │   ├── ForgotPasswordPage.tsx
        │   ├── ResetPasswordPage.tsx
        │   ├── DashboardPage.tsx            # Phase 5 — full rewrite: KPI cards, pie/line/bar charts, SLA section
        │   ├── TicketsPage.tsx
        │   ├── CreateTicketPage.tsx
        │   ├── EditTicketPage.tsx           # Phase 3 — "assign to me" button removed (Phase 4, moved to AssignmentPanel)
        │   ├── TicketDetailsPage.tsx        # Phase 4 — + AssignmentPanel, CommentsPanel x2, TicketTimeline
        │   ├── ReportsPage.tsx              # Phase 5 — PDF/Excel export
        │   ├── AdminPage.tsx
        │   ├── ProfilePage.tsx
        │   ├── ErrorPage.tsx
        │   └── NotFoundPage.tsx
        ├── providers/
        │   ├── ThemeProvider.tsx
        │   └── QueryProvider.tsx
        ├── routes/
        │   ├── AppRoutes.tsx                # + /reports (Phase 5)
        │   └── ProtectedRoute.tsx
        ├── types/
        │   ├── auth.ts
        │   ├── tickets.ts
        │   ├── lookups.ts
        │   ├── assignments.ts               # Phase 4
        │   ├── comments.ts                   # Phase 4
        │   ├── notifications.ts              # Phase 4
        │   └── dashboard.ts                  # Phase 5
        ├── utils/
        │   ├── constants.ts
        │   ├── tokenStorage.ts
        │   ├── errors.ts
        │   ├── roles.ts
        │   ├── mentions.ts                   # Phase 4 — tokenize/parse/insert @[Name](id) tokens
        │   └── timeline.ts                   # Phase 4 — merges history/assignments/comments into one feed
        ├── App.tsx
        └── main.tsx
```
