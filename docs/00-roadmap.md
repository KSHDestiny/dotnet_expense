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
│   ├── Auth/                   #   endpoints + request/response DTOs + handler/service
│   ├── Categories/
│   └── Expenses/
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
| 5  | **Authorization + current-user accessor** | `[Authorize]` / `RequireAuthorization`, claims, `ICurrentUser` |
| 6  | **Categories feature (CRUD, user-scoped)** | Endpoint groups, EF queries, ownership enforcement |
| 7  | **Expenses feature (CRUD) + validation** | FluentValidation, model binding, filters |
| 8  | **Custom middleware** (correlation id / request logging) | Middleware pipeline, `ILogger` scopes |
| 9  | **Global exception handling + ProblemDetails** | `IExceptionHandler`, RFC 7807 responses |
| 10 | **Summary/reporting endpoints** | LINQ aggregation, projection to DTOs |
| 11 | **Background service / scheduler** (monthly rollup) | `BackgroundService`, scoped service resolution, `PeriodicTimer` |
| 12 | **Tests** (unit + integration) | xUnit, `WebApplicationFactory`, Testcontainers |
| 13 | **Polish**: pagination, rate limiting, OpenAPI, health checks, Docker | production concerns |

---

## Decisions made

- **Database:** PostgreSQL via Docker (`docker compose up db`).
- **API style:** **Minimal APIs** with feature-grouped endpoint registration.
- **Architecture:** single Web project, folders by responsibility (Domain /
  Infrastructure / Features / Common / Middleware), vertical slices per feature.

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

---

## Where we are now

- ✅ Project scaffolded, runs (`dotnet run`), OpenAPI on.
- ✅ Docker compose with Postgres + JWT env vars ready.
- ✅ **Step 1 done** — EF Core + `User` entity + `users` table migrated & verified.
- ✅ **Step 2 done** — DI registration extensions + Options pattern (`JwtOptions`).
- 🔜 **Next: Step 3 — Auth feature (register + login) with password hashing.**

Say **"next"** when ready for Step 3.
