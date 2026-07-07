# API Guide

All endpoints are versioned under `/api/v1/` and (except `/auth/register` and `/auth/login`) require a JWT bearer token. Swagger UI at `http://localhost:5019/swagger` documents every endpoint interactively — click **Authorize** and paste `Bearer <your-access-token>` to try requests directly from the browser.

## Authentication

### Register

```
POST /api/v1/auth/register
Content-Type: application/json

{
  "email": "employee@helpdesk.local",
  "password": "Passw0rd1!",
  "confirmPassword": "Passw0rd1!",
  "firstName": "Emma",
  "lastName": "Reporter"
}
```

Response `200 OK` — an `accessToken`/`refreshToken` pair plus the created user's profile (new registrations default to the `Employee` role):

```json
{
  "accessToken": "eyJhbGciOi...",
  "accessTokenExpiresAt": "2026-07-07T09:11:28Z",
  "refreshToken": "WPu1lROM...",
  "user": {
    "id": "1d4b0c80-...",
    "email": "employee@helpdesk.local",
    "firstName": "Emma",
    "lastName": "Reporter",
    "roles": ["Employee"]
  }
}
```

### Login / Refresh

```
POST /api/v1/auth/login        { "email": "...", "password": "..." }
POST /api/v1/auth/refresh-token { "refreshToken": "..." }
```

Both return the same shape as register. Access tokens are short-lived (15 minutes in production config); the frontend's Axios interceptor automatically calls `refresh-token` on a `401` and retries the original request.

## Tickets

```
GET  /api/v1/tickets?searchTerm=&categoryId=&priorityId=&statusId=&page=1&pageSize=20&sortBy=createdAt&sortDescending=true
POST /api/v1/tickets
GET  /api/v1/tickets/{id}
PUT  /api/v1/tickets/{id}
DELETE /api/v1/tickets/{id}          (Agent+ only)
POST /api/v1/tickets/{id}/restore    (Manager/Admin only)
GET  /api/v1/tickets/{id}/history
```

An **Employee** only ever sees tickets they created; **Agent/Manager/Admin** see every ticket. This is enforced server-side, not just hidden in the UI.

Create example:

```json
POST /api/v1/tickets
{
  "title": "Laptop won't boot",
  "description": "Black screen on startup after the latest update.",
  "categoryId": "c0000000-0000-0000-0000-000000000001",
  "priorityId": "d0000000-0000-0000-0000-000000000002",
  "dueDate": "2026-07-10"
}
```

## Assignment (Agent+ only)

```
POST /api/v1/tickets/{id}/assign        { "assignedToUserId": "<agent-user-id>" }   // or null to unassign
POST /api/v1/tickets/{id}/auto-assign                                              // round-robin
GET  /api/v1/tickets/{id}/assignments                                              // assignment history
```

## Comments & Internal Notes

```
GET  /api/v1/tickets/{id}/comments
POST /api/v1/tickets/{id}/comments
{
  "content": "Hi @[Alex Agent](3a1b...), any update on this?",
  "isInternal": false
}
```

`isInternal: true` requires an Agent+ role and is invisible to the ticket's Employee owner. Mentions use the inline token `@[Display Name](userId)`; the frontend renders it as a highlighted pill.

## Notifications

```
GET  /api/v1/notifications?page=1&pageSize=20&unreadOnly=false
GET  /api/v1/notifications/unread-count
POST /api/v1/notifications/{id}/read
POST /api/v1/notifications/read-all
```

## Dashboard & Reports

```
GET /api/v1/dashboard/kpi-summary?dateFrom=2026-01-01&dateTo=2026-06-30
GET /api/v1/dashboard/tickets-by-category
GET /api/v1/dashboard/tickets-by-priority
GET /api/v1/dashboard/monthly-tickets
GET /api/v1/dashboard/resolution-time
GET /api/v1/dashboard/sla-report
GET /api/v1/dashboard/reports/pdf     (downloads a PDF)
GET /api/v1/dashboard/reports/excel   (downloads an .xlsx)
```

`dateFrom`/`dateTo` are optional on every dashboard endpoint. Results are scoped the same way ticket search is — an Employee's dashboard only reflects their own tickets.

## Error Format

All errors follow RFC 7807 `ProblemDetails`, produced by a global exception-handling middleware:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Ticket not found.",
  "status": 404
}
```

Validation errors (400) additionally include an `errors` dictionary keyed by field name, which the frontend surfaces next to the relevant form field.
