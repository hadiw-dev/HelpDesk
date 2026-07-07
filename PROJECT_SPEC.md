# IT Help Desk & Ticketing Management System

## Executive Summary

You are acting as a Senior Full-Stack Software Architect and Lead Software Engineer.

Your objective is to design, develop, test, and document a production-quality enterprise IT Help Desk & Ticketing Management System following modern software engineering best practices.

The application will be developed incrementally, one phase at a time, using Clean Architecture and industry-standard design principles.

Employees will submit IT support requests, while IT Support Agents, Managers, and Administrators manage, prioritize, assign, monitor, and resolve tickets through a centralized dashboard.

The system must be scalable, secure, maintainable, and suitable for deployment in a real enterprise environment.

Always prioritize:
- Clean Architecture
- SOLID principles
- Maintainability
- Security
- Testability
- Reusability
- Readability

Never skip phases.

Never implement future features before the current phase is fully completed, tested, and documented.

Treat this document as the single source of truth for every development decision.

## Phase 1 – Foundation

Implement Phase 1 – Foundation only. Do NOT implement authentication, tickets, dashboards, or any later features. Stop after Phase 1 is fully complete.

### Objectives

Create the complete foundation for the project so all future phases can be built on top of it.

### Backend

Create the solution using Clean Architecture.

Projects:

- HelpDesk.Api
- HelpDesk.Application
- HelpDesk.Domain
- HelpDesk.Infrastructure
- HelpDesk.Tests

Configure:

- .NET 8 Web API
- Entity Framework Core
- SQL Server Express
- ASP.NET Identity (configuration only, no authentication endpoints yet)
- Dependency Injection
- AutoMapper
- FluentValidation
- Serilog
- Swagger/OpenAPI
- JWT configuration (configuration only)
- API Versioning (/api/v1)
- Global Exception Middleware returning ProblemDetails
- CORS allowing the frontend origin
- Health Check endpoint
- User Secrets for sensitive configuration
- appsettings.Development.json
- appsettings.json

Create the initial folder structure following Clean Architecture.

Create placeholder services and interfaces where appropriate.

### Database

Create AppDbContext.

Create all entities only.

Do NOT implement business logic yet.

Entities:

- ApplicationUser
- Category
- Priority
- Status
- Ticket
- TicketComment
- TicketAttachment
- TicketHistory
- Notification
- ActivityLog

Configure:

- Entity relationships
- Primary keys
- Foreign keys
- Constraints
- Indexes
- Soft delete support

Generate the first EF Core Migration.

Generate the SQL script.

Seed initial lookup data:

Categories

- Hardware
- Software
- Network
- Email
- Access Request
- Other

Priorities

- Low
- Medium
- High
- Critical

Statuses

- Open
- In Progress
- Pending
- Resolved
- Closed

Identity Roles

- Admin
- IT Support Agent
- Employee
- Manager

### Frontend

Create a Vite React + TypeScript application.

Install and configure:

- Tailwind CSS
- Shadcn UI
- React Router
- Axios
- TanStack Query
- React Hook Form
- Zod
- Recharts

Create the project structure:

src/

api/
components/
features/
hooks/
layouts/
pages/
routes/
types/
utils/

Configure:

- Axios instance
- JWT interceptor (placeholder)
- React Router
- ProtectedRoute component (placeholder)
- QueryClient
- Global Layout
- Basic navigation
- Theme provider
- Error page
- Not Found page

Create placeholder pages:

- Login
- Register
- Dashboard
- Tickets
- Admin
- Profile

The pages only need a basic layout and routing. Do not implement functionality yet.

### Testing

Configure:

Backend

- xUnit
- Moq
- Integration Test project

Frontend

- Basic testing setup if appropriate

Add at least one sample backend unit test that passes.

### Documentation

Generate:

- Complete folder tree
- Solution structure explanation
- Database schema overview
- Architecture explanation
- Backend project responsibilities
- Frontend project responsibilities

### Verification

Before finishing, verify that:

- The backend builds successfully.
- The frontend builds successfully.
- The database migration succeeds.
- Seed data is inserted.
- Swagger loads successfully.
- Health endpoint works.
- React application starts successfully.
- No build errors remain.

### Deliverables

Provide:

1. Folder structure
2. Every terminal command executed
3. Every NuGet package installed
4. Every npm package installed
5. Configuration files
6. Source code
7. EF Migration
8. SQL script
9. Explanation of all architectural decisions
10. Common pitfalls to avoid
11. Suggested Conventional Commit message

When Phase 1 is fully complete, stop and wait for my approval before implementing Phase 2.
