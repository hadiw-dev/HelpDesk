# Phase 6 — Administration

## What this phase adds

Admin-facing management screens and secure ticket file attachments — the last two gaps this project had been deliberately deferring since Phase 1 ("full CRUD management of lookup values is an administration-phase concern," "no user-management UI exists yet"). One new table (`SystemSettings`); everything else builds on entities that already existed (`TicketAttachment` since Phase 1, `ActivityLog` since Phase 2, `Category`/`Priority`/`Status` since Phase 1).

### Backend

**Secure ticket file attachments** (`Features/Attachments/`, `Infrastructure/Services/AttachmentService.cs` + `LocalFileStorageService.cs`)
- `POST/GET /tickets/{id}/attachments`, `GET /tickets/{id}/attachments/{attachmentId}/download`, `DELETE /tickets/{id}/attachments/{attachmentId}` — access follows the same rule as the ticket itself (`TicketAccessGuard`, reused from Phase 4/5), not a separate admin-only concern.
- **Storage outside web root**: files are saved under a configurable root (`FileStorage:RootPath` in `appsettings.json`, resolved to `backend/file-storage/` in dev) — a sibling of `backend/src/`, never inside the Api project's own directory tree, and there is no static-file middleware configured at all. The *only* way to read a file back is `GET .../download`, which runs the same access check as every other ticket endpoint before streaming bytes through the app.
- **Validation**: file extension is checked against an admin-configurable whitelist and size against an admin-configurable limit (both live in the new `SystemSettings` table — see below), and the file is always stored under a server-generated GUID name (`IFileStorageService.SaveAsync`), never the caller-supplied filename, so nothing about the upload path is attacker-controlled.
- **Authorization**: upload/download/list follow ticket access; delete additionally requires being the uploader or Agent+.

**Admin APIs**, all under `[Authorize(Policy = "RequireAdmin")]`:
- **Users** (`AdminUsersController` / `IAdminUserService`) — search/filter/paginate, create (this is what finally replaces the raw-SQL role promotions every prior phase's live-testing needed), update profile/active-status, change role (single-role swap — remove all current roles, add the new one), soft-delete. Self-delete, self-role-change, and self-deactivation are all blocked server-side so an Admin can't lock themselves out.
- **Categories / Priorities / Statuses** (`AdminLookupsController` / generic `IAdminLookupService<TEntity>`) — full CRUD, deferred from Phase 3's read-only dropdowns. One generic service backs all three (they're identical in shape); delete soft-deletes via the same global override tickets already use, so existing tickets referencing a deleted lookup value keep their (now-hidden) reference rather than breaking a foreign key.
- **Activity Logs** (`AdminActivityLogsController` / `IActivityLogQueryService`) — read side of the activity log every phase has been writing to since Phase 2; filter by user/action/date range, paginated.
- **System Settings** (`AdminSettingsController` / `ISystemSettingsService`) — a genuinely-wired settings screen, not a decorative form: `MaxFileUploadSizeMb` and `AllowedFileExtensions` are read by `AttachmentService` on every upload, so changing them here immediately changes what the upload endpoint accepts.
- **"Role Management"** from the spec is folded into User Management (assign/change a user's role inline) rather than a separate CRUD screen — the 4 roles are fixed and hardcoded into every authorization policy in this system (`RequireAdmin`, `RequireManagerOrAdmin`, `RequireAgentOrAbove`), so letting an Admin create arbitrary new roles wouldn't integrate with any of them.

### Frontend

- **Admin Dashboard** (`AdminPage`, full rewrite) — a landing page linking to each management screen.
- **User Management** — search/filter, inline role-change dropdown and active/inactive toggle per row, an expandable edit form for profile fields, and a create-user form.
- **Category / Priority / Status Management** — one generic `LookupManagementPage` component reused for all three (matching the backend's generic service), each a thin wrapper page passing its resource name.
- **System Settings** — a form for the four settings fields, using React Hook Form's `values` option to sync with the loaded settings (the same pattern `EditTicketPage` established in Phase 3, rather than `useEffect` + `setState`).
- **Activity Log Viewer** — filterable, paginated table.
- **Attachments Panel** on the ticket details page — upload button, list with download links, delete (uploader or Agent+).
- New `AdminRoute` guard wraps every `/admin/*` route (redirects non-Admins to the dashboard); the Navbar's "Admin" link is now only rendered for Admin users.

## Verification performed

- **99 backend tests pass** (96 unit incl. **32 new** + 3 integration): `AttachmentServiceTests` (valid upload, disallowed extension, oversized file, cross-user access denial for upload/download, delete by uploader vs. non-uploader vs. Agent, list visibility), `AdminUserServiceTests` (create + role assignment, duplicate-email conflict, profile update, self-deactivate/self-role-change/self-delete all blocked, role change removes old roles, search filtering), `AdminLookupServiceTests` (ordering by `DisplayOrder`, create, duplicate-name conflict, update, soft-delete — run against `Category`/`Priority`/`Status` to prove the generic service works for all three), `ActivityLogQueryServiceTests` (newest-first ordering, action filter, null-user → "System", pagination), `SystemSettingsServiceTests` (get-or-create singleton row, idempotent on repeated calls, update persists).
- **Live verification via `curl`** against the real API + LocalDB: an Employee is blocked (403) from every `/admin/*` endpoint; an Admin creates a new IT Support Agent account directly (no more raw SQL role promotions); the Admin is blocked (400) from changing their own role or deleting their own account; duplicate category name correctly rejected (409); System Settings get/update round-trips correctly; Activity Log viewer shows real historical entries (60+) with resolved user names; a real ticket attachment upload/download cycle confirmed end-to-end — valid `.txt` accepted, `.exe` rejected (400), the uploader and an Agent can both download, an unrelated Employee gets 403, and the file was confirmed to physically exist at `backend/file-storage/<guid>.txt` — outside `backend/src/` entirely.
- **Frontend**: `npm run build` (0 TypeScript errors) and `npm run lint` (0 errors — one real error was caught and fixed: `SystemSettingsPage` initially synced loaded settings into local state via `useEffect` + `setState`, which `eslint-plugin-react-hooks`' `set-state-in-effect` rule correctly flagged as a cascading-render risk; switched to React Hook Form's `values` option instead, matching the pattern already established for this exact "sync a form with async-loaded data" problem in `EditTicketPage`).
- **Not verified in an actual browser**: no browser-automation tool was available in this environment, consistent with Phases 4/5 — the new admin screens and attachment UI were compiled, linted, and exercised via their API layer only.

## Known limitations (by design, for this phase)

- No magic-byte/content sniffing on uploads — validation is by file extension and declared `Content-Type` only. A determined user could rename a file to bypass the extension whitelist; this is a reasonable line for this phase's scope, but a production deployment should add real content-type verification.
- Deleted attachments are physically removed from disk immediately (no restore), unlike tickets — there was no "restore an attachment" requirement, so keeping the physical file around for an unused restore path would just be dead weight.
- User creation only supports the four fixed roles; there's still no way to create a *new* role through the UI, by design (see the Role Management note above).
- `MaxFileUploadSizeMb` is only enforced by application-level validation — raising it past ASP.NET Core/Kestrel's own default request body size limits (30MB) would additionally need a Kestrel configuration change, which this phase didn't need since the default (10MB) is well under that.
