# Phase 3 — Ticket Management

## What this phase adds

Full CRUD ticket management on top of the entities and auth system already built in Phases 1–2. No new database tables or migration were needed — `Ticket` and `TicketHistory` already had every column this phase needed since Phase 1.

### Backend

**Application layer**:
- `Features/Tickets/` — `TicketDto` (full detail), `TicketListItemDto` (lighter, for the list view), `TicketHistoryDto`, `CreateTicketRequest`, `UpdateTicketRequest`, `TicketQueryParameters` (search/filter/sort/paging), `ITicketService`, FluentValidation validators (including one for `TicketQueryParameters` itself — page ≥ 1, page size 1–100, `sortBy` restricted to an allow-list)
- `Features/Lookups/` — `ILookupService` + `LookupItemDto`, a small read-only addition: the ticket create/edit/filter UI needs category/priority/status dropdowns, and nothing prior to this phase exposed that data over HTTP. Full CRUD *management* of lookup values is deliberately left for the administration phase — this is read-only.
- `Common/Models/PagedResult<T>` — generic paging envelope (`Items`, `TotalCount`, `Page`, `PageSize`, computed `TotalPages`)
- `Common/Exceptions/AppException.cs` gained **`ForbiddenAppException` (403)** — see the "bug found during live testing" note below

**Infrastructure — `TicketService`** (`Infrastructure/Services/TicketService.cs`) implements every operation:
- **Create** — assigns the next `TicketNumber` (`HD-000001`, ...), defaults to the `Open` status, records an initial `"Created"` history entry
- **Get / Search** — an Employee only ever sees tickets they created; Agent/Manager/Admin see everything. Enforced in the query itself, not just the UI.
- **Update** — Agents and above can change every field, including `StatusId` and `AssignedToUserId`. Employees may only edit their **own** ticket's title/description, and only while it's still `Open`. Every changed field writes a `TicketHistory` row with human-readable before/after values (category/priority/status *names*, not GUIDs; assignee *names*, not IDs) — resolved via small lookup dictionaries and user-name queries, not stored as raw IDs.
- Changing `StatusId` to `Resolved` or `Closed` automatically stamps `Ticket.ResolvedAt`/`ClosedAt` (the columns existed since Phase 1 but nothing wrote to them until now).
- **Soft Delete** — Agent+ only; goes through the ordinary `AppDbContext.Remove()`, which the global `SaveChanges` override already converts to `IsDeleted = true` (Phase 1 behavior, now actually exercised).
- **Restore** — Manager/Admin only (one tier above Delete, deliberately — undoing a removal is treated as more sensitive than performing it).
- **Search/Filter/Sort/Pagination** — `TicketQueryParameters` binds directly from the query string (`?searchTerm=&categoryId=&priorityId=&statusId=&assignedToUserId=&page=&pageSize=&sortBy=&sortDescending=`); search matches title or ticket number (`Contains`); sorting supports `title`, `priority`, `status`, `dueDate`, `ticketnumber`, defaulting to `createdAt`.
- **History** — `GET /tickets/{id}/history`, newest first, with the same access check as `GetById` (so history can't leak ticket existence/content to someone who shouldn't see the ticket at all).

**Api** — `TicketsController` (all 7 operations) and `LookupsController` (3 read-only endpoints), both `[Authorize]` by default; Delete and Restore additionally require the `RequireAgentOrAbove`/`RequireManagerOrAdmin` policies already registered in Phase 2 — reused, not reinvented.

### A bug found and fixed during live testing

Manually walking through "an Employee views someone else's ticket" returned **401 Unauthorized**. That's wrong: the caller *is* authenticated (valid token) — they're just not allowed to see that specific resource, which is **403 Forbidden**. The Phase 2 `AppException` hierarchy only had `UnauthorizedAppException`, used indiscriminately for both "who are you?" and "I know who you are, but no." This phase splits that: added `ForbiddenAppException` (403) for resource-ownership/permission checks (`TicketService`'s "not your ticket" and the redundant service-level role checks behind Delete/Restore, which the controller-level policies already gate — defense in depth), while `UnauthorizedAppException` (401) stays reserved for genuine authentication failures (bad credentials, missing/expired token, "current user could not be identified"). Verified live: bystander → 403, missing token → 401.

### Frontend

- **`features/tickets/`** — `api.ts` (typed Axios calls for all 7 operations), `schemas.ts` (Zod, mirroring the backend's FluentValidation rules), `queries.ts` (TanStack Query: `useTicketsQuery`, `useTicketQuery`, `useTicketHistoryQuery`, and mutations for create/update/delete/restore, all invalidating the `['tickets']` query key on success)
- **`features/lookups/`** — `api.ts` + `queries.ts` for categories/priorities/statuses (5-minute `staleTime`, since lookup data barely changes)
- **`TicketsPage`** (list) — search box, category/priority/status filter dropdowns, sort field + direction, pagination — all synced to the URL via `useSearchParams` so a filtered/sorted/paginated view is bookmarkable and survives a refresh
- **`CreateTicketPage`** — title, description, category, priority, optional due date
- **`EditTicketPage`** — title/description/due date always editable; category/priority/status are disabled `<select>`s for Employees (matches the backend rule — submitting them unchanged is harmless, but the UI shouldn't imply they're editable when they aren't) and enabled for Agent+; an "Assign to me" / "Unassign" button replaces a full user-picker dropdown, since building a searchable user list is an administration-phase concern this phase doesn't need
- **`TicketDetailsPage`** — full detail + a history timeline (`field changed from X to Y`, by whom, when); Edit/Delete/Restore buttons render conditionally based on the signed-in user's roles and the ticket's ownership/status, mirroring the backend's actual authorization rules (not just hiding buttons — the backend enforces all of this independently)

## Verification performed

- **34 backend unit tests pass** (13 auth + 3 misc from Phase 1/2 + **18 new `TicketServiceTests`**), covering: create (valid + unknown category), get (not found, cross-user 403, agent access), update (agent full update + history count, employee own-open-ticket success, employee cross-user 403, employee editing a non-open ticket rejected), delete (employee 403, agent success + soft-delete verified via `IgnoreQueryFilters`), restore (agent 403, manager success), search (employee scoping, agent sees all, status filter, pagination math), and history ordering.
- **Live end-to-end via `curl`** against the real API + LocalDB: register two users, promote one to `IT Support Agent` then `Manager` via direct SQL (no user-management UI exists yet — expected, that's a later phase), then: create → get → cross-user 403 → agent reassigns + resolves (auto-stamps `ResolvedAt`) → employee blocked from editing the now-Resolved ticket → history shows all 4 changes with readable names → agent delete → 404 for everyone → manager restore → search/filter/sort/pagination all confirmed against a 4-ticket dataset → invalid `page=0` correctly rejected with a 400.
- **Cross-origin verified**: the exact `GET /tickets` and `GET /lookups/categories` calls the React app makes, sent with `Origin: http://localhost:5173`, succeed with correct CORS headers.
- **Frontend**: `npm run build` (0 TypeScript errors) and `npm run lint` (0 errors, same 3 pre-existing benign fast-refresh warnings from Phases 1–2).

## Known limitations (by design, for this phase)

- `TicketNumber` generation (`count + 1`) has a race condition under concurrent creates — acceptable at this stage; a production system would use a DB sequence or a different numbering strategy.
- No comments or attachments UI yet — `TicketComment`/`TicketAttachment` entities exist (Phase 1) but their CRUD isn't part of this phase's scope per the spec, so they're untouched.
- No searchable "assign to any agent" picker — only "assign to me / unassign" for the currently signed-in agent, to avoid pulling a user-management endpoint into this phase.
