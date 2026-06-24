# Step 5 — Authorization + `ICurrentUser` Accessor

A small injectable service exposing the authenticated user's identity, read from the
validated JWT claims. Reused by both REST and (Step 6+) GraphQL resolvers.

> Status: ✅ complete. `/me` refactored to inject `ICurrentUser`; tested (id matches
> registration; 401 without token).

---

## Why

The temporary `/me` reads `ClaimsPrincipal` and parses the `sub` claim inline. Doing
that in every data resolver scatters claim-string knowledge, repeats `Guid.Parse`,
and is hard to test. An `ICurrentUser` abstraction centralizes it:

```csharp
public interface ICurrentUser
{
    Guid Id { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}
```

Handlers inject `ICurrentUser` and read `.Id` — no claim strings, no parsing.

## How it reads claims

`IHttpContextAccessor` exposes the current request's `HttpContext` (and
`HttpContext.User`, the validated `ClaimsPrincipal`) to any service. Registered via
`AddHttpContextAccessor()`. Works identically for REST and GraphQL — both flow
through the same pipeline + JWT middleware.

## Decision: unauthenticated behavior

`IsAuthenticated` is `false`; reading `.Id` / `.Email` **throws**
`InvalidOperationException`. All data resolvers are `[Authorize]`-gated, so reading
the id outside an authenticated context is a bug — fail loudly, don't return a
sentinel.

## Reused by GraphQL (Step 6+)

A resolver like `myCategories` injects `ICurrentUser` and does
`db.Categories.Where(c => c.UserId == currentUser.Id)`. No GraphQL-specific auth
code. That's why we build it now.

## Chunks

- [x] 5a — `ICurrentUser` + `CurrentUser` in `Common/` (reads `IHttpContextAccessor`)
- [x] 5b — `AddHttpContextAccessor()` + scoped `ICurrentUser` (in `AddJwtAuth`)
- [x] 5c — `/me` refactored to inject `ICurrentUser` (dropped inline claim parsing)
- [x] 5d — built + tested

## Registration (lifetimes matter)

```csharp
services.AddHttpContextAccessor();           // lets services reach HttpContext.User
services.AddScoped<ICurrentUser, CurrentUser>(); // per-request — user differs per request
```

`ICurrentUser` is **scoped** (one per request). Registered inside `AddJwtAuth` since
it's auth-related and lives in `Common/` (consumed by every feature, not just Auth).

## Depends on Step 4's claim mapping

`CurrentUser.Id` reads the `sub` claim by its short name — this works because Step 4
set `JwtSecurityTokenHandler.DefaultMapInboundClaims = false`. Without that, `sub`
would be remapped and not found.

## Final `/me`

```csharp
app.MapGet("/me", (ICurrentUser currentUser) =>
    Results.Ok(new { id = currentUser.Id, email = currentUser.Email }))
   .RequireAuthorization();
```
