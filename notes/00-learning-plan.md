# .NET Learning Plan (for a Laravel Developer)

A step-by-step path from Laravel/PHP to ASP.NET Core. Work through these in order.
Each note has the same structure: **concept → Laravel analogy → key differences → code**.

## The big picture first

| Laravel / PHP | .NET equivalent | Notes |
|---|---|---|
| PHP (interpreted) | **C#** (compiled) | C# is statically typed and compiled. Type errors caught at build, not runtime. |
| Laravel (framework) | **ASP.NET Core** | The web framework on top of the runtime. |
| Composer | **NuGet** | Package manager. |
| `composer.json` | **`.csproj`** | Project + dependency manifest. |
| Artisan | **`dotnet` CLI** | Scaffolding, running, building, migrations. |
| `php artisan serve` | **`dotnet run`** | But .NET compiles first. |
| Eloquent ORM | **Entity Framework Core (EF Core)** | The ORM. |
| Blade | **Razor** | (Only if doing MVC/Blazor; we're doing Web API so mostly JSON.) |
| PSR / Composer autoload | **Namespaces + assemblies** | No autoload file; the compiler resolves it. |

## Topics (in order)

1. **[01-csharp-basics.md](01-csharp-basics.md)** — C# syntax vs PHP: types, classes, properties, `var`.
2. **[02-project-structure.md](02-project-structure.md)** — Solution, project, `.csproj`, `Program.cs`. Where things live vs Laravel.
3. **[03-dotnet-cli.md](03-dotnet-cli.md)** — The `dotnet` CLI as your Artisan + Composer.
4. **[04-routing-controllers.md](04-routing-controllers.md)** — Minimal APIs and Controllers vs Laravel routes/controllers.
5. **[05-dependency-injection.md](05-dependency-injection.md)** — Built-in DI container vs Laravel's service container.
6. **[06-middleware.md](06-middleware.md)** — Request pipeline vs Laravel middleware.
7. **[07-ef-core.md](07-ef-core.md)** — Entity Framework Core vs Eloquent (migrations, models, queries).
8. **[08-validation.md](08-validation.md)** — DataAnnotations / FluentValidation vs Form Requests.
9. **[09-config-env.md](09-config-env.md)** — `appsettings.json` + environment vs `.env`.
10. **[10-async-await.md](10-async-await.md)** — async/await — the thing PHP mostly doesn't have.
11. **[11-testing.md](11-testing.md)** — xUnit vs PHPUnit/Pest.

## Going beyond the basics

- **[foundations.md](foundations.md)** — the big detailed reference. Four
    parts: **tooling & config** (`global.json`, `Directory.Build.props`, central
    package mgmt, analyzers, secrets), **design patterns** (DI/lifetimes, Options,
    Minimal APIs vs Controllers, Result, Repository, CQRS/MediatR — with *when not
    to use them*), **language & runtime idioms** (records, pattern matching, LINQ
    DB-vs-memory, async pitfalls, nullable refs, `Span<T>`), and **ecosystem &
    production** (Serilog, FluentValidation, ProblemDetails, health checks, rate
    limiting, OpenAPI, Polly, testing, Docker, OpenTelemetry). Each topic gets the
    full treatment — what it is, Laravel analogy, gotchas, code, when to use/skip —
    tagged **Basic / Intermediate / Advanced**.

> **Applying these to a real app?** If this repo is building a specific app, its
> hands-on build notes live in [`../docs/`](../docs/) — that folder holds
> project-specific notes & references. This `notes/` folder is for **general .NET
> learning** that outlives any one project.

## How to use this with Claude

Tell Claude which topic you're on (e.g. "let's do topic 5, DI"). It will explain in Laravel
terms first, then show the .NET way, then have you build something small in this project.
