# 09 — Config & Environment (vs .env)

## Short version
.NET configuration is **layered**: `appsettings.json` → `appsettings.{Environment}.json` →
environment variables → user secrets. Later layers override earlier ones. There's no single
`.env` file by default (though you can add one).

## Laravel analogy
- `.env` → **environment variables + `appsettings.json`**
- `config/*.php` (typed config) → **`appsettings.json` sections** read via `IConfiguration`
- `config('app.name')` → **`builder.Configuration["App:Name"]`**
- `APP_ENV=local` → **`ASPNETCORE_ENVIRONMENT=Development`**

## Reading config
```jsonc
// appsettings.json
{
  "ConnectionStrings": { "Default": "Data Source=app.db" },
  "App": { "Name": "DotnetApp", "PageSize": 20 }
}
```
```csharp
var name = builder.Configuration["App:Name"];           // ":" navigates nested keys
var size = builder.Configuration.GetValue<int>("App:PageSize");
var conn = builder.Configuration.GetConnectionString("Default");
```

## Strongly-typed config (the idiomatic way)
Bind a config section to a class — there's no Laravel equivalent, and it's nicer than
`config('...')` strings:
```csharp
public class AppSettings { public string Name { get; set; } = ""; public int PageSize { get; set; } }
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));
// inject IOptions<AppSettings> wherever you need it
```

## Environments
`ASPNETCORE_ENVIRONMENT` (set in `Properties/launchSettings.json` for local dev) decides which
`appsettings.{env}.json` loads and toggles things like the OpenAPI/dev tooling — you can see
`if (app.Environment.IsDevelopment())` already in `Program.cs`.

| Laravel | .NET |
|---|---|
| `local` | `Development` |
| `production` | `Production` |
| `App::environment()` | `app.Environment.EnvironmentName` |

## Secrets
- **Local dev:** `dotnet user-secrets` (keeps secrets out of the repo) — better than `.env`.
  ```bash
  dotnet user-secrets init --project src/DotnetApp
  dotnet user-secrets set "App:ApiKey" "xyz" --project src/DotnetApp
  ```
- **Production:** real environment variables (same as you'd do for Laravel).

## Key differences
- Config is **JSON + layered**, not a flat `.env`.
- **Never** put secrets in `appsettings.json` (it's committed) — use user-secrets or env vars.
- Nested keys use `:` (`App:Name`); in env vars use `__` (double underscore): `App__Name`.
