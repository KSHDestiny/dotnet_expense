# Step 2 — DI Registration Extensions + Options Pattern

Keeping the composition root (`Program.cs`) thin as the app grows, and binding
config to strongly-typed classes instead of magic strings.

> Status: ✅ complete. (JWT options class + config ready; binding/validation wired in Step 4.)

---

## Why

A real app registers dozens of services. If every `AddXxx` call lives directly in
`Program.cs`, it becomes an unreadable wall. Two conventions fix this:

1. **`IServiceCollection` extension methods** — group registrations by concern, one
   extension per feature, so `Program.cs` reads like a table of contents.
2. **Options pattern** — bind a config section to a typed class once; inject
   `IOptions<T>` instead of reaching into `IConfiguration` with string keys.

---

## Pattern 1 — service registration extensions

An extension method is `static`, first parameter `this IServiceCollection`, returns
`IServiceCollection` so calls **chain**.

```csharp
public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("Default"))
                   .UseSnakeCaseNamingConvention());
        return services;
    }
}
```

**Decision:** extensions live **beside their feature** (e.g.
`Infrastructure/Persistence/PersistenceServiceCollectionExtensions.cs`), not in a
central folder — each concern owns its wiring. Scales with vertical slices.

`Program.cs` then reads:
```csharp
builder.Services.AddPersistence(builder.Configuration);
```

---

## Pattern 2 — Options pattern (strongly-typed config)

```csharp
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpiryMinutes { get; set; }
}
```

Bind once at startup, with **fail-fast validation**:
```csharp
services.AddOptions<JwtOptions>()
    .Bind(config.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();          // app won't start if config is invalid/missing
```

Inject anywhere as `IOptions<JwtOptions>` (or `IOptionsSnapshot<T>` for per-request
reload). No magic strings, compile-time property names, one place to change.

**Why now:** demonstrates the pattern this step; structure is ready for Step 4 (JWT
issuing). Matches the `Jwt__*` env vars already in `docker-compose.yml`.

---

## Chunks

- [x] 2a — `AddPersistence()` extension; moved DbContext reg out of `Program.cs`
- [x] 2b — `JwtOptions` class (DataAnnotations validation); binding deferred to Step 4
- [x] 2c — `Jwt` section in `appsettings.json` (Issuer/Audience/ExpiryMinutes only)
- [x] 2d — slimmed `Program.cs`; build clean

## Secrets handling (important)

`Jwt:Secret` is **not** in `appsettings.json` (would be committed to git). Only
non-secret structure lives there. The secret comes from:
- **Docker:** env var `Jwt__Secret` (already in `docker-compose.yml`); `__` → `:`.
- **Local dev:** `dotnet user-secrets` (covered in Step 4), never a committed file.

Config sources layer; env vars override JSON.

## Final `Program.cs` registration

```csharp
builder.Services.AddOpenApi();
builder.Services.AddPersistence(builder.Configuration);
```
