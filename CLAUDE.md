# CLAUDE.md

Guidance for Claude Code when working in this repository.

## About me

I'm an experienced **Laravel / PHP developer learning .NET** (ASP.NET Core Web API).
When you explain .NET concepts, **map them back to Laravel equivalents** wherever possible —
that's the fastest way for me to understand. Assume I'm fluent in MVC, Eloquent, Artisan,
Composer, middleware, service containers, and Blade, but new to C# syntax and the .NET ecosystem.

## Project

- **Type:** ASP.NET Core Web API (minimal API style), .NET 10
- **Main project:** `src/DotnetApp/` — entry point is `Program.cs`
- **Solution:** `DotnetApp.slnx`
- **Learning notes:** `notes/` — general .NET learning (topic-by-topic plan, the
  Laravel↔.NET mapping). Reusable across projects.
- **Project docs:** `docs/` — knowledge tied to the *specific* app built in this
  repo (build plan, design decisions, gotchas). If a `docs/` index exists, start
  there. Project-specific working agreements live in `AGENTS.md` (if present).

## How I want you to work

### Teaching mode (default)
This is a learning project, so **prioritize explanation over just shipping code**:
- When you write C#, briefly explain *why* it's written that way, especially where it differs
  from how Laravel/PHP would do it.
- Call out **C# / .NET idioms** I might not know (LINQ, async/await, dependency injection,
  records, nullable reference types, attributes).
- When a Laravel concept has a direct .NET equivalent, name both. Examples:
  - Eloquent → **Entity Framework Core**
  - Artisan → **dotnet CLI**
  - Composer / `composer.json` → **NuGet / `.csproj`**
  - `php artisan serve` → **`dotnet run`**
  - Middleware → **Middleware** (similar concept, different syntax)
  - Service container / `app()->make()` → **built-in DI container**
  - Form Requests / Validation → **DataAnnotations / FluentValidation**
  - Routes file → **endpoint mapping in `Program.cs`** or **Controllers**
  - `.env` → **`appsettings.json` + environment variables**
  - Tinker → **C# Interactive / `dotnet fsi`** (rough equivalent)

### Pacing — IMPORTANT
**Build one feature at a time. I drive.** Do not implement multiple features in
one burst. For each feature: explain the concept first (using the structure
below), then write the code in small chunks, then **STOP and wait** for me to say
"go"/"next" before starting the next feature.

### Code style
- Prefer clear, idiomatic, modern C# (file-scoped namespaces, `var` where the type is obvious,
  records for DTOs, async all the way down for I/O).
- Match the existing style in `Program.cs` and surrounding files.
- Don't add abstractions I haven't asked for — keep it simple while I'm learning.

### When I ask "how does X work in .NET"
Structure the answer as:
1. **Short answer** in .NET terms.
2. **Laravel analogy** — the closest equivalent I already know.
3. **Key differences** — where the analogy breaks down (this is the important part).
4. **Minimal code example.**

## Commands

```bash
dotnet run --project src/DotnetApp      # run the app (like `php artisan serve`)
dotnet build                            # compile (PHP has no compile step; this catches type errors early)
dotnet watch --project src/DotnetApp    # hot-reload dev server (like Vite/artisan with --watch)
dotnet test                             # run tests (like `php artisan test` / PHPUnit)
dotnet add src/DotnetApp package <Name> # add a NuGet package (like `composer require`)
```

## Notes & docs folders

Two folders, deliberately separate:
- **`notes/`** — general .NET learning that applies beyond any one project. If I
  ask about a topic that has a note file here, update or reference it.
- **`docs/`** — knowledge specific to the app built in this repo (build plan,
  per-feature notes, references). When building a feature, update its `docs/` note.

Keep both practical and example-driven, always with the Laravel comparison.
