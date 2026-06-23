# 10 — async / await (the thing PHP mostly doesn't have)

## Short version
.NET is **async-first** for I/O (DB, HTTP, files). Methods that do I/O return a `Task<T>` and
you `await` them. This frees the thread to handle other requests while waiting — a server
scalability win PHP's request-per-process model gets differently.

## Why this is new to you
In Laravel/PHP, code runs top-to-bottom and blocks on I/O; each request typically gets its own
worker. In .NET, one process handles many concurrent requests, so blocking a thread is wasteful.
`await` says "pause here, let the thread do other work, resume when the data's ready."

## Syntax
```csharp
// 'async' on the method, 'await' on the I/O call, return Task<T>
public async Task<List<User>> GetUsersAsync(AppDbContext db)
{
    var users = await db.Users.ToListAsync();   // await the DB
    return users;
}
```
In endpoints:
```csharp
app.MapGet("/users", async (AppDbContext db) => await db.Users.ToListAsync());
```

## Rules of thumb
1. **Async all the way down.** If you `await` something, your method is `async` and returns
   `Task`/`Task<T>`. Callers `await` you too. Don't break the chain.
2. **Use the `...Async` methods** for EF Core / HttpClient (`ToListAsync`, `FindAsync`,
   `SaveChangesAsync`, `GetAsync`).
3. **Don't block on async** — never `.Result` or `.Wait()` on a Task; that can deadlock. Always
   `await`.
4. **`Task`** ≈ a promise/future. `Task<T>` returns a value; `Task` returns nothing (like
   `void` but async).

## Laravel comparison

| Concept | Laravel/PHP | .NET |
|---|---|---|
| I/O model | Blocking, request-per-worker | Async, many requests per thread |
| Returning a value from I/O | Just `return $x;` | `return await ...;` |
| "Promise" | (queues / not core) | `Task<T>` |

## Mental shortcut
Wherever you'd touch the DB, network, or filesystem: make the method `async`, add `await`, and
use the `Async` variant. Everything else (pure logic, mapping) stays normal/synchronous.
