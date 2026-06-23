# .NET Learning Starter

A small, batteries-included **ASP.NET Core (.NET 10) Web API** starter for
learning .NET — built by (and for) a Laravel/PHP developer. It pairs a clean
project setup (Docker + PostgreSQL, secrets, style enforcement) with a folder of
**learning notes** that map .NET concepts back to their Laravel equivalents.

> This repo is meant to be cloned and built up feature-by-feature in **teaching
> mode**: explain the concept first, then write small, reviewable chunks of code.

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for the PostgreSQL database / containerized run)

## Quick start

### Local dev (fast feedback — recommended while learning)
Run the database in Docker, the app on your machine:

```bash
docker compose up -d db                     # PostgreSQL on localhost:5432
dotnet user-secrets set "Jwt:Secret" "dev-only-value-at-least-32-chars-long" \
  --project src/DotnetApp                    # local secret, stored outside the repo
dotnet run --project src/DotnetApp          # or: dotnet watch --project src/DotnetApp
```

### Everything in Docker (prod-like)
```bash
docker compose up --build                   # builds the image, starts db + api
# API available at http://localhost:8080
docker compose down                         # stop (keeps the database volume)
docker compose down -v                      # stop and wipe the database volume
```

## Scaffold a brand-new app

The .NET equivalent of `laravel new` is `dotnet new` — no extra tooling needed:

```bash
dotnet new sln -n MyApp --format slnx       # the solution (workspace)
dotnet new webapi -o src/MyApp              # the Web API project
dotnet sln add src/MyApp/MyApp.csproj       # wire it into the solution
```

See [notes/03-dotnet-cli.md](notes/03-dotnet-cli.md) for the full CLI tour.

## Project structure

```
.
├── src/DotnetApp/            # the Web API (entry point: Program.cs)
├── notes/                    # general .NET learning (Laravel ↔ .NET mapping)
│   ├── 00-learning-plan.md   #   topic-by-topic index, start here
│   ├── 01..11-*.md           #   focused topics (DI, EF Core, middleware, ...)
│   └── foundations.md        #   detailed reference (tooling, patterns, idioms)
├── docs/                     # knowledge specific to the app built in this repo *
├── Dockerfile                # multi-stage build (SDK build → slim runtime)
├── docker-compose.yml        # api + postgres services
├── CLAUDE.md / AGENTS.md     # working agreements for AI assistants
└── DotnetApp.slnx            # the solution

* docs/ and AGENTS.md describe whatever app this clone is building; they may be
  absent in a fresh, generic clone.
```

## Key commands

```bash
dotnet build                                 # compile (catches type errors early)
dotnet run    --project src/DotnetApp        # run the app   (≈ php artisan serve)
dotnet watch  --project src/DotnetApp        # hot-reload dev server
dotnet test                                  # run tests     (≈ php artisan test)
dotnet format                                # auto-fix code style
dotnet add src/DotnetApp package <Name>      # add a NuGet package (≈ composer require)
```

## Configuration & secrets

Configuration is layered; later sources win:

```
appsettings.json → appsettings.{Environment}.json → user-secrets (Dev) → env vars
```

- **Local secrets** live in user-secrets (outside the repo) — set with
  `dotnet user-secrets set "Key" "value"`.
- **Docker / production** uses environment variables; nested keys use a double
  underscore, e.g. `ConnectionStrings__Default`.
- Never commit secrets to `appsettings.json`.

See [notes/foundations.md](notes/foundations.md) for the full explanation.

## Learning approach

- **`notes/`** — general .NET knowledge that applies to any project. Start at
  [notes/00-learning-plan.md](notes/00-learning-plan.md).
- **`docs/`** — knowledge specific to the app this repo is building (if present).
- Each .NET concept is explained against its **Laravel/PHP** equivalent, the
  difference called out, then shown in a minimal example.

## Tech stack

ASP.NET Core (Minimal APIs) · Entity Framework Core + Npgsql (PostgreSQL) ·
JWT auth · FluentValidation · Serilog · Docker. See
[notes/foundations.md](notes/foundations.md) for what each is and when to use it.
