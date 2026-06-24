using Microsoft.EntityFrameworkCore;

namespace DotnetApp.Infrastructure.Persistence;

/// <summary>
/// DI registration for the persistence layer. Keeps EF Core wiring out of
/// <c>Program.cs</c> — the composition root just calls <see cref="AddPersistence"/>.
/// </summary>
public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        // Scoped lifetime (one DbContext per request) is applied by AddDbContext.
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default"))
                   .UseSnakeCaseNamingConvention());

        return services;
    }
}
