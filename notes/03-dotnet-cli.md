# 03 — The `dotnet` CLI (your Artisan + Composer)

## Short version
The `dotnet` command is **Artisan and Composer combined**: it scaffolds, runs, builds,
manages packages, and runs migrations.

## Command map

| Laravel | .NET | What it does |
|---|---|---|
| `php artisan serve` | `dotnet run --project src/DotnetApp` | Run the app |
| (Vite watch / nodemon) | `dotnet watch --project src/DotnetApp` | Hot-reload on file change |
| — (no compile in PHP) | `dotnet build` | Compile + type-check |
| `composer require x/y` | `dotnet add src/DotnetApp package Y` | Add a dependency |
| `composer install` | `dotnet restore` | Restore dependencies (usually automatic) |
| `php artisan test` | `dotnet test` | Run tests |
| `php artisan make:controller` | `dotnet new <template>` | Scaffold from a template |
| `php artisan migrate` | `dotnet ef database update` | Run EF Core migrations (needs `dotnet-ef` tool) |
| `php artisan make:migration` | `dotnet ef migrations add <Name>` | Create a migration |

## Key differences
- **`dotnet build` has no Laravel equivalent** — PHP is interpreted. Get used to building;
  it catches a whole class of bugs before runtime.
- **`dotnet new`** is template-based scaffolding for whole projects, not individual classes.
  List templates with `dotnet new list`.
- **EF Core tools are a separate install:**
  ```bash
  dotnet tool install --global dotnet-ef
  ```
  (≈ a global Composer package.)

## Most-used commands while learning
```bash
dotnet run --project src/DotnetApp     # start the API
dotnet watch --project src/DotnetApp   # start with hot reload (use this most)
dotnet build                           # just type-check / compile
dotnet add src/DotnetApp package <Name>
```
