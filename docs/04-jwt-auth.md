# Step 4 — JWT Issuing + Authentication

Stateless bearer-token auth. Login/register issue a signed JWT; every request with
`Authorization: Bearer <jwt>` is validated by the JwtBearer middleware.

> Status: ✅ complete. Tested end-to-end — token issued on register/login, `/me`
> protected, claims read correctly. Builds on `JwtOptions` from Step 2.

---

## How JWT auth works

A JWT is `header.payload.signature` (base64url). The payload holds **claims** (user
id, email, expiry). The HMAC signature (signed with our secret) lets the server
verify integrity **without a DB lookup** → stateless auth.

```
login → issue JWT → client stores it → sends "Authorization: Bearer <jwt>"
      → middleware validates signature + issuer + audience + expiry → runs as that user
```

Two halves:
1. **Issuing** — `IJwtTokenService` builds + signs a token from a `User`.
2. **Validating** — `AddAuthentication().AddJwtBearer(...)` middleware checks every
   incoming bearer token against `TokenValidationParameters`.

## Claims issued

| Claim | Value |
|-------|-------|
| `sub` | user id (standard subject) |
| `email` | user email |
| `jti` | unique token id (future revocation/logging) |
| `exp` | now + `JwtOptions.ExpiryMinutes` |
| `iss` / `aud` | from `JwtOptions`, validated on the way in |

## Decisions

- **Register + login both return a token** (register = auto-login).
- **Local dev secret via User Secrets** (`dotnet user-secrets`) — never committed.
  Docker uses the `Jwt__Secret` env var (already in `docker-compose.yml`).
- `JwtOptions` bound with `ValidateDataAnnotations().ValidateOnStart()` → app fails
  fast if the secret is missing / < 32 chars.

## Secret handling

```bash
# local dev — stored in ~/.microsoft/usersecrets, outside the repo
dotnet user-secrets set "Jwt:Secret" "<32+ char dev secret>" --project src/DotnetApp
```
Never put the signing secret in appsettings.json (committed). Config layering: env
vars + user-secrets override JSON; `Jwt__Secret` (`__` → `:`) supplies it in Docker.

## Chunks

- [x] 4a — JwtBearer package + dev secret via User Secrets (UserSecretsId in csproj)
- [x] 4b — `IJwtTokenService` + `JwtTokenService` (HMAC-SHA256, sub/email/jti claims)
- [x] 4c — `AddJwtAuth` (bind/validate options, JwtBearer TokenValidationParameters, ClockSkew 30s)
- [x] 4d — `Token` on `AuthResponse`; issued on register + login (shared `ToResponse`)
- [x] 4e — pipeline wiring (`UseAuthentication` → `UseAuthorization`) + protected `/me`
- [x] 4f — built + tested over HTTP

## Pipeline order (matters)

`app.UseAuthentication()` **before** `app.UseAuthorization()`. AuthN sets who the
caller is; AuthZ decides if they're allowed. `.RequireAuthorization()` on an endpoint
(or a `MapGroup`) enforces a valid token.

## Gotcha: claim remapping (hit + fixed)

By default the JWT handler remaps inbound `sub`/`email` to long legacy Microsoft
claim URIs, so `user.FindFirstValue("sub")` returned **null** even though the token
was valid (endpoint still returned 200 — auth worked, claim *reading* didn't).

Fix (one line in `AddJwtAuth`):
```csharp
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
```
Keeps claim names exactly as issued.

## Verified behavior

| Scenario                    | Result |
|-----------------------------|--------|
| Register / login            | 201 / 200 + JWT in `token` |
| `/me` with valid token      | 200 + real `id`/`email` from claims |
| `/me` without token         | 401 |
| `/me` with garbage token    | 401 |

Request collection: `http/me.http` (uses REST Client chaining —
`{{login.response.body.token}}`).

## Note: `/me` is temporary

The inline `/me` in `Program.cs` proves validation works. Step 5 replaces ad-hoc
`ClaimsPrincipal` reading with a proper `ICurrentUser` accessor.
