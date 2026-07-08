# Deployment Checklist

A pre-flight list for standing up this application in **any** environment (local Docker Compose, a staging server, or a first production rollout). For what additionally needs hardening before a *real* production launch specifically, see `docs/PRODUCTION_CHECKLIST.md`.

## Before you start

- [ ] `.env` created from `.env.example`, with a real `MSSQL_SA_PASSWORD` (meets SQL Server's complexity policy) and a real `JWT_SECRET_KEY` (32+ characters, generated randomly — not typed by hand)
- [ ] Ports `3000`, `5019`, `1433` (or your chosen overrides) are free on the target host
- [ ] Docker Desktop / Docker Engine + Compose plugin installed and running (`docker compose version`)

## Build & start

- [ ] `docker compose up --build` completes without errors
- [ ] `docker compose ps` shows all three services as `(healthy)`
- [ ] `docker compose logs api` shows `"No migrations were applied. The database is already up to date."` (fresh deploy: shows the migrations actually being applied instead) and no fatal errors

## Verification

- [ ] `curl http://localhost:5019/health` → `200 OK`
- [ ] `curl http://localhost:5019/swagger/v1/swagger.json` → valid JSON, `200 OK`
- [ ] Swagger UI browsable at `http://localhost:5019/swagger`
- [ ] `curl http://localhost:3000/health` → `healthy`
- [ ] Frontend loads at `http://localhost:3000` with no console errors
- [ ] Register a new account through the UI → succeeds, lands on Dashboard
- [ ] Log out, log back in → succeeds
- [ ] Create a ticket → appears in the Tickets list
- [ ] Upload an attachment to a ticket → succeeds, downloadable afterward
- [ ] `docker compose down && docker compose up` (without `-v`) → data persists (the ticket/account created above are still there)

## Data & secrets

- [ ] `.env` is **not** committed to git (`git status` shows it untracked/ignored)
- [ ] `sqlserver_data` and `helpdesk_file_storage` volumes exist (`docker volume ls`) and are the ones actually being written to
- [ ] Confirm `MSSQL_PID` (Express/Standard/Enterprise) matches your actual license entitlement if this isn't just a local demo

## Documentation handed off

- [ ] `docs/DEPLOYMENT_GUIDE.md` — how to run and troubleshoot the stack
- [ ] `docs/SETUP_GUIDE.md` — native (non-Docker) dev setup, for anyone who needs to debug outside a container
- [ ] `postman/HelpDesk-API.postman_collection.json` + environment file — importable, runnable API collection
- [ ] `docs/swagger/swagger.json` — static export of the API's OpenAPI document, for offline reference or import into other tools

## Rollback plan

- [ ] Know how to revert: `docker compose down`, check out the previous tagged commit/image, `docker compose up --build` again
- [ ] Confirm the previous release's data is still compatible (EF Core migrations in this project are additive/forward-only — rolling back code while a *newer* migration has already run against the shared database is not safe without a corresponding down-migration plan)
