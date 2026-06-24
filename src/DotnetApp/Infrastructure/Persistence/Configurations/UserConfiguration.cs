using DotnetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetApp.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for <see cref="User"/>. Constraints only — table/column casing
/// is handled globally by the snake_case naming convention in <c>AppDbContext</c>.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();

        // Unique email enforced at the database level — the source of truth for
        // "no duplicate accounts", independent of any app-side check.
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        // DB stamps the row on insert; the app never has to set it.
        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("now()");
    }
}
