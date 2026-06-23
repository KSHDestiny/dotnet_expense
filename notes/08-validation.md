# 08 — Validation (vs Form Requests)

## Short version
.NET validates with **DataAnnotations** (attributes on your DTO properties) and/or
**FluentValidation** (a separate package, closest to Laravel's Form Requests).

## DataAnnotations ≈ inline rules
```php
// Laravel controller validation
$request->validate([
    'name'  => 'required|max:100',
    'email' => 'required|email',
    'age'   => 'integer|min:18',
]);
```
```csharp
// .NET — attributes on the DTO
public record CreateUser(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress]   string Email,
    [Range(18, 150)]           int Age
);
```
With controllers + `[ApiController]`, invalid requests are **auto-rejected with a 400** and a
problem-details JSON body — you don't write the check yourself (like Form Request auto-422).

## FluentValidation ≈ Form Request
For complex rules, install FluentValidation — it reads almost like a Laravel Form Request:
```bash
dotnet add src/DotnetApp package FluentValidation.AspNetCore
```
```csharp
public class CreateUserValidator : AbstractValidator<CreateUser>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).GreaterThanOrEqualTo(18);
    }
}
```
That class ≈ `app/Http/Requests/CreateUserRequest.php`'s `rules()`.

## Key differences

| Laravel | .NET |
|---|---|
| `$request->validate([...])` | `[ApiController]` auto-validates DataAnnotations |
| Form Request class | FluentValidation `AbstractValidator<T>` |
| Returns 422 with errors | Returns 400 `ValidationProblemDetails` by default |
| Rules as strings (`'required\|email'`) | Strongly-typed attributes / methods (compiler-checked) |

## Recommendation
- Simple DTOs → **DataAnnotations** (zero setup, it's built in).
- Anything conditional or cross-field → **FluentValidation** (it'll feel like home).
