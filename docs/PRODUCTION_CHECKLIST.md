# Production Checklist

This project is portfolio/demo-quality: Clean Architecture, tested, containerized, and documented — but the Docker Compose stack in this repo makes several deliberate simplifications that a **real, internet-facing production deployment** should not carry forward as-is. This is the honest list of what to change, organized by risk area, cross-referenced to where each item is currently handled and why.

## Secrets & configuration

| Item | Current state | Needed for production |
|---|---|---|
| JWT signing key | `.env` file / `dotnet user-secrets` | Managed secret store (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault, or your orchestrator's native secrets) with rotation policy |
| SQL `sa` password | `.env` file, SQL Server built-in superuser | Managed secret store; replace `sa` with a least-privilege application login scoped to just this database |
| CORS allowed origins | Single hardcoded origin per environment | Confirm the real production frontend origin(s) are the *only* ones listed — never `*` with `AllowCredentials` |

## Transport security

- [ ] Terminate TLS somewhere in front of both the frontend and API (reverse proxy, load balancer, or cloud-native ingress) — nothing in this repo's containers does this itself
- [ ] Update `VITE_API_BASE_URL` (build-time) and `Cors:AllowedOrigins` to `https://` origins once TLS is in place
- [ ] Confirm HSTS (`app.UseHsts()`, active outside Development) is actually meaningful once real HTTPS is in front of the app

## Database

- [ ] Move off SQL Server Express (`MSSQL_PID: Express`, 10GB/1.4GB buffer pool caps) to a licensed edition or a managed service (Azure SQL, RDS for SQL Server) sized for real ticket volume
- [ ] Apply migrations as a discrete release-pipeline step, not `ApplyMigrationsOnStartup` — safe for this repo's single-replica demo stack, not safe for multiple replicas racing to migrate concurrently
- [ ] Set up automated backups and a tested restore procedure (neither exists in this repo — it's out of scope for an application-level Clean Architecture demo)
- [ ] Hash refresh tokens at rest (currently stored as plaintext in the `RefreshTokens` table — documented since Phase 2, never revisited)
- [ ] Don't expose SQL Server's port to the public internet (in this repo's compose file it's published to the host purely for local dev convenience)

## Application hardening

- [ ] Add rate-limiting on `/auth/login` and `/auth/register` beyond ASP.NET Identity's built-in account lockout (5 failed attempts) — a dedicated rate limiter (`Microsoft.AspNetCore.RateLimiting`) protects against distributed brute-force/credential-stuffing across many accounts, which per-account lockout alone doesn't
- [ ] Replace `LoggingEmailSender` (logs "sent" emails to console/file) with a real transactional email provider (SendGrid, SES, Postmark, etc.) for password resets and notifications
- [ ] Real-time notification delivery (SignalR/WebSockets) instead of polling, if notification latency becomes a real user complaint
- [ ] Reconsider Swagger's availability in production — this repo intentionally leaves it on in every environment (it exposes API shape, not secrets), but some organizations require it gated behind auth or disabled entirely in production; if so, reintroduce an environment check around `app.UseSwagger()`/`UseSwaggerUI()` in `Program.cs`

## Observability

- [ ] Ship Serilog output somewhere queryable (Seq, Elasticsearch, Azure Application Insights, CloudWatch) instead of local rolling files inside a container that gets destroyed on redeploy
- [ ] Add alerting on the `/health` endpoint and on the Activity Log's `LoginFailed` volume (a spike is a real signal worth paging someone for)
- [ ] Add distributed tracing / correlation IDs if this ever becomes a multi-service deployment

## Infrastructure & process

- [ ] CI/CD pipeline (build → test → build image → push to registry → deploy) — this repo currently ships Dockerfiles and a compose file, but no pipeline definition
- [ ] Image scanning (Trivy, Grype, or your registry's built-in scanner) on the built API/frontend images before deploy
- [ ] A real load balancer / orchestrator (Kubernetes, ECS, App Service) instead of a single `docker compose up` host, if uptime/scaling requirements exceed one machine
- [ ] Documented on-call/incident-response process — out of scope for this repo, but worth having before real users depend on this

## Explicitly out of scope for this project (by design, not oversight)

These are noted throughout `docs/ROADMAP.md` and `docs/PITFALLS.md` as deliberate scope boundaries for a portfolio-sized Clean Architecture demonstration, not gaps discovered late:

- Custom role creation (the 4 roles are fixed, hardcoded into every authorization policy)
- Configurable per-priority SLA policies (SLA is measured against each ticket's own `DueDate` field)
- Multi-tenancy
