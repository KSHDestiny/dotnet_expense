using DotnetApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotnetApp.Infrastructure.Persistence.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(e => e.Id);

        // Money: numeric(18,2). decimal in C#; precision/scale fixed at the DB.
        builder.Property(e => e.Amount)
            .HasColumnType("numeric(18,2)");

        builder.Property(e => e.Note)
            .HasMaxLength(280);

        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("now()");

        // Common query path: a user's expenses, often filtered by date.
        builder.HasIndex(e => new { e.UserId, e.SpentAt });

        // Deleting a user removes their expenses.
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // RESTRICT: a category with expenses cannot be deleted (data integrity).
        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
