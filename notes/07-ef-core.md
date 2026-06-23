# 07 — Entity Framework Core (vs Eloquent)

## Short version
**EF Core** is the .NET ORM. Like Eloquent, it maps classes to tables and gives you migrations
and a query API (LINQ). The biggest difference: EF Core is **not** Active Record — entities are
plain classes, and you query/save through a **`DbContext`**, not through the model itself.

## Setup (once)
```bash
dotnet tool install --global dotnet-ef                       # the migration CLI tool
dotnet add src/DotnetApp package Microsoft.EntityFrameworkCore.Sqlite   # or .SqlServer / Npgsql for Postgres
dotnet add src/DotnetApp package Microsoft.EntityFrameworkCore.Design
```

## Model: entity ≈ Eloquent model
```php
// Laravel
class User extends Model { protected $fillable = ['name', 'email']; }
```
```csharp
// EF Core — just a plain class (POCO)
public class User
{
    public int Id { get; set; }          // convention: 'Id' is the primary key
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}
```

## DbContext — the key new concept
There's no `User::all()`. Instead you have a `DbContext` that holds your tables (`DbSet<T>`)
and you inject it. Think of it as the DB connection + all your models registered together.

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();   // ≈ the 'users' table
}
```
Register it in `Program.cs` (DI again — see note 05):
```csharp
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=app.db"));
```

## Queries — Eloquent vs LINQ

| Eloquent | EF Core (LINQ) |
|---|---|
| `User::all()` | `await db.Users.ToListAsync()` |
| `User::find($id)` | `await db.Users.FindAsync(id)` |
| `User::where('age', '>', 18)->get()` | `await db.Users.Where(u => u.Age > 18).ToListAsync()` |
| `User::first()` | `await db.Users.FirstOrDefaultAsync()` |
| `$user->save()` (create) | `db.Users.Add(user); await db.SaveChangesAsync();` |
| `$user->save()` (update) | mutate the object, then `await db.SaveChangesAsync();` |
| `$user->delete()` | `db.Users.Remove(user); await db.SaveChangesAsync();` |
| `User::with('posts')` (eager) | `db.Users.Include(u => u.Posts)` |

Note **`SaveChangesAsync()`**: EF Core tracks changes and writes them in one batch — unlike
Eloquent where each `->save()` hits the DB immediately. This is the "Unit of Work" pattern.

## Migrations ≈ Laravel migrations
```bash
dotnet ef migrations add CreateUsers     # ≈ php artisan make:migration (but auto-generated from your models!)
dotnet ef database update                # ≈ php artisan migrate
```
**Big difference:** you don't hand-write migrations. You change your C# classes, and EF Core
*generates* the migration by diffing against the last one. More like Laravel's schema-from-code
dreams.

## Key differences summary
- **Not Active Record** — query through `DbContext`, not `Model::`.
- **Change tracking + `SaveChanges`** batches writes (Unit of Work).
- **Migrations are generated** from your model classes.
- **Async everywhere** — use the `...Async` methods and `await` (see note 10).
