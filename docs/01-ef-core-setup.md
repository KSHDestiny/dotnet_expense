# Step 1 — EF Core Setup (Persistence Layer)

How this project's persistence layer is wired: EF Core + PostgreSQL, code-first
migrations. This is the project-specific record; general EF notes live in
`notes/07-ef-core.md`.

> Status: ✅ complete. `users` table created and verified in Postgres.

---

## The mental model

EF Core (the .NET ORM) has three moving parts:

1. **Entity** — a plain C# class (POCO) mapped to a table. No base class needed.
2. **`DbContext`** — unit-of-work + DB session. Exposes `DbSet<T>` per table,
   tracks changes, flushes on `SaveChangesAsync()`. Registered **scoped**
   (one instance per HTTP request).
3. **Migrations** — versioned C# files describing schema changes, *generated* by
   diffing the current model against the last migration's snapshot.

## Code-first workflow (the core idiom)

The C# model is the source of truth. You never hand-write SQL/schema:

```bash
# 1. write / change the entity in C#
# 2. generate a migration (diffs model vs snapshot)
dotnet ef migrations add <Name> --project src/DotnetApp
# 3. apply pending migrations to the DB
dotnet ef database update --project src/DotnetApp
```

Migrations are generated, but **reviewed and committed** — treat them as real code.

---

## What was installed (chunk 1a)

```bash
# CLI tool that runs migration commands (global tool)
dotnet tool install --global dotnet-ef          # v10.0.9

# PostgreSQL provider (driver + EF SQL translation)
dotnet add src/DotnetApp package Npgsql.EntityFrameworkCore.PostgreSQL

# Design-time support the dotnet-ef CLI needs to scaffold migrations
dotnet add src/DotnetApp package Microsoft.EntityFrameworkCore.Design
```

**PATH note:** the global tool lives in `~/.dotnet/tools`. Add to `~/.zshrc`:

```bash
export PATH="$PATH:$HOME/.dotnet/tools"
```

**`.csproj` detail:** the SDK auto-marked the `Design` package with
`<PrivateAssets>all</PrivateAssets>` — it's design-time only and is **not** shipped
as a runtime dependency. Correct and intentional.

---

## Design decisions (this project)

### Per-entity configuration: `IEntityTypeConfiguration<T>`
Instead of cramming everything into `DbContext.OnModelCreating`, we use **one
configuration class per entity** (column types, indexes, constraints) and call
`modelBuilder.ApplyConfigurationsFromAssembly(...)`. Keeps the `DbContext` tiny and
colocates mapping with intent.

### `Guid` primary keys (not `int`)
- Non-enumerable in a public API — you can't guess `/users/2`.
- Generatable before `SaveChanges` / client-side.
- Trade-off: larger than `int`, non-sequential (index fragmentation). Configured to
  use Postgres-generated `uuid` to mitigate.

### Nullable reference types: `= null!;`
With NRT enabled, non-nullable string properties EF will populate get `= null!;` —
tells the compiler "this won't actually be null at runtime" without making the
property nullable. Avoids the CS8618 warning honestly.

### UTC timestamps
`DateTime` (UTC) maps to Postgres `timestamp with time zone` via Npgsql. Always
store UTC; convert at the edges.

---

## Target structure for this step

```
src/DotnetApp/
├── Domain/Entities/User.cs
└── Infrastructure/Persistence/
    ├── AppDbContext.cs
    ├── Configurations/UserConfiguration.cs
    └── Migrations/            # generated
```

## Remaining chunks

- [x] 1a — tooling + packages
- [x] 1b — `User` entity
- [x] 1c — `UserConfiguration : IEntityTypeConfiguration<User>`
- [x] 1d — `AppDbContext`
- [x] 1e — connection string + `AddDbContext` registration
- [x] 1f — generate / review / apply first migration, verify table

## Migration workflow used (1f)

```bash
# generate (diffs model vs snapshot); placed in our structure via --output-dir
dotnet ef migrations add InitialCreate \
  --project src/DotnetApp \
  --output-dir Infrastructure/Persistence/Migrations

# REVIEW the generated Up()/Down() before applying (senior habit)

# apply to the running Postgres container
dotnet ef database update --project src/DotnetApp
```

Generated 3 files: `<timestamp>_InitialCreate.cs` (Up/Down), `.Designer.cs`, and
`AppDbContextModelSnapshot.cs` (the model state EF diffs against next time). **Commit
all three.**

**Verify in DB:**
```bash
docker exec dotnet-app-db psql -U app_user -d app_db -c "\d users"
```

Result confirmed: snake_case columns, `uuid` PK (`pk_users`), unique `ix_users_email`,
`created_at` default `now()`, all the right types. Migration recorded in
`__EFMigrationsHistory`.

> First-run note: a benign "Failed executing DbCommand" appears while EF probes for
> the not-yet-existing `__EFMigrationsHistory` table, then creates it. Not an error.

## Registration & connection string (1e)

`Program.cs`:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"))
           .UseSnakeCaseNamingConvention());
```
- `AddDbContext` → **scoped** lifetime (one per request); never singleton.
- `UseSnakeCaseNamingConvention()` applied here at registration (where the package wants it).

`appsettings.json` holds the **localhost** connection string for `dotnet run`. In Docker,
compose injects `ConnectionStrings__Default` (env var) which **overrides** it — `__`
maps to the `:` config separator.

## Naming convention decision

Global **snake_case** via the `EFCore.NamingConventions` package
(`UseSnakeCaseNamingConvention()`, applied in `Program.cs`). Tables/columns/indexes
auto-map (`PasswordHash` → `password_hash`, `Users` → `users`). Config classes hold
**constraints only**, no naming boilerplate.

## DbContext idioms used

- **Primary constructor**: `AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)`
  — no boilerplate ctor body.
- **`public DbSet<User> Users => Set<User>();`** — expression-bodied, read-only,
  no `= null!;` needed (plays well with NRT).
- **`ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly)`** — auto-discovers
  every `IEntityTypeConfiguration<T>`; adding an entity + config needs no DbContext edit.
