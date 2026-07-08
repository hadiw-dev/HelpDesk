# Folder Structure

Complete tree as of the end of Phase 8 (Production Readiness). Build artifacts (`bin/`, `obj/`, `node_modules/`, `dist/`, `TestResults/`) are omitted.

```
HelpDeskSystem/
├── PROJECT_SPEC.md
├── RELEASE_NOTES.md                      # Phase 8
├── docker-compose.yml                     # Phase 8 — api + frontend + sqlserver, orchestrated together
├── .env.example                           # Phase 8 — template for docker-compose.yml's secrets
├── docs/
│   ├── FOLDER_STRUCTURE.md
│   ├── ARCHITECTURE.md
│   ├── DATABASE.md
│   ├── database-design.md
│   ├── api-guide.md
│   ├── development-notes.md
│   ├── PHASE2_AUTHENTICATION.md
│   ├── PHASE3_TICKET_MANAGEMENT.md
│   ├── PHASE4_TICKET_WORKFLOW.md
│   ├── PHASE5_DASHBOARDS_REPORTING.md
│   ├── PHASE6_ADMINISTRATION.md
│   ├── PHASE7_HARDENING.md
│   ├── PHASE7_TESTING_REPORT.md
│   ├── PHASE7_COVERAGE_REPORT.md
│   ├── PHASE8_PRODUCTION_READINESS.md   # Phase 8
│   ├── SETUP_GUIDE.md                   # Phase 8 — native local dev setup
│   ├── DEPLOYMENT_GUIDE.md              # Phase 8 — Docker Compose deployment, verification, troubleshooting
│   ├── USER_GUIDE.md                    # Phase 8 — feature walkthrough per role
│   ├── DEPLOYMENT_CHECKLIST.md          # Phase 8
│   ├── PRODUCTION_CHECKLIST.md          # Phase 8
│   ├── PRESENTATION_SUMMARY.md          # Phase 8
│   ├── swagger/swagger.json             # Phase 8 — static export of the live OpenAPI document
│   ├── PITFALLS.md
│   └── ROADMAP.md
├── postman/                              # Phase 7 — complete Postman collection + local environment
│   ├── HelpDesk-API.postman_collection.json
│   └── HelpDesk-Local.postman_environment.json
├── screenshots/
│   └── README.md
├── backend/
│   ├── HelpDesk.sln
│   ├── Dockerfile                          # Phase 8 — multi-stage build, non-root runtime user
│   ├── .dockerignore                       # Phase 8
│   ├── file-storage/                       # Phase 6 — uploaded ticket attachments, outside backend/src/ entirely
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
│       │   │   ├── RefreshToken.cs         # Phase 2
│       │   │   └── SystemSetting.cs        # Phase 6 — singleton settings row
│       │   ├── Enums/
│       │   │   ├── NotificationType.cs     # + Mention (Phase 4)
│       │   │   └── AssignmentType.cs       # Phase 4 — Manual | RoundRobin
│       │   └── Identity/
│       │       ├── ApplicationUser.cs
│       │       ├── ApplicationRole.cs
│       │       └── RoleNames.cs            # Phase 6 — the 4 fixed role name constants
│       │
│       ├── HelpDesk.Application/           # Use-case contracts, cross-cutting interfaces — depends on Domain only
│       │   ├── Common/
│       │   │   ├── Exceptions/
│       │   │   │   └── AppException.cs     # NotFound/Unauthorized/Forbidden/Conflict/Validation
│       │   │   ├── Interfaces/
│       │   │   │   ├── ICurrentUserService.cs
│       │   │   │   ├── IDateTimeProvider.cs
│       │   │   │   ├── IActivityLogService.cs  # Phase 2 — write side only; read side is Features/Admin/ActivityLogs (Phase 6)
│       │   │   │   ├── IEmailSender.cs         # Phase 2 + SendNotificationEmailAsync (Phase 4)
│       │   │   │   └── IFileStorageService.cs  # Phase 6 — abstracts disk I/O for attachments
│       │   │   ├── Options/
│       │   │   │   └── JwtOptions.cs
│       │   │   ├── Models/
│       │   │   │   └── PagedResult.cs      # Phase 3 — generic paging envelope, reused throughout
│       │   │   ├── Utils/
│       │   │   │   ├── MentionParser.cs    # Phase 4 — extracts @[Name](userId) tokens from comment text
│       │   │   │   └── FileSignatureValidator.cs  # Phase 7 — magic-byte content validation for uploads
│       │   │   └── Mappings/
│       │   │       └── MappingProfile.cs
│       │   ├── Features/
│       │   │   ├── Auth/                   # Phase 2 (Dtos/Interfaces/Validators/Mappings)
│       │   │   ├── Lookups/                # Phase 3 + GetAssignableAgentsAsync (Phase 4) — read-only dropdowns
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
│       │   │   ├── Reports/                  # Phase 5
│       │   │   │   └── Interfaces/IReportService.cs
│       │   │   ├── Attachments/              # Phase 6 — ticket-scoped, not admin-only
│       │   │   │   ├── Dtos/
│       │   │   │   │   ├── TicketAttachmentDto.cs
│       │   │   │   │   ├── UploadAttachmentRequest.cs   # IFormFile-agnostic stand-in
│       │   │   │   │   └── AttachmentDownloadResult.cs
│       │   │   │   └── Interfaces/IAttachmentService.cs
│       │   │   └── Admin/                    # Phase 6 — all behind [Authorize(Policy = "RequireAdmin")]
│       │   │       ├── Users/
│       │   │       │   ├── Dtos/             # AdminUserDto, CreateUserRequest, UpdateUserRequest,
│       │   │       │   │                     # ChangeUserRoleRequest, AdminUserQueryParameters
│       │   │       │   ├── Interfaces/IAdminUserService.cs
│       │   │       │   └── Validators/
│       │   │       ├── Lookups/
│       │   │       │   ├── Dtos/AdminLookupItemDto.cs, LookupUpsertRequest.cs
│       │   │       │   ├── Interfaces/IAdminLookupService.cs   # generic: <TEntity> where TEntity : LookupEntity
│       │   │       │   └── Validators/LookupUpsertRequestValidator.cs
│       │   │       ├── ActivityLogs/
│       │   │       │   ├── Dtos/ActivityLogEntryDto.cs, ActivityLogQueryParameters.cs
│       │   │       │   ├── Interfaces/IActivityLogQueryService.cs   # read side (separate from Common's write-only IActivityLogService)
│       │   │       │   └── Validators/
│       │   │       └── Settings/
│       │   │           ├── Dtos/SystemSettingsDto.cs, UpdateSystemSettingsRequest.cs
│       │   │           ├── Interfaces/ISystemSettingsService.cs   # also consumed by AttachmentService for upload limits
│       │   │           └── Validators/
│       │   └── DependencyInjection.cs
│       │
│       ├── HelpDesk.Infrastructure/        # EF Core, persistence, external services — depends on Application + Domain
│       │   ├── Persistence/
│       │   │   ├── AppDbContext.cs         # + SystemSettings DbSet (Phase 6)
│       │   │   ├── Configurations/         # + SystemSettingConfiguration.cs (Phase 6, seeds one default row)
│       │   │   ├── Migrations/             # InitialCreate, AddRefreshTokens, AddTicketAssignments, AddSystemSettings (Phase 6)
│       │   │   └── Seed/SeedIds.cs
│       │   ├── Services/
│       │   │   ├── DateTimeProvider.cs
│       │   │   ├── TokenService.cs         # Phase 2
│       │   │   ├── AuthService.cs          # Phase 2
│       │   │   ├── ActivityLogService.cs   # Phase 2 — write side
│       │   │   ├── LoggingEmailSender.cs   # Phase 2 + SendNotificationEmailAsync (Phase 4)
│       │   │   ├── LookupService.cs        # Phase 3 + GetAssignableAgentsAsync (Phase 4)
│       │   │   ├── TicketService.cs        # Phase 3 — refactored to use Shared/ helpers (Phase 4)
│       │   │   ├── AssignmentService.cs    # Phase 4 — manual + round-robin assignment
│       │   │   ├── CommentService.cs       # Phase 4 — public comments + internal notes, mentions
│       │   │   ├── NotificationService.cs  # Phase 4 — replaces NoOpNotificationService
│       │   │   ├── DashboardService.cs     # Phase 5 — KPI/breakdown/monthly/resolution-time/SLA queries
│       │   │   ├── ReportService.cs        # Phase 5 — QuestPDF (PDF) + ClosedXML (Excel) rendering
│       │   │   ├── LocalFileStorageService.cs  # Phase 6 — saves outside web root, GUID-named files
│       │   │   ├── AttachmentService.cs        # Phase 6 — upload/download/delete, validated against SystemSettings
│       │   │   ├── AdminUserService.cs         # Phase 6 — UserManager-based create/update/role-change/delete
│       │   │   ├── AdminLookupService.cs       # Phase 6 — generic CRUD for Category/Priority/Status
│       │   │   ├── ActivityLogQueryService.cs  # Phase 6 — read side of the activity log
│       │   │   ├── SystemSettingsService.cs    # Phase 6 — get-or-create singleton row
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
│           │   ├── TicketsController.cs    # Phase 3 + assign/comments (Phase 4) + attachments (Phase 6)
│           │   ├── NotificationsController.cs  # Phase 4
│           │   ├── DashboardController.cs  # Phase 5 — data endpoints + PDF/Excel report export
│           │   ├── AdminUsersController.cs         # Phase 6
│           │   ├── AdminLookupsController.cs       # Phase 6 — categories/priorities/statuses, one controller
│           │   ├── AdminActivityLogsController.cs  # Phase 6
│           │   └── AdminSettingsController.cs      # Phase 6
│           ├── Filters/ValidationFilter.cs
│           ├── Middleware/
│           │   ├── ExceptionHandlingMiddleware.cs
│           │   ├── HealthCheckResponseWriter.cs
│           │   └── SecurityHeadersMiddleware.cs   # Phase 7 — nosniff/frame-options/referrer-policy headers
│           ├── Services/CurrentUserService.cs
│           ├── Program.cs                  # + QuestPDF license (Phase 5); + JWT secret length fail-fast, HSTS (Phase 7); + Swagger in all envs, ApplyMigrationsOnStartup (Phase 8)
│           ├── appsettings.json            # + FileStorage:RootPath (Phase 6)
│           └── appsettings.Development.json
│   └── tests/
│       ├── HelpDesk.Tests/                 # Unit tests (xUnit + Moq) — 114 tests total
│       │   ├── Common/
│       │   │   ├── AutoMapperConfigurationTests.cs
│       │   │   └── FileSignatureValidatorTests.cs      # Phase 7 — 5 methods / 14 cases
│       │   ├── Persistence/
│       │   ├── Auth/                       # Phase 2 + failed-login/lockout logging tests (Phase 7)
│       │   ├── Tickets/TicketServiceTests.cs
│       │   ├── Assignments/AssignmentServiceTests.cs   # Phase 4 — 7 tests
│       │   ├── Comments/CommentServiceTests.cs         # Phase 4 — 8 tests
│       │   ├── Notifications/NotificationServiceTests.cs  # Phase 4 — 8 tests
│       │   ├── Dashboard/                              # Phase 5
│       │   │   ├── DashboardServiceTests.cs            # 9 tests
│       │   │   └── ReportServiceTests.cs                # 2 tests
│       │   ├── Attachments/                             # Phase 6 + magic-byte tests (Phase 7) — 12 tests
│       │   │   ├── AttachmentServiceTests.cs
│       │   │   └── FakeFileStorageService.cs           # in-memory IFileStorageService test double
│       │   └── Admin/                                   # Phase 6
│       │       ├── AdminUserServiceTests.cs            # 9 tests
│       │       ├── AdminLookupServiceTests.cs          # 6 tests
│       │       ├── ActivityLogQueryServiceTests.cs     # 4 tests
│       │       └── SystemSettingsServiceTests.cs       # 3 tests
│       └── HelpDesk.IntegrationTests/      # WebApplicationFactory-based integration tests — 35 tests total, real LocalDB
│           ├── Infrastructure/
│           │   ├── TestWebApplicationFactory.cs
│           │   ├── ApiCollection.cs        # Phase 7 — shared collection fixture (see PITFALLS.md)
│           │   └── AuthTestHelper.cs       # Phase 7 — register/login/promote-role helpers, shared JSON options
│           ├── ApiSmokeTests.cs            # 3 tests
│           ├── AuthFlowTests.cs            # Phase 7 — 10 tests
│           ├── AuthorizationTests.cs       # Phase 7 — 7 tests
│           ├── SecurityTests.cs            # Phase 7 — headers/CORS/SQLi/XSS, 5 methods / 7 cases
│           └── TicketApiTests.cs           # Phase 7 — 8 tests
│
└── frontend/
    ├── Dockerfile                            # Phase 8 — multi-stage build (node → nginx static serve)
    ├── .dockerignore                         # Phase 8
    ├── nginx.conf                            # Phase 8 — SPA fallback routing + /health endpoint
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
        │   ├── ErrorBoundary.tsx           # Phase 7 — catches render errors outside React Router's own errorElement
        │   ├── PageLoadingFallback.tsx     # Phase 7 — Suspense fallback for lazy-loaded routes
        │   ├── layout/Navbar.tsx           # + NotificationCenter (Phase 4), + Reports (Phase 5), Admin link gated to Admin role (Phase 6)
        │   ├── notifications/              # Phase 4
        │   │   └── NotificationCenter.tsx
        │   ├── tickets/                     # Phase 4
        │   │   ├── AssignmentPanel.tsx
        │   │   ├── AttachmentsPanel.tsx    # Phase 6 — upload/list/download/delete
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
        │   ├── dashboard/                     # Phase 5
        │   │   ├── api.ts                    # + getPdfReport/getExcelReport (blob downloads)
        │   │   └── queries.ts
        │   ├── attachments/                    # Phase 6
        │   │   ├── api.ts                     # multipart upload, blob download
        │   │   └── queries.ts
        │   └── admin/                          # Phase 6
        │       ├── users/{api,queries}.ts
        │       ├── lookups/{api,queries}.ts    # generic — parametrized by resource ('categories'|'priorities'|'statuses')
        │       ├── activityLogs/{api,queries}.ts
        │       └── settings/{api,queries}.ts
        ├── hooks/useAuth.ts
        ├── layouts/
        │   ├── AppLayout.tsx                # + <Suspense> around <Outlet/> for lazy-loaded pages (Phase 7)
        │   └── AuthLayout.tsx               # + <Suspense> around <Outlet/> for lazy-loaded pages (Phase 7)
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
        │   ├── TicketDetailsPage.tsx        # Phase 4 — + AssignmentPanel, CommentsPanel x2, TicketTimeline; + AttachmentsPanel (Phase 6)
        │   ├── ReportsPage.tsx              # Phase 5 — PDF/Excel export
        │   ├── AdminPage.tsx                # Phase 6 — full rewrite: dashboard landing linking to admin/* sub-pages
        │   ├── admin/                        # Phase 6
        │   │   ├── UserManagementPage.tsx
        │   │   ├── LookupManagementPage.tsx  # generic, reused by the three wrapper pages below
        │   │   ├── CategoryManagementPage.tsx
        │   │   ├── PriorityManagementPage.tsx
        │   │   ├── StatusManagementPage.tsx
        │   │   ├── SystemSettingsPage.tsx
        │   │   └── ActivityLogPage.tsx
        │   ├── ProfilePage.tsx
        │   ├── ErrorPage.tsx
        │   └── NotFoundPage.tsx
        ├── providers/
        │   ├── ThemeProvider.tsx
        │   └── QueryProvider.tsx
        ├── routes/
        │   ├── AppRoutes.tsx                # + /reports (Phase 5), + /admin/* nested under AdminRoute (Phase 6); all pages lazy-loaded (Phase 7)
        │   ├── lazyPages.ts                 # Phase 7 — React.lazy definitions, kept out of AppRoutes.tsx (see PITFALLS.md)
        │   ├── ProtectedRoute.tsx
        │   └── AdminRoute.tsx               # Phase 6 — redirects non-Admins away from /admin/*
        ├── types/
        │   ├── auth.ts
        │   ├── tickets.ts
        │   ├── lookups.ts
        │   ├── assignments.ts               # Phase 4
        │   ├── comments.ts                   # Phase 4
        │   ├── notifications.ts              # Phase 4
        │   ├── dashboard.ts                  # Phase 5
        │   ├── attachments.ts                 # Phase 6
        │   └── admin.ts                        # Phase 6
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
