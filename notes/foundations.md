# Foundations: The .NET Knowledge You Need (Detailed)

A deep reference for a Laravel/PHP developer learning .NET. Unlike the topic
notes (01–11), this is the **landscape in detail** — every topic gets the full
treatment:

- **What it is**
- **Laravel analogy**
- **Key differences / gotchas**
- **Code**
- **When to use / when to skip**

Levels: **[Basic]** = need it to be productive · **[Intermediate]** = expected in
code review · **[Advanced]** = know it exists, reach for it only when justified.

> The senior signal in .NET isn't using every pattern — it's knowing **when not
> to**. The "when to skip" notes are as important as the patterns.

---

# Part 1 — Tooling & Project Config

## `global.json` — pin the SDK version  **[Basic]**

**What it is.** A file at the repo root that pins which .NET **SDK** version builds
the project. Without it, whoever has the newest SDK installed builds with *that*,
which can change behavior subtly. `rollForward` controls how flexible the match is.

**Laravel analogy.** The `"require": { "php": "^8.2" }` constraint in
`composer.json` — but `global.json` governs the **build toolchain (SDK)**, not the
language runtime. There's no separate "compile step" in PHP, so this concept is
new: you're pinning the *compiler/tooling*, not just the runtime.

**Key differences / gotchas.**
- It pins the **SDK** (build tools), not the **runtime** (`<TargetFramework>` in
  the `.csproj` does that). Two different version knobs.
- `rollForward: latestMinor` = "use 10.0.x, newest patch is fine" — usually what
  you want. `disable` = exact match (strict, brittle).
- If the pinned SDK isn't installed, the build fails fast with a clear message —
  that's the point (no silent drift).

**Code.**
```json
// global.json (repo root)
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestMinor"
  }
}
```

**When to use / skip.** Use on any repo with more than one developer or a CI
pipeline — it's cheap insurance against "works on my machine." Skip for a throwaway
scratch project.

---

## `Directory.Build.props` — shared build settings  **[Intermediate]**

**What it is.** A file MSBuild automatically imports into **every** `.csproj` at or
below its folder. Put settings you'd otherwise repeat per project here:
`Nullable`, `LangVersion`, `TreatWarningsAsErrors`, implicit usings, analyzer
packages, common metadata.

**Laravel analogy.** No clean equivalent — PHP has one `composer.json` per package
and no per-project build config. Closest mental model: a base config every
sub-package inherits. It's "DRY for project files."

**Key differences / gotchas.**
- MSBuild walks **up** the directory tree and imports the nearest
  `Directory.Build.props` — so one at the repo root covers all projects.
- A child project can still override a property locally (last write wins).
- There's also `Directory.Build.targets` (imported *after* the project) for build
  steps rather than properties — rarer.

**Code.**
```xml
<!-- Directory.Build.props (repo root) -->
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

**When to use / skip.** Use the moment you have **≥2 projects** (e.g. app + test
project) so settings can't drift apart. For a single project it's optional — the
`.csproj` already holds everything.

---

## Central Package Management — `Directory.Packages.props`  **[Intermediate]**

**What it is.** Move all NuGet **version numbers** to one central file; individual
`.csproj` files then reference packages by **name only**. Guarantees every project
uses the same version of a given package.

**Laravel analogy.** A single root `composer.json` controlling versions for a
monorepo of packages — you don't restate versions in each sub-package.

**Key differences / gotchas.**
- Enabled via `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`.
- `.csproj` uses `<PackageReference Include="X" />` (no `Version=`); the version
  lives in `<PackageVersion Include="X" Version="..." />` centrally.
- Prevents the classic bug where two projects pull different versions of the same
  transitive dependency.

**Code.**
```xml
<!-- Directory.Packages.props -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Serilog.AspNetCore" Version="10.0.0" />
  </ItemGroup>
</Project>

<!-- a .csproj just says: -->
<PackageReference Include="Serilog.AspNetCore" />
```

**When to use / skip.** Use in **multi-project** solutions. Skip for a single
project — the overhead isn't worth it.

---

## `.editorconfig` + Roslyn analyzers + `dotnet format`  **[Basic]**

**What it is.** `.editorconfig` defines code style and which rules are
warnings/errors; **Roslyn analyzers** (built into the compiler) flag violations at
build time; `dotnet format` auto-applies fixes.

**Laravel analogy.** Pint / PHP-CS-Fixer (formatting) **plus** PHPStan / Larastan
(static analysis) — except in .NET both are **built into the compiler/SDK**, no
extra packages required.

**Key differences / gotchas.**
- Severity matters: `suggestion` (IDE hint), `warning`, `error` (fails the build).
  Set style rules you care about to `warning` or `error` to actually enforce them.
- Analyzers run during `dotnet build`, so CI catches violations automatically.
- You can add extra analyzer packages; `Microsoft.CodeAnalysis.NetAnalyzers` is on
  by default, others add security/async rules.

**Code.**
```ini
# .editorconfig
[*.cs]
csharp_style_namespace_declarations = file_scoped:error   # fail build if violated
dotnet_diagnostic.CA2007.severity = none                  # tune individual rules
```
```bash
dotnet format          # auto-fix style across the solution
```

**When to use / skip.** Always. It's free quality and makes diffs consistent.

---

## Configuration & secrets — layering, user-secrets, env vars  **[Basic]**

**What it is.** .NET builds configuration from multiple **layered** sources; later
sources override earlier ones. Local dev secrets go in **user-secrets** (stored
outside the repo); production uses **environment variables**.

**Laravel analogy.** `.env` (gitignored) + `config/*.php` defaults — but .NET makes
the **precedence explicit** and supports strongly-typed binding.

**Key differences / gotchas.**
- Precedence (last wins):
  `appsettings.json` → `appsettings.{Environment}.json` → user-secrets (Dev only)
  → environment variables → command-line args.
- Nested keys via env vars use a **double underscore**: `Jwt__Secret` maps to
  `Jwt:Secret`. (Colons aren't valid in env-var names on all OSes.)
- user-secrets are keyed to a `UserSecretsId` GUID in the `.csproj` and live in
  your home directory — never committed.
- Bind a whole section to a typed class with the **Options pattern** (see Part 2).

**Code.**
```bash
dotnet user-secrets set "Jwt:Secret" "dev-only-32+char-value"   # local, off-repo
# In Docker/prod instead:
#   ConnectionStrings__Default=Host=db;...   (env var, highest priority)
```
```csharp
var secret = builder.Configuration["Jwt:Secret"];               // read a value
var cs = builder.Configuration.GetConnectionString("Default");  // connection string
```

**When to use / skip.** Always use this layering. Never hard-code secrets or put
them in committed `appsettings.json`.

---

## CLI workflow — `dotnet watch`, `dotnet test`, local tool manifest  **[Basic]**

**What it is.** `dotnet watch` = hot-reload dev loop. `dotnet test` runs the test
projects. A **local tool manifest** (`.config/dotnet-tools.json`) pins CLI tools
(like `dotnet-ef`) **per repo** instead of installing them globally on your machine.

**Laravel analogy.** `php artisan serve` + a `--watch`; `dotnet test` ≈
`php artisan test`. The tool manifest is like committing a specific version of a
Composer-installed CLI so every clone gets the same tool.

**Key differences / gotchas.**
- Global tools (`dotnet tool install -g`) aren't reproducible across machines; the
  **manifest** is committed, so `dotnet tool restore` gives everyone the same
  versions. Prefer the manifest for anything the build/migrations depend on.
- `dotnet watch` does true hot reload for many edits (no full restart).

**Code.**
```bash
dotnet new tool-manifest                     # creates .config/dotnet-tools.json
dotnet tool install dotnet-ef                # pinned locally, committed
dotnet tool restore                          # on a fresh clone
dotnet ef migrations add Init                # runs the pinned tool
```

**When to use / skip.** Use a tool manifest whenever the project depends on a CLI
tool (EF migrations being the common case). Global install is fine for purely
personal, cross-project tools.

---

# Part 2 — Design Patterns in .NET

## Dependency Injection + service lifetimes  **[Basic]**

**What it is.** A built-in IoC container. You register services in `Program.cs`,
then declare dependencies as **constructor parameters**; the container creates and
injects them. Three lifetimes: **Singleton** (one instance forever), **Scoped**
(one per HTTP request), **Transient** (a new one every time it's requested).

**Laravel analogy.** The service container + `app()->bind()/singleton()`. But .NET
is **constructor-injection first** and has **no facades** — you don't call a global
`app()`; dependencies arrive through the constructor.

**Key differences / gotchas.**
- The **#1 DI bug**: injecting a **Scoped** service (like `DbContext`) into a
  **Singleton**. The singleton captures one scope forever → stale/disposed context,
  thread-safety problems. Fix: inject `IServiceScopeFactory` and create a scope per
  unit of work (this is exactly the background-service gotcha).
- `DbContext` is **Scoped** by default and is **not thread-safe** — don't share one
  across parallel tasks.
- Minimal APIs/Controllers resolve constructor (or parameter) dependencies by type
  automatically — no attribute needed in the common case.

**Code.**
```csharp
// Register (Program.cs)
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(cs)); // Scoped

// Consume (constructor injection)
public class OrderService(AppDbContext db, IClock clock) : IOrderService
{
    public Task<int> CountAsync() => db.Orders.CountAsync();
}
```

**When to use / skip.** Always — DI is the backbone of ASP.NET Core. The only
"skip" is over-injecting: don't create an interface for something with one
implementation that will never be swapped or mocked.

---

## Options pattern — typed configuration  **[Intermediate]**

**What it is.** Bind a configuration section to a strongly-typed class and inject it
via `IOptions<T>` (or `IOptionsSnapshot<T>` for per-request reload).

**Laravel analogy.** `config('jwt.secret')` — but instead of stringly-typed lookups
you get a typed object with compile-time safety and validation.

**Key differences / gotchas.**
- `IOptions<T>` is a **singleton** snapshot (read once). `IOptionsSnapshot<T>` is
  **scoped** and re-reads per request (use when config can change at runtime).
- You can attach validation: `.ValidateDataAnnotations().ValidateOnStart()` fails
  fast at startup if config is invalid — much better than a null at request time.

**Code.**
```csharp
public class JwtOptions { public string Secret { get; set; } = ""; public int ExpiryMinutes { get; set; } }

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection("Jwt"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

public class TokenService(IOptions<JwtOptions> options)   // inject typed config
{
    private readonly JwtOptions _opts = options.Value;
}
```

**When to use / skip.** Use for any non-trivial config group (JWT, mail, feature
flags). For a single value read once, `Configuration["Key"]` is fine.

---

## Minimal APIs vs Controllers  **[Basic]**

**What it is.** Two ways to define HTTP endpoints. **Minimal APIs** map routes to
delegates directly in `Program.cs` (terse). **Controllers** are classes with action
methods, model binding conventions, filters, and attribute routing (more structure).

**Laravel analogy.** Route closures (`Route::get('/x', fn() => ...)`) vs controller
classes (`Route::get('/x', [C::class, 'index'])`). Same trade-off.

**Key differences / gotchas.**
- Minimal APIs scale fine with `MapGroup` for organizing routes; you don't *need*
  Controllers for a medium API.
- Controllers give you **action filters** (cross-cutting per-endpoint logic),
  conventional model binding/validation, and `[ApiController]` niceties.
- Mixing both in one app is allowed.

**Code.**
```csharp
// Minimal API with a group
var orders = app.MapGroup("/orders").RequireAuthorization();
orders.MapGet("/", (IOrderService s) => s.ListAsync());
orders.MapPost("/", (CreateOrderRequest dto, IOrderService s) => s.CreateAsync(dto));
```

**When to use / skip.** Default to **Minimal APIs** for small/medium APIs. Reach for
**Controllers** on large surfaces with heavy filter/convention needs. Don't
cargo-cult Controllers just because tutorials use them.

---

## Service layer  **[Basic]**

**What it is.** Put business logic in injectable service classes. Endpoints stay
thin: validate input → call a service → shape the response.

**Laravel analogy.** Action classes / service classes resolved from the container.
Identical idea; constructor-injected.

**Key differences / gotchas.**
- Keep services focused (one responsibility). They depend on the `DbContext` and
  other services, not on HTTP types (`HttpContext`) — that keeps them testable.
- Return domain results/DTOs, not `IResult`/HTTP objects, from services.

**Code.**
```csharp
public class BudgetService(AppDbContext db)
{
    public async Task<Budget> CreateAsync(int userId, CreateBudgetRequest r)
    {
        var budget = new Budget { UserId = userId, Limit = r.Limit, /* ... */ };
        db.Budgets.Add(budget);
        await db.SaveChangesAsync();
        return budget;
    }
}
```

**When to use / skip.** Use once an endpoint has more than trivial logic. For a
one-line passthrough, calling the `DbContext` directly in the endpoint is fine.

---

## Repository pattern — usually **don't**  **[Intermediate]**

**What it is.** Wrapping data access behind an `IRepository<T>` interface
(`Add/Get/Remove/Find`) to abstract the persistence layer.

**Laravel analogy.** The same "wrap Eloquent in a repository" debate exists in the
Laravel world — and the same answer applies.

**Key differences / gotchas.**
- **EF Core's `DbContext` already IS a Unit of Work, and `DbSet<T>` already IS a
  repository.** Adding your own usually just re-implements them worse and hides
  LINQ's power (you end up adding `GetByXAndY` methods endlessly).
- It can also break query composition (you lose `IQueryable` chaining).

**Code.**
```csharp
// Often-redundant wrapper:
public interface IOrderRepository { Task<Order?> GetAsync(int id); }
// vs just using the context, which is already this:
var e = await db.Orders.FindAsync(id);
```

**When to use / skip.** **Skip by default.** Justified only when you must support
multiple/swappable data stores, or enforce a strict architectural boundary
(e.g. Clean Architecture) where the domain can't reference EF. For typical CRUD,
use the `DbContext` directly (optionally behind a thin service).

---

## Result pattern — return failures instead of throwing  **[Intermediate]**

**What it is.** For **expected** failures (validation, "not found", business rule
violations), return a `Result`/`Result<T>` object describing success or failure,
rather than throwing an exception. Exceptions stay for **exceptional** conditions.

**Laravel analogy.** Returning a value object or a validation result rather than
throwing — keeps normal control flow out of try/catch.

**Key differences / gotchas.**
- Exceptions in .NET are relatively **expensive** and meant for the unexpected;
  using them for routine "user typed a bad value" is an anti-pattern.
- A Result type makes the failure path explicit in the method signature (the caller
  *must* handle it), improving correctness.
- You can map a `Result` to HTTP (`Ok` / `BadRequest` / `NotFound`) at the edge.

**Code.**
```csharp
public readonly record struct Result<T>(bool Ok, T? Value, string? Error)
{
    public static Result<T> Success(T v) => new(true, v, null);
    public static Result<T> Fail(string e) => new(false, default, e);
}

var r = await service.CreateAsync(dto);
return r.Ok ? Results.Ok(r.Value) : Results.BadRequest(r.Error);
```

**When to use / skip.** Use for predictable domain failures. Don't wrap *everything*
in Result — genuinely exceptional cases (DB down, bug) should still throw and be
caught by the global handler.

---

## Background / hosted services  **[Intermediate]**

**What it is.** Long-running in-process work via `IHostedService` /
`BackgroundService` — scheduled jobs, queue consumers, cleanup loops. Starts with
the app.

**Laravel analogy.** The task scheduler (`Kernel::schedule`) and queue workers —
but it runs **inside** the app process, not via an external `cron` line.

**Key differences / gotchas.**
- A `BackgroundService` is a **singleton** → use `IServiceScopeFactory` to get a
  scoped `DbContext` per tick (the DI lifetime gotcha again).
- It only runs while the app is up; for **guaranteed/distributed** jobs use
  **Hangfire** or **Quartz.NET** (persistent storage, dashboards, retries).
- Respect the `CancellationToken` for clean shutdown.

**Code.**
```csharp
public class CleanupService(IServiceScopeFactory scopes) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(1));
        while (await timer.WaitForNextTickAsync(ct))
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // ... do work, await db.SaveChangesAsync(ct) ...
        }
    }
}
builder.Services.AddHostedService<CleanupService>();
```

**When to use / skip.** Use `BackgroundService` for simple in-process recurring
work. Move to Hangfire/Quartz when you need durability, retries, or multiple
instances coordinating.

---

## Entity ↔ DTO mapping: manual vs AutoMapper  **[Intermediate]**

**What it is.** Converting between database entities and API DTOs. Either map by
hand (or via a LINQ `Select` projection) or use **AutoMapper** to do it by
convention.

**Laravel analogy.** API Resources (`JsonResource`) shape models into responses;
AutoMapper is a more automatic, reflection-based version.

**Key differences / gotchas.**
- **Manual / projection** is explicit, fast, and lets EF translate the projection
  into SQL (selecting only needed columns). Current community sentiment leans
  **manual** for clarity.
- **AutoMapper** removes boilerplate but hides mapping bugs, complicates debugging,
  and can defeat SQL projection if misused.

**Code.**
```csharp
// Manual projection — also makes EF select only these columns:
var list = await db.Orders
    .Where(e => e.UserId == userId)
    .Select(e => new OrderResponse(e.Id, e.Amount, e.Description, e.SpentOn, e.Category.Name))
    .ToListAsync();
```

**When to use / skip.** Prefer **manual/projection** by default. Consider AutoMapper
only when you have many large, repetitive, stable maps and the boilerplate is real
pain.

---

## CQRS + MediatR  **[Advanced]**

**What it is.** **CQRS** = separate the model for **reads** (queries) from **writes**
(commands). **MediatR** is a library that dispatches a command/query object to its
handler, decoupling the endpoint from the logic and enabling pipeline behaviors
(validation, logging, transactions) around every request.

**Laravel analogy.** A command bus / dispatcher. The "behaviors" are like global
middleware, but around application messages instead of HTTP requests.

**Key differences / gotchas.**
- It adds **indirection**: one feature spreads across a command, a handler, maybe a
  validator and behaviors. Great for big domains, overkill for CRUD.
- CQRS doesn't require MediatR (you can split read/write services without it), and
  MediatR doesn't require CQRS.

**Code.**
```csharp
public record CreateOrder(int UserId, decimal Amount) : IRequest<int>;

public class CreateOrderHandler(AppDbContext db) : IRequestHandler<CreateOrder, int>
{
    public async Task<int> Handle(CreateOrder cmd, CancellationToken ct)
    {
        var e = new Order { UserId = cmd.UserId, Amount = cmd.Amount };
        db.Orders.Add(e); await db.SaveChangesAsync(ct);
        return e.Id;
    }
}
// endpoint: await mediator.Send(new CreateOrder(userId, dto.Amount));
```

**When to use / skip.** **Skip on small/medium apps** — it's the most over-applied
.NET pattern. Justified for large domains, many cross-cutting behaviors, or team
conventions that already use it. Adding it to a CRUD API to look sophisticated is a
classic over-engineering tell.

---

# Part 3 — Language & Runtime Idioms (what a PHP dev won't expect)

## `record` types — immutable DTOs with value equality  **[Basic]**

**What it is.** A `record` is a reference type the compiler enriches with: a
primary constructor, init-only properties, **value-based equality** (two records
with equal fields are `==`), a readable `ToString()`, and non-destructive copies
via `with`.

**Laravel analogy.** A readonly value object / DTO, but you'd hand-write all that
boilerplate in PHP. `record Foo(string Bar)` generates it for you.

**Key differences / gotchas.**
- Default class equality is **by reference**; record equality is **by value** — big
  difference when comparing or using them as dictionary keys.
- `record struct` is a value-type version (stack-friendly, copy semantics).
- Use records for DTOs/messages, not usually for EF entities (entities are mutable,
  tracked, and identity-based).

**Code.**
```csharp
public record OrderResponse(int Id, decimal Amount, string Category);

var a = new OrderResponse(1, 10m, "Food");
var b = a with { Amount = 20m };          // copy with one field changed
bool same = a == new OrderResponse(1, 10m, "Food");  // true — value equality
```

**When to use / skip.** Use for DTOs, API contracts, and immutable data. Skip for
entities and for types with lots of mutable behavior.

---

## Properties, `init`, and auto-properties  **[Basic]**

**What it is.** C# **properties** look like fields but are get/set accessors.
`{ get; set; }` is a mutable auto-property; `{ get; init; }` can only be set during
construction (immutable after); `{ get; }` is read-only.

**Laravel analogy.** PHP 8 has property hooks now, but historically you'd write
getter/setter methods. C# properties are first-class and ubiquitous.

**Key differences / gotchas.**
- Required + immutable: `public required string Email { get; init; }` forces the
  caller to set it at construction, then locks it.
- Expression-bodied computed property: `public int Total => Items.Sum(i => i.Qty);`

**Code.**
```csharp
public class User
{
    public required string Email { get; init; }     // must set, then immutable
    public string Role { get; set; } = "Member";     // mutable, with default
    public bool IsAdmin => Role == "Admin";          // computed, read-only
}
```

**When to use / skip.** Always — this is how C# exposes state. Prefer `init`/`required`
for data you don't want mutated after creation.

---

## Nullable reference types (NRT)  **[Basic]**

**What it is.** With `<Nullable>enable</Nullable>`, the compiler tracks whether a
reference can be null. `string` = shouldn't be null; `string?` = may be null. You
get **warnings** when you might dereference null or assign null to a non-nullable.

**Laravel analogy.** Like PHPStan/Psalm-enforced nullability — but **built into the
compiler** and on by default in new projects.

**Key differences / gotchas.**
- The **null-forgiving operator** `x!` tells the compiler "trust me, not null." It
  **silences the check without changing runtime** — overusing it defeats the whole
  feature. Use only when you can prove non-null (and ideally comment why).
- `?.` (null-conditional) and `??` (null-coalescing) are your everyday tools.
- NRT is **compile-time only** — it doesn't add runtime null checks; a value coming
  from outside (JSON, DB) can still be null at runtime.

**Code.**
```csharp
string? maybe = GetName();           // could be null
int len = maybe?.Length ?? 0;        // safe: 0 if null
string name = maybe ?? "anonymous";  // default if null
// string forced = maybe!;           // AVOID unless provably non-null
```

**When to use / skip.** Keep it enabled always. Don't reach for `!` to make
warnings disappear — fix the actual nullability instead.

---

## Pattern matching  **[Intermediate]**

**What it is.** Concise branching on **shape and value**: `switch` expressions,
type patterns (`is`), property patterns, relational/logical patterns, and list
patterns. Replaces verbose `if/else` and type-check ladders.

**Laravel analogy.** PHP's `match()` is similar for values; C# goes much further
(types, nested properties, ranges).

**Key differences / gotchas.**
- `is { } x` means "not null, and bind to `x`" — a clean null-check + capture.
- `switch` **expressions** return a value (vs the older `switch` statement).
- The compiler can warn about non-exhaustive matches.

**Code.**
```csharp
// value + relational + logical
string band = age switch
{
    < 13 => "child",
    >= 13 and < 20 => "teen",
    _ => "adult",
};

// type + property pattern with capture
if (user is { Role: "Admin", IsActive: true } admin)
    Promote(admin);
```

**When to use / skip.** Use freely for branching and dispatch. For a single simple
condition, a plain `if` is clearer — don't force a `switch`.

---

## LINQ — and the critical "where does it run?"  **[Basic]**

**What it is.** Language-Integrated Query: `Where`, `Select`, `OrderBy`, `GroupBy`,
`Sum`, `Any`, `First`, etc., over any sequence. Two flavors: **LINQ-to-Objects**
(runs in C# over in-memory collections) and **LINQ-to-Entities** (EF translates it
to **SQL**).

**Laravel analogy.** Eloquent Query Builder (DB) vs Collection methods (in memory).
The trap: in C# **the syntax is identical**, so you must know which side you're on.

**Key differences / gotchas.**
- On a `DbSet`/`IQueryable`, the query is **deferred** and becomes SQL only when you
  enumerate it (`ToListAsync`, `FirstAsync`, `foreach`). Build the full query, then
  execute once.
- Calling `.ToList()` **too early** pulls the whole table into memory, then filters
  in C# — the .NET version of an over-fetch / N+1 footgun. Keep operations on
  `IQueryable` until the end.
- Some C# methods can't be translated to SQL → runtime error or silent client-side
  evaluation. Keep DB queries to translatable expressions.

**Code.**
```csharp
// GOOD: filter + project run in SQL, one round trip, only needed columns
var rows = await db.Orders
    .Where(e => e.UserId == userId && e.Amount > 100)
    .Select(e => new { e.Id, e.Amount })
    .ToListAsync();

// BAD: pulls ALL orders into memory, then filters in C#
var bad = (await db.Orders.ToListAsync())
    .Where(e => e.Amount > 100);
```

**When to use / skip.** Use LINQ everywhere — it's central to C#. The discipline is
keeping DB queries on `IQueryable` and materializing once, deliberately.

---

## async / await — and its pitfalls  **[Intermediate]**

**What it is.** Cooperative asynchrony: an `async` method returns a `Task`/`Task<T>`
and `await` yields the thread while waiting for I/O, freeing it to serve other
requests. This is how .NET achieves high throughput.

**Laravel analogy.** PHP request handling is mostly **synchronous** (one request =
one blocked worker). This model is genuinely new — treat it as a core topic
(see `notes/10-async-await.md`).

**Key differences / gotchas.**
- **Never** call `.Result` or `.Wait()` on a Task in web code — it blocks a thread
  and can **deadlock**. Always `await`.
- **Async all the way down**: an async call chain shouldn't have a sync method in
  the middle blocking on a Task.
- Prefer the `...Async` EF/HTTP methods and `await` them.
- `Task.WhenAll(...)` runs independent awaits **concurrently** instead of
  sequentially.
- `IAsyncEnumerable<T>` + `await foreach` streams results without buffering all.
- Pass `CancellationToken` through so requests can be cancelled.

**Code.**
```csharp
public async Task<Report> BuildAsync(int userId, CancellationToken ct)
{
    // concurrent independent queries
    var spentTask = db.Orders.Where(e => e.UserId == userId).SumAsync(e => e.Amount, ct);
    var countTask = db.Orders.CountAsync(e => e.UserId == userId, ct);
    await Task.WhenAll(spentTask, countTask);
    return new Report(spentTask.Result, countTask.Result); // .Result safe AFTER await
}
```

**When to use / skip.** Use async for **all I/O** (DB, HTTP, files). Don't make
**CPU-bound** trivial code async for no reason — async is about I/O wait, not
parallel computation (that's `Task.Run`/parallelism, a separate topic).

---

## Collections & the type you expose  **[Intermediate]**

**What it is.** Common types: `List<T>`, arrays, `Dictionary<K,V>`, `HashSet<T>`,
plus the abstractions `IEnumerable<T>`, `IReadOnlyList<T>`, `IQueryable<T>`. The
**interface you return** signals intent.

**Laravel analogy.** PHP arrays + Collections. C# separates concrete types from the
interfaces, and the interface choice matters for callers.

**Key differences / gotchas.**
- `IEnumerable<T>` = "you can iterate" (may be lazy/streamed). `IReadOnlyList<T>` =
  "materialized, indexable, don't mutate." `IQueryable<T>` = "still a query, may hit
  the DB." Returning `IQueryable` from a service can leak DB execution to callers —
  often you want to materialize first.
- Exposing `List<T>` lets callers mutate your internal state; prefer a read-only
  interface for return values.

**Code.**
```csharp
public IReadOnlyList<OrderResponse> Recent() => _cache;   // callers can't mutate
public async Task<IReadOnlyList<X>> ListAsync() =>
    await db.X.Where(...).ToListAsync();                     // materialize at the edge
```

**When to use / skip.** Return the **least powerful** interface that satisfies the
caller. Use concrete types internally where you need their members.

---

## `Span<T>` / `Memory<T>` — zero-allocation slices  **[Advanced]**

**What it is.** Stack-only (`Span<T>`) and heap-friendly (`Memory<T>`) views over
contiguous memory (arrays, strings, buffers) that let you slice and parse **without
allocating** new arrays/substrings.

**Laravel analogy.** None — PHP doesn't expose this level. It's a performance tool
for hot paths.

**Key differences / gotchas.**
- `Span<T>` can't be stored on the heap (no fields in classes, no async capture).
- Mostly relevant in parsing, serialization, and high-throughput libraries.

**Code.**
```csharp
ReadOnlySpan<char> s = "2026-06-23";
var year = int.Parse(s[..4]);     // no substring allocation
```

**When to use / skip.** **Skip** in normal web/business code. Reach for it only when
a profiler shows allocation/GC pressure on a hot path. Know the name so you
recognize it in library code.

---

# Part 4 — Ecosystem & Production

## Structured logging with Serilog  **[Intermediate]**

**What it is.** Logging where each entry is a **message template with named
properties**, not a pre-formatted string — so logs can be queried by field and
shipped to sinks (console, file, Seq, Elasticsearch).

**Laravel analogy.** Monolog — but the structured-properties model is more
first-class, and it integrates with .NET's built-in `ILogger<T>` abstraction.

**Key differences / gotchas.**
- Log the **template**, not an interpolated string:
  `logger.LogInformation("User {UserId} spent {Amount}", id, amt);` — `{UserId}` and
  `{Amount}` become queryable fields. Using `$"..."` interpolation loses that.
- **Log scopes** attach context (like a correlation id) to every line within a
  block — pairs perfectly with correlation-id middleware.
- `ILogger<T>` is the abstraction you inject; Serilog is one implementation behind
  it, so your code doesn't depend on Serilog directly.

**Code.**
```csharp
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

public class OrderService(ILogger<OrderService> logger)
{
    public void Create(int userId, decimal amount) =>
        logger.LogInformation("User {UserId} created order {Amount}", userId, amount);
}
```

**When to use / skip.** Use structured logging in any real service. Plain
`Console.WriteLine` is fine only for throwaway experiments.

---

## Validation with FluentValidation  **[Intermediate]**

**What it is.** A library for expressing validation rules as fluent C# in a
validator class per DTO, with rich rules, conditions, and async checks.

**Laravel analogy.** Form Requests — but rules are typed C# (`RuleFor(x => x.Email)
.NotEmpty().EmailAddress()`) instead of a string array.

**Key differences / gotchas.**
- There's also built-in **DataAnnotations** (`[Required]`, `[EmailAddress]`
  attributes). DataAnnotations are simpler; FluentValidation handles complex,
  conditional, cross-field, and async rules far better.
- In Minimal APIs you typically resolve the validator from DI and run it explicitly
  (or via a small filter), then return `Results.ValidationProblem(errors)`.

**Code.**
```csharp
public class CreateOrderValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.SpentOn).Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow));
    }
}
```

**When to use / skip.** FluentValidation for anything beyond trivial; DataAnnotations
are acceptable for simple `[Required]`/length checks. Don't run both on the same DTO.

---

## Error handling: `IExceptionHandler` + ProblemDetails  **[Intermediate]**

**What it is.** A global exception handler turns unhandled exceptions into a clean
HTTP response. **ProblemDetails** (RFC 7807) is the standard JSON error shape
(`type`, `title`, `status`, `detail`, `errors`).

**Laravel analogy.** `App\Exceptions\Handler` rendering a consistent JSON error.
ProblemDetails standardizes that shape across the industry.

**Key differences / gotchas.**
- Register `AddProblemDetails()` and `UseExceptionHandler()`; an unhandled exception
  becomes a 500 with a ProblemDetails body — **never leak stack traces** to clients
  in production.
- Return `Results.Problem(...)` / `Results.ValidationProblem(dict)` for expected
  error responses so everything uses the same contract.

**Code.**
```csharp
builder.Services.AddProblemDetails();
app.UseExceptionHandler();          // unhandled -> 500 ProblemDetails

// expected failure:
return Results.ValidationProblem(new Dictionary<string, string[]>
{
    ["Amount"] = ["Amount must be greater than 0."]
});
```

**When to use / skip.** Always have a global handler + a consistent error contract.
Don't invent a custom `{ "error": "..." }` shape — use ProblemDetails.

---

## Health checks  **[Intermediate]**

**What it is.** Endpoints that report whether the app (and its dependencies, like
the DB) are healthy — consumed by load balancers, Kubernetes, and uptime monitors.

**Laravel analogy.** Laravel 11's `/up` route, but extensible: you can register
checks for the database, cache, external APIs, etc.

**Key differences / gotchas.**
- `AddDbContextCheck<AppDbContext>()` makes `/health` actually verify DB
  connectivity — a green check then means "can serve traffic," not just "process is
  up."
- Separate **liveness** (is the process alive?) from **readiness** (can it serve?)
  via tags if you deploy to k8s.

**Code.**
```csharp
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
app.MapHealthChecks("/health");
```

**When to use / skip.** Add for anything deployed behind an orchestrator or LB.
Optional for a local-only learning toy, but it's a strong reviewer signal.

---

## Rate limiting  **[Intermediate]**

**What it is.** Built-in middleware (since .NET 7) to cap request rates per client
or endpoint, using algorithms like fixed window, sliding window, token bucket, and
concurrency limits.

**Laravel analogy.** The `throttle:60,1` middleware — same intent, configured in C#.

**Key differences / gotchas.**
- Define named limiters, then attach with `.RequireRateLimiting("name")` on specific
  endpoints (e.g. a tight limit on `/auth/login` to blunt brute force).
- Choose the algorithm to match intent: fixed window is simplest; token bucket
  allows bursts.

**Code.**
```csharp
builder.Services.AddRateLimiter(o => o.AddFixedWindowLimiter("auth", w =>
{
    w.Window = TimeSpan.FromMinutes(1);
    w.PermitLimit = 5;
}));
app.UseRateLimiter();
// loginEndpoint.RequireRateLimiting("auth");
```

**When to use / skip.** Use on auth and other abuse-prone or expensive endpoints.
You don't need it on everything.

---

## OpenAPI / Swagger docs  **[Basic]**

**What it is.** Auto-generated, interactive API documentation from your endpoints.
.NET 10 ships built-in OpenAPI generation; **Swashbuckle**/**Scalar** add a UI.

**Laravel analogy.** Scribe / L5-Swagger. Here the document is generated from your
strongly-typed endpoints, so it stays in sync with the code.

**Key differences / gotchas.**
- Add a JWT "Authorize" button in the UI so you can test protected endpoints with a
  bearer token.
- Typically enable the UI only in Development.

**Code.**
```csharp
builder.Services.AddOpenApi();      // built-in document
if (app.Environment.IsDevelopment())
    app.MapOpenApi();               // serve the spec (add Swagger/Scalar UI on top)
```

**When to use / skip.** Always for an API — it's near-free and makes the API
self-documenting and testable.

---

## Resilience with Polly  **[Advanced]**

**What it is.** A resilience library for **retries**, **circuit breakers**,
**timeouts**, **bulkheads**, and **fallbacks** — mainly for outbound calls to other
services. Integrated with `HttpClient` via `Microsoft.Extensions.Http.Resilience`.

**Laravel analogy.** No direct equivalent in core Laravel; you'd hand-roll retries.

**Key differences / gotchas.**
- A **circuit breaker** stops hammering a failing dependency, giving it time to
  recover — important in distributed systems.
- Don't blindly retry **non-idempotent** operations (e.g. a payment POST) — you can
  double-charge.

**Code.**
```csharp
builder.Services.AddHttpClient("api")
    .AddStandardResilienceHandler();   // sensible retry + circuit breaker defaults
```

**When to use / skip.** Use when calling external/unreliable services. Skip for an
app with no outbound dependencies.

---

## Testing stack: xUnit, WebApplicationFactory, Testcontainers  **[Basic→Intermediate]**

**What it is.** **xUnit** is the common test framework (with **FluentAssertions** for
readable asserts, **Moq/NSubstitute** for mocking). **`WebApplicationFactory<T>`**
spins up the whole app in-memory for **integration tests** hitting real endpoints.
**Testcontainers** runs a real Postgres in Docker for tests that need a true DB.

**Laravel analogy.** PHPUnit/Pest; `WebApplicationFactory` ≈ Laravel's HTTP testing
(`$this->getJson(...)`); Testcontainers ≈ spinning a real DB for feature tests
instead of SQLite-in-memory.

**Key differences / gotchas.**
- Prefer **integration tests via `WebApplicationFactory`** over heavily-mocked unit
  tests for web APIs — they catch wiring/DI/serialization bugs unit tests miss.
- The EF **in-memory provider** doesn't behave like a real relational DB (no real
  SQL, constraints, transactions) — prefer Testcontainers Postgres for DB-accurate
  tests. See `notes/11-testing.md`.

**Code.**
```csharp
public class OrdersTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Unauthenticated_request_is_401()
    {
        var client = factory.CreateClient();
        var res = await client.GetAsync("/orders");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

**When to use / skip.** Always have tests. Reach for Testcontainers when in-memory
fakes would hide real DB behavior; keep fast unit tests for pure logic.

---

## Deployment: Docker, config, AOT/trimming  **[Intermediate]**

**What it is.** Package the app as a **multi-stage Docker image** (SDK builds →
slim runtime runs), configure via environment variables, run as non-root. **AOT
(Native AOT)** and **trimming** produce smaller, faster-starting binaries by
compiling ahead of time and removing unused code.

**Laravel analogy.** Containerizing a Laravel app — but .NET's compile step means a
build stage, and AOT has no PHP analog (PHP is interpreted).

**Key differences / gotchas.**
- Keep the runtime image (`aspnet`) separate from the build image (`sdk`) so the
  shipped image has no compiler.
- AOT/trimming can break code relying on **reflection** (some serializers, DI edge
  cases) — test thoroughly before adopting.

**Code.**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# ...restore, publish...
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
USER app
ENTRYPOINT ["dotnet", "App.dll"]
```

**When to use / skip.** Multi-stage Docker is standard for shipping. Use AOT/trimming
only when fast cold-start / small image truly matters and you've verified nothing
reflection-based breaks.

---

## Observability with OpenTelemetry  **[Advanced]**

**What it is.** A vendor-neutral standard for **traces, metrics, and logs**.
Instrument once, export to many backends (Jaeger, Prometheus, Grafana, Azure
Monitor, etc.). .NET has first-class OTel support.

**Laravel analogy.** Broader and more standardized than Telescope; it's about
distributed tracing across services, not a single-app debugger.

**Key differences / gotchas.**
- Most valuable in **multi-service** systems where a request crosses boundaries and
  you need an end-to-end trace.
- For a single small app, structured logging + health checks usually suffice.

**Code.**
```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddAspNetCoreInstrumentation().AddEntityFrameworkCoreInstrumentation())
    .WithMetrics(m => m.AddAspNetCoreInstrumentation());
```

**When to use / skip.** Adopt for distributed/production systems that need tracing
and metrics. Overkill for a learning monolith — know it exists.

---

## Where to go next

- The hands-on application of many of these lives in the per-feature build notes
  (project-specific) under `docs/` if this repo has them.
- For the focused topic deep-dives, see the numbered notes `01`–`11` in this folder.
- Rule of thumb: get comfortable across **[Basic]** and most **[Intermediate]**
  topics before going deep on any **[Advanced]** one — and remember that choosing
  the simpler option on purpose is itself a senior decision.



