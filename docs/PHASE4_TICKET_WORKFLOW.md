# Phase 4 — Ticket Workflow

## What this phase adds

Ticket assignment, collaboration, and notifications on top of the CRUD/search foundation from Phase 3. One new table (`TicketAssignments`); everything else reuses Phase 1–3 entities (`TicketComment`, `Notification`, `TicketHistory`) that existed but were unused until now.

### Backend

**Assignment** (`Features/Assignments/`, `Infrastructure/Services/AssignmentService.cs`)
- **Manual assignment / reassignment** — `POST /tickets/{id}/assign` with `{ assignedToUserId }` (`null` unassigns). Agent+ only. The target must hold an Agent+ role (Admin/Manager/IT Support Agent) — a ticket can't be assigned to the reporting Employee.
- **Round-robin auto-assignment** — `POST /tickets/{id}/auto-assign`. Rotates strictly through users in the **IT Support Agent** role, ordered by user ID for a stable rotation. The "last assigned agent" is derived by querying the most recent `TicketAssignments` row with `AssignmentType == RoundRobin` — no separate singleton counter/state row was needed; the assignment table itself is the source of truth, which also makes the rotation correct across process restarts and naturally auditable.
- **Assignment history** — `GET /tickets/{id}/assignments`, its own table (not reused from `TicketHistory`) because it carries richer structured data than a generic field-diff: previous assignee, new assignee, who performed the assignment (`null` for round robin — that's the "system did it" signal), and `AssignmentType`.
- Assignment still also writes one `TicketHistory` "AssignedTo" row (Phase 3's generic diff feed), so nothing already depending on that endpoint regressed. The frontend's unified timeline drops that redundant row and shows the richer assignment-table entry instead.
- **`AssignedToUserId` was removed from `UpdateTicketRequest`.** In Phase 3 it was one of several fields an Agent+ could set via the general ticket update. Now that assignment is a first-class workflow action with its own history, notifications, and role validation, mixing it back into a generic PUT would mean two code paths could both mutate assignment with different guarantees. It's exclusively a Phase 4 endpoint now.

**Comments & Internal Notes** (`Features/Comments/`, `Infrastructure/Services/CommentService.cs`)
- One `TicketComment` table, one `IsInternal` flag, shared `GET/POST /tickets/{id}/comments`. Internal notes (`isInternal: true`) can only be **created** by Agent+, and are filtered out of the response entirely for anyone else — an Employee never sees an internal note even on their own ticket, and never learns one exists.
- **@Mentions** use inline markdown-style tokens: `@[Display Name](userId)`, the same convention GitHub/GitLab comments use. `MentionParser` (Application/Common/Utils) extracts mentioned user IDs via regex; the frontend renders the same token as a highlighted pill instead of raw text.
- Posting a comment notifies: the ticket creator and current assignee (excluding the author, and excluding the creator entirely if the comment is internal — an internal note must never notify the Employee who filed the ticket), plus anyone @mentioned. A mention inside an **internal** note additionally checks the mentioned user's role before notifying — mentioning the ticket's Employee reporter in an internal note does not notify them, since that would leak the existence of an internal note through a side channel even though they can't read it directly.

**Notifications** (`Features/Notifications/`, `Infrastructure/Services/NotificationService.cs`)
- Replaces Phase 1's `NoOpNotificationService` placeholder with a real implementation: persists a `Notification` row and calls the email stub for every dispatch (ticket assigned/reassigned, new comment, new internal note, mention).
- `GET /notifications` (paged, `unreadOnly` filter), `GET /notifications/unread-count`, `POST /notifications/{id}/read`, `POST /notifications/read-all` — all scoped to the caller; marking someone else's notification read is a 403, not a 404 (its existence isn't hidden, since the ID isn't guessable-sensitive, but acting on it is forbidden).
- **Email Notification Stub** — `IEmailSender.SendNotificationEmailAsync` extends the same stub interface Phase 2 introduced for password-reset emails, logging the subject/body via Serilog instead of sending real mail (`LoggingEmailSender`, unchanged pattern).
- A `NotifyUserAsync` call for a user ID that doesn't exist (e.g. a garbage/stale mention token) silently no-ops rather than throwing — a malformed mention shouldn't crash the comment post.

**Shared infrastructure cleanup** — Introducing three services that all need "can this user see this ticket," "resolve a user's display name," and "record a history row" (`TicketService`, `AssignmentService`, `CommentService`) turned duplicated private methods from Phase 3 into a real cross-cutting need. Extracted to `Infrastructure/Services/Shared/`: `TicketAccessGuard`, `UserDisplayNameResolver`, `TicketHistoryRecorder`. `TicketService` was refactored to use them too, with no behavior change (covered by the existing Phase 3 test suite still passing unmodified).

**Api** — `TicketsController` gained `assign`, `auto-assign`, `assignments`, and `comments` (GET/POST) actions; new `NotificationsController`; `LookupsController` gained `GET /lookups/agents` (Agent+ only) so the Assignment Panel and mention autocomplete have a name to show instead of a raw GUID.

### Frontend

- **`AssignmentPanel`** — current assignee, an agent picker + Assign/Reassign button, an Auto-assign (round robin) button, and Unassign when applicable. Agent+ only (an Employee never sees this panel).
- **`CommentsPanel`** (reused twice) — one instance renders public comments (everyone), a second renders internal notes (Agent+ only, both to view and post) — mirroring the backend's visibility split exactly rather than hiding a single feed with CSS.
- **`MentionTextarea`** — typing `@` filters a candidate dropdown (the ticket's creator/assignee, plus the full agent list for Agent+ users) and inserts the `@[Name](id)` token on selection.
- **`MentionText`** — renders stored comment content, turning `@[Name](id)` tokens into a highlighted pill instead of showing the raw markup.
- **`NotificationCenter`** — a bell icon in the navbar with an unread-count badge, polling every 30s (`TanStack Query` `refetchInterval` — this phase doesn't add push/WebSocket delivery, matching the ticket-workflow scope; real-time delivery remains a candidate for a later phase). Clicking a notification marks it read and navigates to the related ticket; "mark all read" clears the badge.
- **`TicketTimeline`** — merges ticket history, assignment history, and comments into one chronological feed (built client-side via `buildTimeline`, sorted by timestamp), replacing Phase 3's plain history-only list on the ticket details page.

## Verification performed

- **53 backend unit tests pass** (18 from Phase 3 + **new**: 7 `AssignmentServiceTests` covering manual assign/reassign/unassign, role validation, round-robin rotation across agents, no-eligible-agents rejection, and history ordering; 8 `CommentServiceTests` covering public/internal visibility, the internal-note-authoring permission check, mention extraction + notification, and the internal-note-mention-leak protection; 8 `NotificationServiceTests` covering persistence + email stub dispatch, unknown-user no-op, pagination, unread filtering/counting, mark-read, mark-all-read, and cross-user protection) + 3 integration tests — all passing.
- **Live end-to-end via `curl`** against the real API + LocalDB: registered a fresh Employee and two fresh Agents, promoted the agents via direct SQL (still no user-management UI — later phase); then: Employee blocked from assigning (403) → Agent manually assigns → assignment history shows it → assigning to a non-agent user rejected (400) → Employee posts a public comment mentioning an agent → Employee blocked from posting an internal note (403) → Agent posts an internal note → Employee's `GET comments` excludes it, Agent's includes both → mentioned agent's notification feed shows both the assignment and mention notifications → mark-one-read and mark-all-read both confirmed via unread-count → round-robin auto-assign across 3 new tickets correctly rotated through all 3 eligible agents with no repeats → an internal note mentioning the Employee reporter confirmed **not** to produce a notification for them (unread count unchanged) → the email stub's log lines confirmed in `Logs/log-*.txt` for every dispatch.
- **Frontend**: `npm run build` (0 TypeScript errors) and `npm run lint` (0 errors, same 3 pre-existing benign fast-refresh warnings from Phases 1–3).
- **Not verified**: no browser-automation tool was available in this environment, so the new UI (Assignment Panel, mention autocomplete/highlighting, Notification Center) was not clicked through in an actual browser — only compiled/linted and exercised through its API layer. Worth a manual pass before considering the UI itself finished.

## Known limitations (by design, for this phase)

- Notification delivery is poll-based (30s), not push/real-time — explicitly out of scope for this phase; a later phase can add SignalR/WebSockets without changing the `INotificationService` contract.
- Round robin only rotates through the **IT Support Agent** role, not Managers/Admins — manual assignment still allows any Agent+ user as a target.
- No UI to browse *all* notifications beyond the last 10 in the dropdown (no dedicated "all notifications" page) — the spec asked for a Notification Center, not a full notification management screen.
- Mention candidates in the compose box are limited to the ticket's creator/assignee (any user) and, for Agent+ users, the full agent list — an Employee can't mention an arbitrary agent who isn't already on the ticket, since exposing a full user directory to Employees is an administration-phase concern.
