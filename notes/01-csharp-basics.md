# 01 — C# Basics (vs PHP)

## Short version
C# is **statically typed and compiled**. You declare types; the compiler checks them before
the app runs. PHP figures types out at runtime; C# refuses to build if they don't line up.

## Laravel/PHP analogy
You know `$user = new User()`. C# is similar but every variable has a known type.

```php
// PHP
$name = "Kaung";
$age = 30;
function greet(string $name): string {
    return "Hi $name";
}
```

```csharp
// C#
string name = "Kaung";
int age = 30;
var city = "Yangon";   // 'var' = "infer the type for me" (still static — it's string)

string Greet(string name) => $"Hi {name}";   // $"..." is string interpolation (like "Hi $name")
```

## Key differences

| PHP | C# |
|---|---|
| `$variable` (dollar sign) | `variable` (no sigil) |
| Dynamic types | Static types (checked at compile) |
| `"Hi $name"` | `$"Hi {name}"` (note the leading `$`) |
| `null` anywhere | **Nullable reference types** — `string?` can be null, `string` should not |
| Arrays do everything | Distinct types: `List<T>`, arrays `[]`, `Dictionary<K,V>` |
| `array_map`, `array_filter` | **LINQ**: `.Select()`, `.Where()` |

## Classes & properties
PHP has `public $name;`. C# has **properties** with `get`/`set`:

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";   // default value
}
```

## Records — best for DTOs / API responses
A `record` is an immutable class with value equality — perfect for data you send as JSON.
Laravel doesn't have a direct equivalent (closest: a readonly DTO / value object).

```csharp
public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary);
```

You already have one of these in `src/DotnetApp/Program.cs`.

## LINQ ≈ Laravel Collections
This will feel very familiar — it's the most "Laravel-like" part of C#.

```php
// Laravel collection
$adults = collect($users)->filter(fn($u) => $u->age >= 18)->map(fn($u) => $u->name);
```

```csharp
// C# LINQ
var adults = users.Where(u => u.Age >= 18).Select(u => u.Name);
```

## Try it
Open `Program.cs` and read the `WeatherForecast` record and the LINQ in the `/weatherforecast`
endpoint — you now understand every line.
