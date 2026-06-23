# 02 — Project Structure (vs a Laravel app)

## Short version
A .NET app is a **solution** (`.slnx`) containing one or more **projects** (`.csproj`).
There's no enforced `app/Http/Controllers` folder structure like Laravel — you organize it
yourself.

## Laravel analogy
- Laravel project root with `composer.json` → **a project folder with a `.csproj`**
- `composer.json` → **`.csproj`** (lists dependencies + settings, in XML)
- `vendor/` → **`obj/` and `bin/`** (compiled output + restored packages; git-ignore them)
- `public/index.php` (entry) → **`Program.cs`** (entry point)
- `.env` → **`appsettings.json`**

## This project's layout
```
dotnet_app/
├── DotnetApp.slnx              # solution — groups projects (like a workspace)
├── CLAUDE.md
├── notes/                      # your learning notes
└── src/
    └── DotnetApp/
        ├── DotnetApp.csproj    # the project manifest (≈ composer.json)
        ├── Program.cs          # entry point + app setup (≈ public/index.php + bootstrap/app.php + routes)
        ├── appsettings.json    # config (≈ .env + config/*.php)
        └── Properties/
            └── launchSettings.json   # local dev ports/profiles
```

## Key differences
- **No magic folders.** Laravel auto-discovers controllers/migrations by convention. In .NET
  you wire most things up explicitly in `Program.cs` (or via attributes). More boilerplate,
  more transparency.
- **`Program.cs` does a lot.** In minimal APIs it's the routes file, the service registration
  (`AppServiceProvider`), AND the bootstrap — all in one place. We'll split it as the app grows.
- **Compiled output** (`bin/`, `obj/`) is generated — never edit, always git-ignore.

## A typical grown-up structure (for later)
As the app grows you'll add folders by convention (yours to choose), e.g.:
```
src/DotnetApp/
├── Endpoints/ or Controllers/   # routing
├── Models/                      # EF Core entities (≈ app/Models)
├── Data/                        # DbContext (≈ database connection/config)
├── Services/                    # business logic (≈ app/Services)
└── Dtos/                        # request/response records (≈ Form Requests / API Resources)
```
