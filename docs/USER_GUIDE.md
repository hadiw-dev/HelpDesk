# User Guide

A walkthrough of the application from each role's perspective. For API-level detail see `docs/api-guide.md`; for how the system is built see `docs/ARCHITECTURE.md`.

## Roles

| Role | Can do |
|---|---|
| **Employee** | Submit tickets, view/comment on their own tickets, upload attachments to their own tickets, view their own profile |
| **IT Support Agent** | Everything an Employee can, plus: view/act on *all* tickets, assign/reassign tickets, write internal notes (hidden from Employees), delete/restore tickets |
| **Manager** | Everything an Agent can, plus: restore soft-deleted tickets, view dashboards/reports across the whole system |
| **Admin** | Everything a Manager can, plus: manage users and roles, manage Categories/Priorities/Statuses, view the system-wide activity log, edit system settings (upload size limits, allowed file types, site name, default page size) |

The four roles are fixed — there is no UI for creating custom roles (a deliberate scope decision; see `docs/PITFALLS.md`).

## Getting started

1. **Register** (`/register`): email, password, first/last name. New accounts default to the Employee role.
2. **Log in** (`/login`): issues a short-lived access token (refreshed automatically in the background) and a longer-lived refresh token.
3. You land on the **Dashboard** — KPI summary cards, ticket breakdowns by category/priority, a monthly trend chart, and (for Agent+) an SLA compliance section.

## As an Employee: submitting and tracking a ticket

1. **Tickets → New Ticket**: fill in Title, Description, Category, Priority, and an optional Due Date. Submitting creates the ticket in **Open** status, automatically assigned a ticket number (`HD-000123`).
2. **Tickets** list shows only tickets you created (Agents and above see everyone's). Search by title/ticket number, filter by category/priority/status, sort, and page through results.
3. Open a ticket to see its **Timeline** — a merged, chronological feed of status changes, assignments, and comments — plus a **Comments** panel (public comments only; internal notes are hidden from you) and an **Attachments** panel (upload/download files up to the admin-configured size limit and allowed types).
4. You can edit or delete your own ticket only while it's still **Open**; once an Agent starts working it (status moves past Open), further changes go through comments instead.

## As an IT Support Agent: working the queue

1. **Tickets** shows the full queue across all Employees. Filter by status/priority/category, or by "assigned to me" via the assignable-agents lookup.
2. Open any ticket and use the **Assignment panel** to assign it to a specific agent, or **Auto-assign (round robin)** to let the system pick the next agent in rotation based on assignment history — no manual "whose turn is it" tracking needed.
3. Use **Internal Notes** (a second comments panel, marked "internal") for agent-to-agent or agent-to-manager communication that the ticket's Employee creator can never see — useful for noting vendor contact info, escalation reasoning, etc. Regular **Comments** are visible to the Employee too; use those for status updates you want them to see.
4. **@Mention** a teammate in any comment (`@[Jane Doe]`, autocompletes as you type) to trigger an in-app notification for them — the notification bell in the top nav shows an unread badge and a dropdown feed. Mentioning someone in an internal note only notifies them if they're actually allowed to see internal notes (Employees are silently excluded even if mentioned by name, so an internal note's existence never leaks to them).
5. Delete a ticket (soft delete — recoverable) if it's a duplicate/spam; a Manager or Admin can restore it later.

## As a Manager: oversight and reporting

Everything above, plus:

- **Dashboard**: KPI cards (open/in-progress/resolved counts, average resolution time), category/priority pie charts, a monthly ticket-volume line chart, and an SLA section showing compliance % (measured against each ticket's own Due Date) with a breached-ticket table.
- **Reports**: export the same dashboard data as a formatted **PDF** or **Excel** file, with a date-range filter, for sharing outside the app.
- **Restore** a soft-deleted ticket that an Agent removed.

## As an Admin: system administration

Everything above, plus an **Admin** area:

- **User Management**: create accounts directly (skipping self-registration), edit names/department/job title, activate/deactivate accounts, and change a user's role — this is the only way to promote someone to Agent/Manager/Admin.
- **Category / Priority / Status Management**: add, rename, reorder, or soft-delete lookup values used across every ticket form. Deleting one doesn't affect tickets already using it — they keep their (now-hidden) reference.
- **System Settings**: site name, max file upload size (MB), allowed file extensions (comma-separated), default page size — these are enforced live by the upload validation and pagination throughout the app, not just cosmetic.
- **Activity Log**: a searchable, paginated audit trail of every significant action system-wide — logins (including failed attempts), registrations, ticket/comment/attachment changes, role changes, password resets, and more. Filter by action type.

## Account & profile

- **Profile** page: view/update your own name, department, and job title; change your password.
- **Forgot your password?** (`/forgot-password`): requests a reset email (in this project's current state, "sending" an email logs it to the server console/log file via `LoggingEmailSender` rather than a real mail provider — see `docs/PITFALLS.md` and the Future Improvements list in the README).
- Sessions persist across browser restarts (refresh token stored client-side) until you explicitly log out or the refresh token expires (7 days by default).

## Accessibility

Every form control has an accessible name (visible label or `aria-label`), icon-only buttons (theme toggle, logout, notifications) are labeled for screen readers, and the whole app is keyboard-navigable using standard Tab/Enter/Space/Arrow interactions on native HTML form controls.
