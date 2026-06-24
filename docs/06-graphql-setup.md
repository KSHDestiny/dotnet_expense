# Step 6 — GraphQL Setup (Hot Chocolate)

The hybrid API's second surface: `/graphql` for all data features. Reuses the
existing JWT auth and `ICurrentUser`. First authorized query: `me`.

> Status: ✅ complete. `/graphql` live; `me` query authorized via the same JWT;
> tested with/without token + introspection.

---

## How Hot Chocolate works

One endpoint `/graphql` serves a **schema** generated from C# types:
- **`Query`** — root type for reads; each public method = a queryable field.
- **`Mutation`** — root type for writes (Step 7+).
- **Resolvers** — the methods. Hot Chocolate injects services
  (`ICurrentUser`, `AppDbContext`) into the method signature, like Minimal APIs.

Client sends exactly the shape it wants:
```graphql
query { me { id name email } }
```

## Auth carries over (the point)

GraphQL runs on the same `HttpContext`, so the same `Authorization: Bearer <jwt>`
header is validated by the existing `UseAuthentication()`. A resolver with
`[Authorize]` requires a valid token; `ICurrentUser` works unchanged inside
resolvers. **No new auth code** — just `.AddAuthorization()` on the GraphQL config
and `[Authorize]` attributes.

## Decision: per-resolver `[Authorize]`

The `/graphql` endpoint stays open (schema/introspection works); individual
resolvers carry `[Authorize]`. Idiomatic GraphQL, flexible for public fields later.

## DTOs, not entities

GraphQL types are DTOs — same rule as REST. `me` returns a `UserDto`
(id/name/email), never the `User` entity (which has `PasswordHash`). Returning the
entity would leak the hash into the schema.

## Chunks

- [x] 6a — Hot Chocolate packages (`HotChocolate.AspNetCore` + `.Authorization` v16.2.3)
- [x] 6b — `UserDto` + `Query.GetMeAsync` → `me` field, `[Authorize]`, EF projection
- [x] 6c — `AddGraphQLApi` extension (`AddGraphQLServer().AddAuthorization().AddQueryType<Query>()`)
- [x] 6d — `MapGraphQL()` in `Program.cs`; removed the temporary REST `/me`
- [x] 6e — tested `me` with/without token + introspection

## Resolver naming

`GetMeAsync` → GraphQL field `me` (HC strips `Get`/`Async`, camelCases). Services
(`AppDbContext`, `ICurrentUser`) are injected from DI directly — `[Service]` is **not
needed** in HC v16. `CancellationToken` is honored.

## Gotcha: GreenDonut `Result<T>` clash (hit + fixed)

Hot Chocolate injects implicit usings, including `global using GreenDonut;`, whose
`Result<T>` collided with our `Common.Result<T>` (CS0104 ambiguity — even in files
that import nothing). Fix in `.csproj`:

```xml
<HotChocolateImplicitUsings>disable</HotChocolateImplicitUsings>
```

(Found the exact property + `disable` value by reading
`hotchocolate.aspnetcore/16.2.3/build/HotChocolate.AspNetCore.targets`. A `<Using
Remove="GreenDonut"/>` does **not** work — HC's target re-adds it.) With HC implicit
usings off, import HotChocolate namespaces explicitly where needed; we no longer use
`[Service]` so nothing else was required.

## Verified behavior

| Query                          | Result |
|--------------------------------|--------|
| `me` with valid token          | returns the user (id/name/email) |
| `me` without token             | error `AUTH_NOT_AUTHENTICATED`, `me: null` |
| `__schema` introspection       | works without a token (endpoint open, resolver gated) |

Same JWT from REST `/auth` authenticated the GraphQL query; `ICurrentUser` worked
inside the resolver. Request collection: `http/graphql.http`. Nitro IDE: open
`/graphql/` in a browser.
