# Deployment Guide

This project ships as three containers — API, frontend, and SQL Server — orchestrated by a single `docker-compose.yml` at the repository root. This guide covers running that stack, plus what changes for a real (non-demo) production deployment.

## Prerequisites

- Docker Desktop (Windows/macOS) or Docker Engine + Compose plugin (Linux), providing the `docker compose` (v2) CLI.
- Ports `3000`, `5019`, and `1433` free on the host (see "Ports" below to change these).

## 1. Configure environment

```bash
cp .env.example .env
```

Edit `.env` and set real values:

| Variable | Purpose | Constraint |
|---|---|---|
| `MSSQL_SA_PASSWORD` | SQL Server `sa` password | 8+ chars, 3 of {upper, lower, digit, symbol} — SQL Server's own complexity policy |
| `JWT_SECRET_KEY` | JWT signing key | 32+ characters (fail-fast checked at API startup) |

`.env` is gitignored — it never gets committed. `.env.example` (committed) only holds placeholders.

## 2. Build and start

```bash
docker compose up --build
```

First run builds three images (SDK-based multi-stage build for the API, Node+nginx multi-stage build for the frontend, and pulls the official SQL Server image) and starts them in dependency order:

1. **`sqlserver`** starts first; its container HEALTHCHECK runs `sqlcmd -Q "SELECT 1"` every 10s until SQL Server is actually accepting connections (not just "container running" — the SQL Server process itself takes a few seconds to initialize after the container starts).
2. **`api`** waits for `sqlserver`'s healthcheck to pass (`depends_on: condition: service_healthy`), then starts. On startup it applies any pending EF Core migrations automatically (`ApplyMigrationsOnStartup: "true"`, set only in this compose file — the native/local setup still applies migrations manually; see `docs/PITFALLS.md`), then begins serving on port 8080 internally.
3. **`frontend`** waits for `api`'s healthcheck, then starts nginx serving the pre-built static React app.

Run detached instead: `docker compose up --build -d`. Watch logs: `docker compose logs -f api` (or `sqlserver` / `frontend`).

## 3. Verify

| Check | Command | Expected |
|---|---|---|
| Frontend loads | `curl -I http://localhost:3000/` | `200 OK` |
| Frontend health | `curl http://localhost:3000/health` | `healthy` |
| API health | `curl http://localhost:5019/health` | `200 OK`, JSON with `status: "Healthy"` |
| Swagger | open `http://localhost:5019/swagger` | Swagger UI loads, lists all endpoints |
| Full flow | open `http://localhost:3000`, register an account, log in | Dashboard loads |

Or check Docker's own view of container health: `docker compose ps` — all three should show `(healthy)`.

## 4. Stopping / resetting

```bash
docker compose down            # stop and remove containers, keep data volumes
docker compose down -v         # also delete sqlserver_data and helpdesk_file_storage volumes — full reset
```

## Architecture of the compose stack

```
Browser
  │
  ├──▶ http://localhost:3000  (frontend container: nginx serving the built React SPA)
  │        │
  │        └── JS bundle calls VITE_API_BASE_URL, baked in at build time
  │
  └──▶ http://localhost:5019/api/v1/...  (api container: Kestrel on internal port 8080)
           │
           └──▶ sqlserver container, internal Docker network, port 1433
                (not reachable from the browser directly — only from the api container)
```

The frontend container and the browser's own requests to the API are **not** on the same path: the browser talks to both `localhost:3000` (frontend) and `localhost:5019` (API) directly, because `VITE_API_BASE_URL` is baked into the JS bundle at build time and must be a browser-reachable address — not the Docker-internal service name `api`, which only resolves *inside* the Docker network between containers.

## Ports

| Service | Container port | Host port | Change by |
|---|---|---|---|
| frontend (nginx) | 80 | 3000 | edit `ports:` in `docker-compose.yml` |
| api (Kestrel) | 8080 | 5019 | edit `ports:` in `docker-compose.yml` **and** the frontend build's `VITE_API_BASE_URL` arg **and** `Cors__AllowedOrigins__0` if the frontend port also changes |
| sqlserver | 1433 | 1433 | edit `ports:` in `docker-compose.yml` (safe to remove entirely if you don't need host-level DB access) |

## What's different from a real production deployment

This compose stack is deliberately shaped to demonstrate the full system running together and to be easy to stand up in one command — it is **not** a hardened production deployment as-is. See `docs/PRODUCTION_CHECKLIST.md` for the full list; the highlights:

- **Secrets**: `.env` file secrets are fine for a local demo; a real deployment should pull `MSSQL_SA_PASSWORD` / `Jwt:SecretKey` from a managed secret store (Azure Key Vault, AWS Secrets Manager, Docker/Kubernetes secrets) instead.
- **TLS**: nothing in this stack terminates HTTPS. A real deployment puts a reverse proxy or cloud load balancer in front of both the frontend and API with a real certificate, and updates `Cors:AllowedOrigins` / `VITE_API_BASE_URL` to `https://` origins.
- **SQL Server edition**: `MSSQL_PID: Express` caps the database at 10GB and limited memory/CPU use — fine for this project's scope, not for a real production ticket volume. Swap to `Standard`/`Enterprise` (licensed) or a managed SQL Server/Azure SQL instance.
- **Migrations on startup**: safe for this single-replica compose stack; a multi-replica/orchestrated (Kubernetes, ECS) production deployment should run migrations as a separate release-pipeline step instead of `ApplyMigrationsOnStartup`, to avoid multiple instances racing to migrate simultaneously.
- **`sa` account**: SQL Server's built-in `sa` superuser is used here for simplicity. A real deployment should create a least-privilege application login instead.
- **Exposed SQL Server port**: `1433` is published to the host for local convenience (connecting via SSMS/Azure Data Studio to inspect data). A real deployment should not expose the database port beyond the private network the API runs in.

## Troubleshooting

- **`sqlserver` never becomes healthy**: check `docker compose logs sqlserver` — the most common cause is `MSSQL_SA_PASSWORD` not meeting SQL Server's complexity policy (SQL Server itself will refuse to start and the container will restart-loop).
- **`api` exits immediately after starting**: check `docker compose logs api` for the fail-fast `Jwt:SecretKey must be configured...` error — means `JWT_SECRET_KEY` in `.env` is missing or under 32 characters.
- **Frontend loads but API calls fail with a CORS error in the browser console**: `Cors__AllowedOrigins__0` in `docker-compose.yml` must exactly match the origin the browser actually loaded the frontend from (scheme + host + port) — if you changed the frontend's host port, update this too.
- **Frontend loads but shows "Network Error" on login**: `VITE_API_BASE_URL` was baked into the frontend image at *build* time — changing the `api` port afterward requires `docker compose build frontend` again (or `docker compose up --build`), not just a restart.
