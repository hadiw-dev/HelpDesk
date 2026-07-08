# Phase 8 — Production Readiness

## What this phase adds

Phase 8 doesn't add product features — it packages the application (built and hardened across Phases 1–7) for deployment: Dockerizes all three runtime components, generates the full documentation set a real handoff needs, and runs an end-to-end verification pass. Everything here is packaging, documentation, and verification — no business logic changed.

### Docker

- **`backend/Dockerfile`** — multi-stage build. Build stage restores/publishes only the `HelpDesk.Api` project and its `Domain`/`Application`/`Infrastructure` dependency graph (test projects are never copied into the image). Final stage is `mcr.microsoft.com/dotnet/aspnet:8.0`, runs as a non-root user (`USER $APP_UID`), pre-creates and opens up `/app/file-storage` and `/app/Logs` (the two directories the app writes to at runtime) before switching off root, and installs `curl` for its own `HEALTHCHECK` against `/health`.
- **`frontend/Dockerfile`** — multi-stage build. Build stage (`node:20-alpine`) runs `npm ci` + `npm run build`, with `VITE_API_BASE_URL` passed as a build `ARG` (Vite bakes `VITE_*` vars into the JS bundle at build time, so this must be a browser-reachable URL, not a Docker-internal service name). Final stage (`nginx:1.27-alpine`) serves the static build with a custom `nginx.conf` providing SPA fallback routing (`try_files ... /index.html`, so a hard refresh on `/tickets/123` doesn't 404) and a lightweight `/health` endpoint for its own `HEALTHCHECK`.
- **`docker-compose.yml`** (repo root) — three services: `sqlserver` (official `mcr.microsoft.com/mssql/server:2022-latest`, `MSSQL_PID: Express`, persisted via a named volume, healthchecked with `sqlcmd -Q "SELECT 1"`), `api` (built from `backend/Dockerfile`, waits for `sqlserver`'s healthcheck via `depends_on: condition: service_healthy`, published on host port `5019` matching every existing doc/Postman-collection convention from earlier phases), and `frontend` (built from `frontend/Dockerfile`, waits for `api`'s healthcheck, published on host port `3000`). Secrets (`MSSQL_SA_PASSWORD`, `JWT_SECRET_KEY`) come from a gitignored `.env` file, templated by a committed `.env.example`.
- **`.dockerignore`** in both `backend/` and `frontend/` keep build contexts lean (excludes `bin/`, `obj/`, `node_modules/`, `tests/`, etc.).

### Backend changes (small, container-motivated)

- **Swagger decoupled from `IsDevelopment()`** (`Program.cs`): now available in every environment. It documents API shape, not secrets, and this lets the Docker Compose stack honestly run as `ASPNETCORE_ENVIRONMENT: Production` (demonstrating a production-shaped deployment) while still satisfying this phase's "Verify: Swagger works" requirement. HSTS remains gated to non-Development, unchanged from Phase 7.
- **`ApplyMigrationsOnStartup` config flag** (`Program.cs`, off by default): when `true`, calls `AppDbContext.Database.Migrate()` right after the host builds. The existing native/local workflow (manual `dotnet ef database update`) is completely unchanged — this flag only exists so `docker compose up` on a fresh clone is genuinely self-contained, without a manual migration step. Verified safe against an already-migrated database (logs `"No migrations were applied. The database is already up to date."` and continues normally) — see Verification below. Deliberately **not** the default/always-on behavior: a multi-replica production deployment should apply migrations as a discrete release step instead of racing several instances to migrate concurrently (documented in `docs/PRODUCTION_CHECKLIST.md`).

### Documentation

New this phase:

- **`docs/SETUP_GUIDE.md`** — native (non-Docker) local development setup, prerequisites, secrets, migrations, running both apps and the test suite.
- **`docs/DEPLOYMENT_GUIDE.md`** — Docker Compose deployment: configuration, build/start, verification steps, an architecture diagram of how the browser/containers/database actually talk to each other, port reference, what's different from a hardened production deployment, and troubleshooting for the failure modes most likely on a first run.
- **`docs/USER_GUIDE.md`** — feature walkthrough from each of the four roles' perspective (Employee/Agent/Manager/Admin), plus account/profile and accessibility notes.
- **`docs/DEPLOYMENT_CHECKLIST.md`** — a pre-flight checklist for standing up the stack in any environment.
- **`docs/PRODUCTION_CHECKLIST.md`** — the honest list of what changes before a *real* internet-facing production launch (secrets management, TLS, database sizing/backups, rate-limiting, real email provider, CI/CD, image scanning), cross-referenced to exactly where each gap lives in the codebase today.
- **`docs/PRESENTATION_SUMMARY.md`** — a one-page portfolio/interview summary: elevator pitch, tech stack, architecture and security highlights, quality metrics, a 5-minute reviewer quickstart, and a documentation map.
- **`RELEASE_NOTES.md`** (repo root) — changelog by phase for the `v1.0.0` release, known limitations, upgrade notes.
- **`docs/swagger/swagger.json`** — a static export of the live OpenAPI document (49 endpoint paths), fetched from a running instance and validated as well-formed JSON.

Already satisfied by earlier phases, verified still accurate rather than duplicated (this project already ran into a real Windows case-insensitive-filesystem collision once from creating a second, differently-cased architecture doc — see `docs/PITFALLS.md` — so an existing, adequate doc is referenced, not shadowed by a new one):

- **Architecture Guide** → `docs/ARCHITECTURE.md` (Clean Architecture layering, frontend architecture, tech stack, coding standards)
- **API Documentation** → `docs/api-guide.md` (endpoint groups with example requests/responses)
- **ER Diagram Description** → `docs/database-design.md`'s "Entity Relationship Diagram" section (Mermaid diagram + design decisions) and `docs/DATABASE.md` (full schema/index detail)
- **Clean Architecture Documentation** → `docs/ARCHITECTURE.md`'s "Clean Architecture (Backend)" section
- **Postman Collection** → `postman/HelpDesk-API.postman_collection.json` (generated in Phase 7; no API surface changed this phase, so it remains accurate as-is)

## Verification performed

All verification below was run directly against the native (`dotnet run` / `npm run dev`) processes, **not** through Docker — this sandboxed environment does not have a Docker daemon available (`docker` is not on `PATH`, confirmed via both Bash and PowerShell). This is disclosed plainly, not glossed over: the Dockerfiles and compose file were written and carefully hand-reviewed against well-established .NET/nginx/SQL Server container conventions, but **were not actually built or run** in this session. If anything doesn't work on your first `docker compose up --build`, it's most likely a small, fixable issue (an image tag, a healthcheck path) rather than a fundamental design problem — see the Troubleshooting section of `docs/DEPLOYMENT_GUIDE.md`, and treat this as the one part of this phase that still needs a real run-through on a machine with Docker installed.

What **was** verified directly:

- `dotnet build HelpDesk.sln` — clean, 0 errors, after the `Program.cs` changes.
- `dotnet test HelpDesk.sln` — 149/149 passing, no regression from the Swagger/migration changes.
- Backend started natively (`dotnet run`, `ASPNETCORE_ENVIRONMENT=Development`): `curl http://localhost:5019/health` → `200`; `curl http://localhost:5019/swagger/v1/swagger.json` → `200`, valid JSON.
- Registered a real account end-to-end via `curl POST /api/v1/auth/register` against the real LocalDB → `200`, valid JWT returned — confirms the database connection, migrations, and Identity are all working together.
- Restarted the backend with `ApplyMigrationsOnStartup=true` against the already-migrated dev database → logged `"No migrations were applied. The database is already up to date."`, started normally, health check still passed. Confirms the new flag is safe to enable unconditionally in the Docker Compose stack.
- Frontend: `npm run build` (production build succeeds, per-route code-split chunks present per Phase 7) and `npm run lint` (0 errors, same 3 pre-existing warnings as Phase 7, no regressions). Frontend dev server started and responded `200` at `http://localhost:5173/`.
- `docker-compose.yml` — no Docker CLI available to run `docker compose config`, so validated by: checking for tab characters (none), consistent 2-space indentation throughout, and a careful manual line-by-line review of every mapping, quoting, and variable-substitution pattern (documented reasoning for the `$$` SQL healthcheck escaping and the unquoted-URL-value cases in the file itself).

## Known limitations

- **Docker was not actually executed in this session** (see Verification above) — this is the one deliverable in this phase that still needs a hands-on run by whoever has Docker available.
- All limitations already carried forward from Phase 7 (plaintext refresh tokens, no rate-limiting beyond account lockout, `LoggingEmailSender` stub, no frontend test suite) remain unaddressed — Phase 8's scope was packaging/documentation/verification, not new hardening work. Full list in `docs/PRODUCTION_CHECKLIST.md`.
- No CI/CD pipeline definition (e.g. GitHub Actions) was created — the Dockerfiles are ready to be driven by one, but the pipeline YAML itself is out of this phase's stated deliverables.
