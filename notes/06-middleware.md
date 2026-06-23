# 06 — Middleware & the Request Pipeline (vs Laravel middleware)

## Short version
ASP.NET Core processes each request through a **pipeline** of middleware, configured in
`Program.cs` with `app.Use...()` calls. The concept is nearly identical to Laravel; the
ordering and syntax differ.

## Laravel analogy
Laravel: middleware registered in `Kernel.php`, applied globally or per-route.
.NET: middleware added **in order** in `Program.cs`. **Order matters** — they run top-to-bottom
on the way in, bottom-to-top on the way out.

```csharp
var app = builder.Build();

app.UseHttpsRedirection();   // you already have this
app.UseAuthentication();     // who are you?
app.UseAuthorization();      // are you allowed?
app.MapControllers();        // finally, hit the endpoint

app.Run();
```

## Built-in middleware ≈ Laravel's

| Laravel middleware | .NET equivalent |
|---|---|
| `TrustProxies` / HTTPS | `app.UseHttpsRedirection()` |
| `Authenticate` | `app.UseAuthentication()` |
| Authorization / `can:` | `app.UseAuthorization()` |
| CORS | `app.UseCors()` |
| Exception handling | `app.UseExceptionHandler()` |

## Writing custom middleware
Laravel: a class with `handle($request, Closure $next)`.
.NET: inline or a class — the `next` delegate is the same idea.

```csharp
// inline
app.Use(async (context, next) =>
{
    // before the request (like code before $next($request))
    Console.WriteLine($"--> {context.Request.Path}");
    await next();                 // call the rest of the pipeline
    // after the response (like code after $next($request))
    Console.WriteLine($"<-- {context.Response.StatusCode}");
});
```

## Key differences
- **Explicit ordering.** In Laravel the kernel mostly handles order. In .NET *you* place each
  `app.Use...()` in the right spot — auth must come before authorization, etc. Getting the order
  wrong is a common beginner bug.
- **`async/await`** — middleware is async by default (see note 10).
- The terminal middleware is your endpoint (`MapGet`/`MapControllers`).
