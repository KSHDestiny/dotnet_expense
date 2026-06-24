using DotnetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetApp.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("now()");

        // A user can't have two categories with the same name; different users can.
        builder.HasIndex(c => new { c.UserId, c.Name })
            .IsUnique();

        // FK to the owning user. Deleting a user removes their categories.
        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
