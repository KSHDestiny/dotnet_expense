# 05 — Dependency Injection (vs Laravel's service container)

## Short version
.NET has a **built-in DI container**. You register services at startup, and they get injected
into controllers/endpoints automatically. This is one of the closest mappings to Laravel.

## Laravel analogy
```php
// Laravel: bind in a ServiceProvider
$this->app->bind(PaymentGateway::class, StripeGateway::class);

// then inject in a controller — Laravel resolves it
public function pay(PaymentGateway $gateway) { ... }
```

```csharp
// .NET: register in Program.cs
builder.Services.AddScoped<IPaymentGateway, StripeGateway>();

// inject into an endpoint — .NET resolves it
app.MapPost("/pay", (IPaymentGateway gateway) => gateway.Charge());
```

`builder.Services` **is** the service container, configured in `Program.cs` (≈ your
`AppServiceProvider::register()`).

## Lifetimes — the important concept
When you register, you choose a **lifetime** (how long one instance lives):

| .NET method | Lifetime | Laravel rough equivalent |
|---|---|---|
| `AddTransient` | New instance every time it's requested | `bind` (default) |
| `AddScoped` | One instance **per HTTP request** | `scoped` |
| `AddSingleton` | One instance for the whole app | `singleton` |

For most services (especially anything touching EF Core / a request), use **`AddScoped`**.

## Key differences
- **Interfaces are idiomatic.** You usually register `AddScoped<IThing, Thing>()` — code depends
  on the interface `IThing`, the container supplies `Thing`. Laravel does this too but C# leans
  on interfaces harder.
- **Constructor injection** is the norm in controllers:
  ```csharp
  public class UsersController(IUserService users) : ControllerBase   // primary constructor
  {
      [HttpGet] public IEnumerable<User> Index() => users.All();
  }
  ```
- **No facades.** Laravel's `User::all()` / `Cache::get()` static-style facades don't exist.
  Everything is injected. (More verbose, but explicit and testable.)

## Try it
1. Make an interface + class:
   ```csharp
   public interface IGreeter { string Greet(string name); }
   public class Greeter : IGreeter { public string Greet(string name) => $"Hi {name}"; }
   ```
2. Register: `builder.Services.AddScoped<IGreeter, Greeter>();`
3. Use it: `app.MapGet("/hi/{name}", (string name, IGreeter g) => g.Greet(name));`
