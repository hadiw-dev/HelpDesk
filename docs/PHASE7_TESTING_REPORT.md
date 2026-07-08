# Phase 7 — Testing Report

Generated from an actual `dotnet test HelpDesk.sln` run against this codebase. Both projects run against the **real** LocalDB (see `HelpDesk.IntegrationTests/Infrastructure/TestWebApplicationFactory.cs` — it boots the app in the `Development` environment, not an isolated in-memory database), so integration tests use GUID-suffixed emails/values to avoid collisions across repeated runs, matching this project's existing convention.

## Result summary

| Project | Test methods (files) | Passing | Failing | Skipped |
|---|---|---|---|---|
| `HelpDesk.Tests` (unit) | 105 methods → **114** test cases (Theory/InlineData expands some methods into multiple cases) | 114 | 0 | 0 |
| `HelpDesk.IntegrationTests` (integration/API) | 33 methods → **35** test cases | 35 | 0 | 0 |
| **Total** | | **149** | **0** | **0** |

Full solution run: `Passed! - Failed: 0, Passed: 149, Total: 149`.

## Unit tests (`HelpDesk.Tests`) — xUnit + Moq, in-memory EF Core provider

| File | Tests | Covers |
|---|---|---|
| `Tickets/TicketServiceTests.cs` | 18 | Create/Get/Update/Delete/Restore, search/filter/sort/paging, ownership rules, history recording |
| `Attachments/AttachmentServiceTests.cs` | 12 | Upload (valid/oversized/disallowed-extension), **magic-byte signature mismatch (new)**, **matching signature accepted (new)**, download, delete (owner/non-owner/agent), list-for-ticket |
| `Dashboard/DashboardServiceTests.cs` | 9 | KPI summary, category/priority breakdown, monthly trend, resolution time, SLA report |
| `Auth/AuthServiceTests.cs` | 9 | Register (new/duplicate email), login (success/wrong-password/inactive/locked-out/unknown), refresh token (valid/revoked/missing), **failed-login activity logging for all 3 failure paths (new)**, **successful-login activity logging (new)** |
| `Admin/AdminUserServiceTests.cs` | 9 | Admin create/update/change-role/delete/list users |
| `Notifications/NotificationServiceTests.cs` | 8 | Dispatch, mark-read, mark-all-read, unread count, mention-notification authorization check |
| `Comments/CommentServiceTests.cs` | 8 | Public comments, internal notes (Agent+ only), @mention parsing and targeted notification |
| `Assignments/AssignmentServiceTests.cs` | 7 | Manual assign, round-robin auto-assign, reassignment, assignment history |
| `Admin/AdminLookupServiceTests.cs` | 6 | Generic CRUD for Category/Priority/Status via the shared `AdminLookupService<T>` |
| `Common/FileSignatureValidatorTests.cs` **(new)** | 5 methods / 14 cases | Magic-byte match per extension (pdf/png/jpg/jpeg/zip/docx/xlsx), mismatch rejected, unrecognized extension (`.txt`) passes through unchecked, header shorter than signature, empty header |
| `Admin/ActivityLogQueryServiceTests.cs` | 4 | Activity log search/filter/paging |
| `Admin/SystemSettingsServiceTests.cs` | 3 | Get-or-create singleton settings row, update |
| `Persistence/SoftDeleteTests.cs` | 2 | Global query filter excludes soft-deleted rows, `SaveChanges` override stamps deletion |
| `Dashboard/ReportServiceTests.cs` | 2 | PDF and Excel report generation produce non-empty byte output |
| `Auth/TokenServiceTests.cs` | 2 | Access token claims, refresh token generation |
| `Common/AutoMapperConfigurationTests.cs` | 1 | All mapping profiles have valid configuration |

## Integration / API tests (`HelpDesk.IntegrationTests`) — `WebApplicationFactory<Program>`, real HTTP pipeline, real LocalDB

All five test classes share **one** `TestWebApplicationFactory` instance via an xUnit collection fixture (`[Collection(ApiCollection.Name)]`) rather than each declaring its own `IClassFixture` — necessary because `Program.cs`'s Serilog bootstrap freezes a process-wide static logger the first time a host builds; a second independent factory in the same process would throw building its own host. See `docs/PITFALLS.md`.

| File | Tests | Covers |
|---|---|---|
| `AuthFlowTests.cs` **(new)** | 10 | Register (success/duplicate-conflict), login (success/wrong-password/unknown-email), refresh-token (valid/invalid), get-profile (no token → 401, valid token → own profile, tampered signature → 401) |
| `TicketApiTests.cs` **(new)** | 8 | Full HTTP CRUD lifecycle (create → get → update → delete), 404 for missing ticket, 400 for missing required fields, 403 for cross-user access and Employee-attempts-delete, search returns the created ticket |
| `AuthorizationTests.cs` **(new)** | 7 | 401 with no token / 403 with wrong role / 200 with correct role, for both `Roles="Admin"` and `RequireManagerOrAdmin`-policy endpoints; ticket-delete 403 for Employee; admin-users endpoint 401 with no token |
| `SecurityTests.cs` **(new)** | 5 methods / 7 cases | Security headers present on every response, CORS allows the allowlisted origin and omits the header for a disallowed one, SQL-injection-style search payloads (3 variants) return 200 not 500, `<script>` tag in a ticket title round-trips as inert JSON text |
| `ApiSmokeTests.cs` | 3 | Health endpoint, versioned ping endpoint, Swagger JSON document availability |

### What the Security tests actually verify

- **SQL injection**: EF Core's LINQ query provider parameterizes every value — there is no raw SQL or string concatenation anywhere in the codebase (confirmed by code review during the Phase 7 security audit). The test sends `' OR '1'='1`, `'; DROP TABLE Tickets; --`, and a blind-injection variant as a search term and asserts a normal `200` response with a valid paged result, not a `500` or evidence of the table being dropped.
- **XSS**: this is a JSON API with a React SPA frontend — the correct defense is that a `<script>` payload comes back as an inert JSON string (never executed server-side) and the frontend escapes it on render (React's default JSX behavior, not something this backend controls). The test asserts the response `Content-Type` is `application/json` and the payload round-trips unchanged, not that it gets stripped or rejected.
- **CORS**: verified from both directions — the allowlisted origin (`http://localhost:5173`) gets `Access-Control-Allow-Origin` echoed back; an arbitrary origin gets no such header at all. That header's absence is what makes a browser reject the response client-side; the server doesn't need to (and doesn't) reject the request itself.
- **JWT**: `AuthFlowTests`' tampered-signature test flips one character in a real, valid access token's signature segment and confirms the API rejects it with `401` rather than accepting a payload whose signature no longer verifies.

## Manual / live verification performed this phase

- `dotnet build HelpDesk.sln` and `dotnet test HelpDesk.sln` re-run after every change batch (not just once at the end).
- `npm run build` and `npm run lint` (frontend) re-run after the ErrorBoundary, code-splitting, and accessibility changes.
- Frontend dev server smoke-tested via `curl` against `/` and `/src/main.tsx` to confirm it serves without a build-time error.

## Not covered by automated tests (disclosed gaps)

- No frontend unit/component tests (Vitest/Testing Library) exist in this project as of Phase 7 — out of this phase's explicit scope (which asked for backend xUnit/Moq/integration/API tests), but a real gap before calling the frontend production-ready.
- No load/performance testing.
- No automated accessibility audit tool (e.g. axe-core) run against the frontend — the accessibility pass in this phase was a manual code-level review (missing labels/aria-attributes), not a tooled audit.
