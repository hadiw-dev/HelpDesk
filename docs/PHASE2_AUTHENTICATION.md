# Phase 2 — Authentication

## What this phase adds

A complete, working authentication and authorization system on top of the Phase 1 foundation — no more "configuration only" JWT/Identity.

### Backend

**New entity**: `RefreshToken` (`Domain/Entities/RefreshToken.cs`) — one row per issued refresh token, with rotation metadata (`ReplacedByToken`, `RevokedAt`, `RevokedByIp`, `CreatedByIp`) and computed `IsActive`/`IsExpired`/`IsRevoked` properties. Migration: `AddRefreshTokens`.

**Application layer** (`Features/Auth/`):
- `Dtos/` — `RegisterRequest`, `LoginRequest`, `RefreshTokenRequest`, `ForgotPasswordRequest`, `ResetPasswordRequest`, `ChangePasswordRequest`, `UpdateProfileRequest`, `UserDto`, `AuthResult`
- `Interfaces/` — `IAuthService`, `ITokenService`
- `Validators/` — one `AbstractValidator<T>` per request DTO, enforcing the same password complexity ASP.NET Identity enforces server-side (8+ chars, upper/lower/digit) so client and server never disagree
- `Mappings/AuthMappingProfile.cs` — `ApplicationUser → UserDto`
- `Common/Exceptions/AppException.cs` — `NotFoundAppException` (404), `UnauthorizedAppException` (401), `ConflictAppException` (409), `ValidationAppException` (400); the Api's exception middleware now maps these to their specific status codes instead of a blanket 500
- `Common/Options/JwtOptions.cs` — moved here from the Api project so `Infrastructure`'s `TokenService` can bind it too (see `PITFALLS.md`)
- `Common/Interfaces/IActivityLogService.cs`, `IEmailSender.cs` — new cross-cutting contracts

**Infrastructure layer** (`Services/`):
- `TokenService` — issues JWTs (HMAC-SHA256) and cryptographically random refresh tokens
- `AuthService` — the real implementation of `IAuthService`, using `UserManager<ApplicationUser>` and `RoleManager<ApplicationRole>` directly (not `SignInManager` — see below)
- `ActivityLogService` — writes to the `ActivityLogs` table (Phase 1 entity, unused until now) for every register/login/logout/refresh/password-change/password-reset/profile-update event
- `LoggingEmailSender` — logs the password reset token via Serilog instead of sending a real email (no SMTP integration yet; keeps forgot/reset-password fully testable)

**Api layer**:
- `Controllers/V1/AuthController.cs` — `register`, `login`, `refresh-token`, `logout`, `forgot-password`, `reset-password`, `change-password`, `GET/PUT profile`
- `Filters/ValidationFilter.cs` — a global `IAsyncActionFilter` that resolves `IValidator<T>` for any action argument and returns a 400 `ValidationProblemDetails` before the action runs, so controllers stay free of repeated manual validation calls
- `PingController` gained `/admin` (`[Authorize(Roles = "Admin")]`) and `/management` (`[Authorize(Policy = "RequireManagerOrAdmin")]`) — small demonstration endpoints proving role-based and policy-based authorization both work, since no ticket/admin feature exists yet to hang real authorization onto
- Three authorization policies registered in `Program.cs`: `RequireAdmin`, `RequireManagerOrAdmin`, `RequireAgentOrAbove`

**Design decision — no `SignInManager`**: a quick spike (see `PITFALLS.md`) confirmed `UserManager<T>`/`RoleManager<T>` resolve fine from the `Infrastructure` class library, but `SignInManager<T>` does not (it needs ASP.NET Core's `IHttpContextAccessor`/`IAuthenticationSchemeProvider`, only available where the web framework is referenced). This turned out to be the *correct* choice anyway: `SignInManager` manages cookie-based sign-in, which a stateless JWT API has no use for. `AuthService` authenticates via `UserManager.CheckPasswordAsync` directly and lives properly in `Infrastructure`.

**Token model**: short-lived JWT access tokens (15 min) + long-lived refresh tokens (7 days) persisted server-side. Refresh tokens rotate on every use — the old one is revoked and linked (`ReplacedByToken`) to the new one, so replay of a used refresh token is rejected. `ResetPasswordAsync` revokes all of a user's active refresh tokens, forcing re-login everywhere after a password reset.

### Frontend

- **`features/auth/`** — `api.ts` (typed Axios calls to every auth endpoint), `schemas.ts` (Zod schemas mirroring the backend's FluentValidation rules), `AuthContext.tsx` (the Auth Context + provider)
- **`hooks/useAuth.ts`** — thin hook over `AuthContext`
- **`routes/ProtectedRoute.tsx`** — now a real guard: redirects to `/login` (preserving the attempted location) when not authenticated, and shows a loading state while the initial session-restore check runs
- **`api/axiosInstance.ts`** — request interceptor attaches the access token; response interceptor catches a 401, exchanges the stored refresh token for a new access token (de-duplicated via a shared in-flight promise so concurrent 401s only trigger one refresh call), retries the original request, and hard-redirects to `/login` if the refresh itself fails
- **Persistent login**: both tokens live in `localStorage`; on app mount, `AuthProvider` calls `GET /auth/profile` if a refresh token exists — success restores the session, and if the access token had expired, the interceptor's silent refresh handles it transparently
- **Pages**: `LoginPage`, `RegisterPage`, `ForgotPasswordPage` (new), `ResetPasswordPage` (new), `ProfilePage` (now a real form: update details, change password, log out) — all using React Hook Form + Zod (`@hookform/resolvers/zod`)
- `Navbar` shows the signed-in user's email and a logout button

## Verification performed

All of the following were exercised against the real API + LocalDB (not mocked), via `curl`:

| Check | Result |
|---|---|
| Register → tokens issued, default `Employee` role assigned | ✅ |
| Login (correct / wrong password / inactive account) | ✅ 200 / 401 / 401 |
| `GET /auth/profile` with/without token | ✅ 200 / 401 |
| Refresh token rotates; reusing the old one fails | ✅ new token differs; old one → 401 |
| Logout revokes the refresh token | ✅ subsequent refresh → 401 |
| Role-based authorization (`/ping/admin`) as Employee vs. Admin | ✅ 403 / 200 |
| Policy-based authorization (`/ping/management`) as Employee vs. Admin | ✅ 403 / 200 |
| Change password, then log in with the new password | ✅ |
| Forgot → reset password (token read from Serilog console output), then log in with the new password | ✅ |
| FluentValidation: weak password / mismatched confirm / invalid email all return 400 with field-level errors | ✅ |
| `ActivityLogs` table records every action | ✅ 7 distinct action types logged |
| CORS + full login round trip from the actual frontend origin (`http://localhost:5173`) | ✅ |
| Backend unit tests (`TokenServiceTests`, `AuthServiceTests`) | ✅ 13/13 passing |
| Frontend build + lint | ✅ 0 TS errors, 0 lint errors |

## Known trade-offs (see `PITFALLS.md` for full detail)

- Refresh tokens are stored as plaintext random strings in the database, not hashed. Acceptable for this stage; hashing (HMAC + pepper) is a reasonable hardening item before production.
- Both tokens live in `localStorage`, which is readable by any script on the page (XSS risk) — the standard trade-off against httpOnly cookies, which would require backend cookie issuance this phase didn't build.
- Password reset emails aren't actually sent; the token is logged via Serilog. Swap `LoggingEmailSender` for a real provider when one is chosen.
