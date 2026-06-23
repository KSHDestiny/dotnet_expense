# 04 — Routing & Controllers (vs Laravel routes/controllers)

## Short version
ASP.NET Core has **two styles**: **Minimal APIs** (routes defined inline, like a `routes/api.php`
on steroids) and **Controllers** (classes, like Laravel controllers). This project uses
minimal APIs.

## Minimal APIs ≈ routes/api.php
```php
// Laravel: routes/api.php
Route::get('/weather', [WeatherController::class, 'index']);
```

```csharp
// .NET: Program.cs
app.MapGet("/weatherforecast", () => { ... });   // you already have this
app.MapPost("/users", (CreateUser dto) => { ... });
app.MapGet("/users/{id}", (int id) => { ... });   // route params are typed
```

`MapGet/MapPost/MapPut/MapDelete` ≈ `Route::get/post/put/delete`.

## Controllers ≈ Laravel controllers
For bigger apps, controllers keep things organized:

```csharp
[ApiController]
[Route("api/[controller]")]          // [controller] → "users" from UsersController
public class UsersController : ControllerBase
{
    [HttpGet]                         // GET api/users
    public IEnumerable<User> Index() => ...;

    [HttpGet("{id}")]                 // GET api/users/{id}
    public ActionResult<User> Show(int id) => ...;

    [HttpPost]                        // POST api/users
    public ActionResult Store(CreateUser dto) => ...;
}
```

The `[HttpGet]`, `[Route]` things are **attributes** — metadata on methods. Laravel uses route
files or PHP 8 attributes; .NET leans heavily on attributes.

## Key differences

| Laravel | .NET |
|---|---|
| Routes in `routes/*.php`, separate from controllers | Routes inline (minimal) or via attributes on the controller method |
| `Request $request` injected | Parameters bound from route/query/body **by type** automatically |
| `return response()->json($data)` | `return data;` — objects are auto-serialized to JSON |
| `return response()->json($x, 201)` | `return Results.Created(...)` or `return CreatedAtAction(...)` |
| Route model binding | Model binding too, but you wire the lookup yourself |

## Model binding (where params come from)
.NET decides automatically: `{id}` in the route → from URL; a complex type like `CreateUser`
→ from the JSON body; simple types not in the route → from query string. You can force it with
`[FromBody]`, `[FromQuery]`, `[FromRoute]`.

## Try it
Add a second minimal-API route to `Program.cs`, e.g. `app.MapGet("/ping", () => "pong");`,
run `dotnet watch`, and hit it.
