# Step 3 — Auth Feature (Register + Login) + Result Pattern

First full vertical slice: `Features/Auth/`. Request → Endpoint → AuthService →
DbContext. Introduces the **Result pattern** and password hashing.

> Status: ✅ complete. Tested end-to-end over HTTP. Login returns user info only;
> JWT token added in Step 4.

---

## Vertical slice flow

```
HTTP → Endpoint → (inline validate) → AuthService → DbContext → DB
          ↑                                  ↓
     Result → HTTP status            returns Result<T>
```

Everything for the feature lives under `Features/Auth/`: DTOs, service, endpoints.
Entities never cross the wire — map to DTOs.

## Result pattern (not exceptions for expected failures)

"Email already taken" / "bad credentials" are **expected** outcomes, not exceptions.
Throwing for control flow is slow and muddy. So the service returns a `Result`:

- `Result<T>` carries `IsSuccess`, `Value`, and an `Error`.
- `Error` has `Code`, `Message`, and an **`ErrorType` enum**
  (Validation / NotFound / Conflict / Unauthorized / ...).
- The endpoint pattern-matches `ErrorType` → HTTP status (409 / 401 / 404 / 400).

Genuine exceptions (DB down) stay exceptions — caught by global middleware (Step 9).

## Password hashing

`PasswordHasher<TUser>` from `Microsoft.AspNetCore.Identity` (in-framework, no NuGet):
salted **PBKDF2** with iteration count baked in.
- `HashPassword(user, plain)` → hash string (stored in `password_hash`)
- `VerifyHashedPassword(user, hash, plain)` → Success / Failed / SuccessRehashNeeded

We hide it behind an `IPasswordHasher` abstraction so the service doesn't depend on
Identity types directly (and it's swappable / testable).

## Decisions

- **Result + Error with `ErrorType` enum** — clean HTTP mapping, reused everywhere.
- **Inline minimal validation** this step (email present, password length).
  FluentValidation introduced properly in Step 7.
- **Login returns user info only** for now; `AuthResponse` shaped so adding the JWT
  in Step 4 is a one-line change.

## Chunks

- [x] 3a — `Result` / `Error` / `ErrorType` in `Common/`
- [x] 3b — Auth DTOs (records) — `AuthResponse` omits `PasswordHash`
- [x] 3c — `IPasswordHasher` abstraction over PBKDF2 `PasswordHasher<User>`
- [x] 3d — `AuthService` (register + login → Result; email normalized; no account enumeration)
- [x] 3e — `AuthEndpoints` (`MapAuthEndpoints`/`AddAuth`) + `ResultExtensions.ToHttpResult`
- [x] 3f — wired into `Program.cs`, built, tested over HTTP

## DI lifetimes (got these right)

- `IAuthService` → **scoped** (depends on scoped `AppDbContext`).
- `IPasswordHasher` → **singleton** (stateless). Injecting scoped into singleton
  would be a captive-dependency bug — avoided.

## ErrorType → HTTP mapping (`ResultExtensions.ToHttpResult`)

Centralized once, reused by every feature. Returns RFC 7807 `ProblemDetails`:

| ErrorType    | Status |
|--------------|--------|
| Validation   | 400    |
| Unauthorized | 401    |
| Forbidden    | 403    |
| NotFound     | 404    |
| Conflict     | 409    |

## Verified behavior (manual HTTP test)

| Scenario              | Result |
|-----------------------|--------|
| Register              | 201 + Location + body **without** passwordHash |
| Duplicate email       | 409 |
| Login (correct)       | 200 |
| Login (wrong pass)    | 401 |
| Login (unknown email) | 401 — identical to wrong-pass (no account enumeration) |

DB check: `password_hash` stored as an 84-char PBKDF2 hash (not plaintext);
`created_at` populated by `now()` default.

> Ready-to-run requests for these scenarios: `http/auth.http` (VS Code REST Client).
> All `.http` collections live in the top-level `http/` folder.

## Gotcha: "address already in use"

Port 8000 was held by a stale `DotnetApp` instance from a prior `dotnet run`/`watch`.
Tested our build on a free port via `ASPNETCORE_URLS=http://127.0.0.1:8099`
(overrides `launchSettings`) rather than killing an unknown process. To free 8000:
`lsof -nP -iTCP:8000 -sTCP:LISTEN` then stop that PID.
