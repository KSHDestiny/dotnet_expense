using DotnetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotnetApp.Infrastructure.Persistence;

/// <summary>
/// EF Core unit-of-work / DB session for the application. Registered scoped
/// (one per request). Entity mappings live in <c>Configurations/</c> and are
/// discovered via <see cref="ModelBuilder.ApplyConfigurationsFromAssembly"/>.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
