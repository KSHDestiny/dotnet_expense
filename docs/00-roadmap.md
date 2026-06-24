# Expense App — Build Roadmap

Project-specific build plan and design decisions for *this* expense tracker API.
Built the way a senior .NET engineer would: concise, SOLID, idiomatic — patterns
applied only where they earn their place. No over-engineering.

> **Pacing:** one feature per step. Concept explained first, code in small chunks,
> then **stop** and wait for "go"/"next". You drive.

---

## What we're building

A **personal expense tracker API** (.NET 10, ASP.NET Core, Minimal APIs, PostgreSQL).
Users register/login, record expenses under categories, get summaries; background
jobs roll up monthly reports. Chosen to exercise the full production stack:
auth, middleware pipeline, background services, clean layering, validation,
error handling, tests, containerization.

---

## Domain model

```
User 1──* Expense *──1 Category
              │
        amount, note, spentAt
```

- **User** — Id, Name, Email (unique), PasswordHash, CreatedAt
- **Category** — Id, UserId, Name
- **Expense** — Id, UserId, CategoryId, Amount, Note, SpentAt, CreatedAt

---

## API design — hybrid REST + GraphQL

Two API surfaces in one app, by design:

- **REST** handles **auth**: `POST /auth/register`, `POST /auth/login` (issue JWT).
- **GraphQL** (Hot Chocolate, `/graphql`) handles **everything else**: categories,
  expenses, summaries — as queries + mutations.

Both share the same JWT bearer token: REST issues it; the client sends it in the
`Authorization` header to GraphQL too. The Step 4 auth middleware and the Step 5
`ICurrentUser` accessor are reused unchanged by GraphQL resolvers (`[Authorize]`).

```
REST    → /auth/register, /auth/login           issues JWT
GraphQL → /graphql   [Authorize]                categories, expenses, summaries
            ↑ same Bearer token validated by the existing JwtBearer middleware
```

**Why hybrid:** auth is a natural request/response fit for REST and stays simple;
the data API benefits from GraphQL's single endpoint, client-shaped queries, and
EF-pushed filtering/paging. Demonstrates integrating both paradigms cleanly.

**GraphQL stack:** Hot Chocolate, **annotation-based** (code-first) — plain C#
`Query`/`Mutation` classes with `[Authorize]`/resolver attributes; the schema is
inferred. EF Core integration for projection/filtering/paging.

## Architecture

A **pragmatic layered architecture** in a single Web project, split into folders by
responsibility. We do *not* split into 4 separate projects (Domain/Application/
Infrastructure/Api) — that's overkill for this scope. We keep clean boundaries via
folders + interfaces, and can extract projects later if it ever warrants it.

```
src/DotnetApp/
├── Program.cs                  # composition root: DI + middleware pipeline only
├── appsettings.json
├── Domain/                     # entities, enums, domain logic — no framework deps
│   └── Entities/
├── Infrastructure/
│   ├── Persistence/            # AppDbContext, EF configurations, migrations
│   └── <cross-cutting>/        # JWT token service, password hasher, etc.
├── Features/                   # vertical slices: one folder per feature
│   ├── Auth/                   #   REST endpoints + DTOs + service (register/login/JWT)
│   ├── Categories/             #   GraphQL query/mutation types + service
│   └── Expenses/               #   GraphQL query/mutation types + service
├── GraphQL/                    # root Query/Mutation, schema config, error filter
├── Common/                     # shared: results, errors, pagination, extensions
│   ├── Errors/
│   └── Extensions/             # IServiceCollection / IEndpointRouteBuilder ext methods
└── Middleware/                 # custom middleware (correlation id, exception handling)
```

### Patterns & principles in play

- **Vertical slice + Minimal API groups.** Each feature owns its endpoints, DTOs,
  and service. Endpoints registered via `MapXxxEndpoints(this IEndpointRouteBuilder)`
  extension methods, keeping `Program.cs` thin.
- **DI everywhere.** Services behind interfaces, registered with correct lifetimes
  (`DbContext` scoped, stateless services scoped/singleton as appropriate).
- **DTOs as `record`s** — never expose entities directly over the wire.
- **Result pattern** for expected failures (not-found, validation, conflict),
  exceptions only for the exceptional. Maps cleanly to `ProblemDetails` / HTTP codes.
- **EF Core code-first** with `IEntityTypeConfiguration<T>` per entity (keeps the
  `DbContext` clean; configuration colocated with intent).
- **Async all the way down** for I/O. `CancellationToken` plumbed through.
- **Options pattern** (`IOptions<JwtOptions>`) for strongly-typed config — no magic
  strings reaching into `IConfiguration`.
- **Centralized cross-cutting concerns** via middleware (exception handling,
  correlation id) rather than scattered try/catch.

---

## Step-by-step plan

| #  | Step | What it establishes |
|----|------|---------------------|
| 1  | **EF Core setup + DbContext + User entity + first migration** | Persistence layer, code-first migrations, Postgres connection, `IEntityTypeConfiguration` |
| 2  | **Folder structure + DI conventions + Options pattern** | Composition root, service registration extensions, strongly-typed config |
| 3  | **Auth: register + login** (password hashing) | Vertical slice, DTOs, service layer, Result pattern |
| 4  | **JWT issuing + authentication** | Token service, `JwtBearer`, Options pattern for secrets |
| 5  | **Authorization + current-user accessor** | `[Authorize]` / `RequireAuthorization`, claims, `ICurrentUser` (reused by GraphQL) |
| 6  | **GraphQL setup (Hot Chocolate)** + first authorized query (`me`) | `/graphql`, schema config, JWT `[Authorize]` in resolvers, Banana Cake Pop |
| 7  | **Categories — GraphQL queries + mutations** (user-scoped) | Query/Mutation types, EF integration, ownership enforcement |
| 8  | **Expenses — GraphQL queries + mutations + validation** | FluentValidation in mutations, input types, filtering/paging |
| 9  | **GraphQL error handling** + global exception handling | Error filters, `ProblemDetails` for REST, GraphQL error mapping |
| 10 | **Summary/reporting (GraphQL)** | LINQ aggregation, projection to GraphQL types |
| 11 | **Custom middleware** (correlation id / request logging) | Middleware pipeline, `ILogger` scopes (covers both REST + GraphQL) |
| 12 | **Background service / scheduler** (monthly rollup) | `BackgroundService`, scoped service resolution, `PeriodicTimer` |
| 13 | **Tests** (unit + integration) | xUnit, `WebApplicationFactory`, Testcontainers; GraphQL request tests |
| 14 | **Polish**: paging, rate limiting, health checks, Docker | production concerns |

---

## Decisions made

- **Database:** PostgreSQL via Docker (`docker compose up db`).
- **API style:** **Hybrid** — REST (Minimal APIs) for auth; **GraphQL (Hot
  Chocolate, annotation-based)** at `/graphql` for all data features. Both share
  the same JWT.
- **Architecture:** single Web project, folders by responsibility (Domain /
  Infrastructure / Features / GraphQL / Common / Middleware), vertical slices per
  feature.

## Conventions

- File-scoped namespaces, `record` DTOs, nullable reference types on, async for I/O.
- Entities never leave the service layer — map to DTOs.
- One `IEntityTypeConfiguration<T>` per entity.
- Service registration grouped into `IServiceCollection` extension methods.
- Strongly-typed config via Options pattern; secrets via env vars.

## Per-feature docs

| Doc | Status |
|-----|--------|
| `00-roadmap.md` | ✅ this file |
| `01-ef-core-setup.md` | ✅ step 1 done |
| `02-di-and-options.md` | ✅ step 2 done |
| `03-auth-register-login.md` | ✅ step 3 done |
| `04-jwt-auth.md` | ✅ step 4 done |
| `05-current-user.md` | ✅ step 5 done |
| `06-graphql-setup.md` | ✅ step 6 done |
| `07-categories-graphql.md` | ✅ step 7 done |
| `08-expenses-graphql.md` | ✅ step 8 done |

---

## Where we are now

- ✅ Project scaffolded, runs (`dotnet run`), OpenAPI on.
- ✅ Docker compose with Postgres + JWT env vars ready.
- ✅ **Step 1 done** — EF Core + `User` entity + `users` table migrated & verified.
- ✅ **Step 2 done** — DI registration extensions + Options pattern (`JwtOptions`).
- ✅ **Step 3 done** — Auth (register + login), Result pattern, PBKDF2 hashing; tested over HTTP.
- ✅ **Step 4 done** — JWT issuing + authentication; token on register/login, protected `/me`; tested.
- 🔄 **Plan updated** — API is now **hybrid**: REST auth (done) + GraphQL for data features (Step 6+).
- ✅ **Step 5 done** — `ICurrentUser` accessor; `/me` refactored; tested.
- ✅ **Step 6 done** — GraphQL (`/graphql`) live; authorized `me` query reuses the JWT + `ICurrentUser`; tested.
- ✅ **Step 7 done** — Categories GraphQL CRUD (queries + mutations), user-scoped; cross-user isolation verified.
- ✅ **Step 8 done** — Expenses GraphQL CRUD + FluentValidation; two-level ownership, RESTRICT delete; tested.
- 🔜 **Next: Step 9 — GraphQL error handling + global exception handling (clean errors, no leaks).**

Say **"next"** when ready for Step 9.
