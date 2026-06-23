# 11 — Testing (vs PHPUnit / Pest)

## Short version
The common .NET test framework is **xUnit** (alternatives: NUnit, MSTest). Tests live in a
**separate project**, not alongside source. `dotnet test` runs them (≈ `php artisan test`).

## Create a test project
```bash
dotnet new xunit -n DotnetApp.Tests -o tests/DotnetApp.Tests
dotnet sln add tests/DotnetApp.Tests/DotnetApp.Tests.csproj
dotnet add tests/DotnetApp.Tests reference src/DotnetApp/DotnetApp.csproj
```

## A test ≈ a PHPUnit test
```php
// PHPUnit
public function test_adds_numbers() {
    $this->assertEquals(4, add(2, 2));
}
```
```csharp
// xUnit
public class CalculatorTests
{
    [Fact]                          // [Fact] = a single test (≈ a test_ method)
    public void Adds_Numbers()
    {
        Assert.Equal(4, Calculator.Add(2, 2));
    }

    [Theory]                        // [Theory] = data-driven (≈ @dataProvider)
    [InlineData(2, 2, 4)]
    [InlineData(3, 5, 8)]
    public void Adds_Cases(int a, int b, int expected)
        => Assert.Equal(expected, Calculator.Add(a, b));
}
```

## Mapping

| PHPUnit / Pest | xUnit |
|---|---|
| `test_*` method / `it(...)` | `[Fact]` method |
| `@dataProvider` | `[Theory]` + `[InlineData]` |
| `$this->assertEquals` | `Assert.Equal` |
| `$this->assertTrue` | `Assert.True` |
| `setUp()` | constructor of the test class |
| `tearDown()` | implement `IDisposable.Dispose()` |
| Mockery | **Moq** or **NSubstitute** (NuGet packages) |

## Integration tests (HTTP endpoints)
Laravel: `$this->getJson('/api/users')`. .NET: `WebApplicationFactory<Program>` spins up the
app in-memory and gives you an `HttpClient`:
```csharp
var client = factory.CreateClient();
var res = await client.GetAsync("/weatherforecast");
res.EnsureSuccessStatusCode();
```
Add `Microsoft.AspNetCore.Mvc.Testing` for this.

## Key differences
- Tests are a **separate project** referencing the app (not a `tests/` folder inside it).
- **Assert is `Assert.Equal(expected, actual)`** — expected comes **first** (opposite of some
  PHPUnit habits, same order though — just remember expected-first).
- Mocking needs a library (Moq/NSubstitute) — no built-in like Laravel's helpers.

## Run
```bash
dotnet test
```
