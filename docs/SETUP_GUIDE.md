# Setup Guide

Two ways to run this project locally: **native** (.NET + Node installed directly on your machine) or **Docker Compose** (everything containerized, including SQL Server). This guide covers the native path in detail; see [`DEPLOYMENT_GUIDE.md`](DEPLOYMENT_GUIDE.md) for Docker Compose.

## Prerequisites (native setup)

| Tool | Version | Check with |
|---|---|---|
| .NET SDK | 8.0.x | `dotnet --version` |
| Node.js | 20.x+ | `node --version` |
| npm | 10.x+ | `npm --version` |
| SQL Server LocalDB (or SQL Server Express/Developer) | any recent | `sqllocaldb info` |
| Git | any recent | `git --version` |

SQL Server LocalDB ships with Visual Studio and the "SQL Server Express LocalDB" component of the SQL Server installer — it's the lightest option for local development and is what `appsettings.Development.json` targets by default.

## 1. Clone and restore

```bash
git clone <your-fork-or-origin-url> HelpDeskSystem
cd HelpDeskSystem
```

## 2. Backend setup

```bash
cd backend
dotnet restore HelpDesk.sln
```

### Configure secrets

The API refuses to start without a JWT signing key of at least 32 bytes (a fail-fast check added in Phase 7) and a connection string. `appsettings.Development.json` already provides the LocalDB connection string; the JWT secret is kept out of any committed file and lives in .NET User Secrets:

```bash
cd src/HelpDesk.Api
dotnet user-secrets set "Jwt:SecretKey" "a-random-string-at-least-32-characters-long"
```

Generate a real random value rather than typing something memorable — e.g. `openssl rand -base64 48` (Git Bash/WSL/macOS/Linux) or `[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(48))` (PowerShell).

### Apply the database migration

```bash
cd backend/src/HelpDesk.Api
dotnet ef database update
```

This creates the `HelpDeskSystemDb` database on your LocalDB instance and seeds the lookup data (Categories, Priorities, Statuses) and the four fixed Identity roles (Admin, Manager, IT Support Agent, Employee). If you don't have `dotnet-ef` installed: `dotnet tool install --global dotnet-ef`.

### Run the API

```bash
cd backend/src/HelpDesk.Api
dotnet run
```

By default this listens on `http://localhost:5019` (see `Properties/launchSettings.json`'s `http` profile). Confirm it's up:

```bash
curl http://localhost:5019/health
curl http://localhost:5019/swagger/v1/swagger.json
```

Swagger UI is browsable at `http://localhost:5019/swagger`.

## 3. Frontend setup

```bash
cd frontend
npm install
npm run dev
```

Opens on `http://localhost:5173` (Vite's default). `frontend/.env.development` already points `VITE_API_BASE_URL` at `http://localhost:5019/api/v1` — no changes needed for the native setup.

## 4. First login

Register a new account through the UI (`http://localhost:5173/register`) — new registrations default to the **Employee** role. To test Agent/Manager/Admin-only features, either:

- Promote a user's role directly via SQL (`UPDATE AspNetUserRoles ...` or, simpler, use the seeded `AspNetRoles` table + `AspNetUserRoles` join), or
- Register a second account, then have an existing Admin promote it via **Admin → User Management → change role** once at least one Admin account exists.

There is no seeded Admin account out of the box — the first Admin has to be created by direct database manipulation (a deliberate scope decision from earlier phases; see `docs/PITFALLS.md`).

## 5. Running the test suite

```bash
cd backend
dotnet test HelpDesk.sln
```

149 tests (114 unit + 35 integration) should pass. The integration tests hit the **real** LocalDB, not a mock — make sure step 2's migration has been applied first.

```bash
cd frontend
npm run build
npm run lint
```

## Common problems

See `docs/PITFALLS.md` for the full list. The most common ones during first setup:

- **"Jwt:SecretKey must be configured and at least 32 bytes long"** — you skipped the `dotnet user-secrets set` step above, or the value is too short.
- **Registration/login silently fails in the browser** — almost always means the backend process isn't running, or a second Vite dev server started on a different port (5174/5175) than the one your browser has open, causing a CORS-origin mismatch. Check `netstat`/Task Manager for stray `dotnet`/`node` processes before assuming it's a code bug.
- **`dotnet ef` commands fail with `HostAbortedException`** — this is expected output from the Serilog bootstrap pattern, not a real failure; look for the actual migration result above it.
