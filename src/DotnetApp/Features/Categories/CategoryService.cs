using DotnetApp.Common;
using DotnetApp.Common.Errors;
using DotnetApp.Domain.Entities;
using DotnetApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DotnetApp.Features.Categories;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetMineAsync(CancellationToken ct);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryInput input, CancellationToken ct);
    Task<Result<CategoryDto>> UpdateAsync(UpdateCategoryInput input, CancellationToken ct);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct);
}

public sealed class CategoryService(AppDbContext db, ICurrentUser currentUser) : ICategoryService
{
    public async Task<List<CategoryDto>> GetMineAsync(CancellationToken ct) =>
        await db.Categories
            .Where(c => c.UserId == currentUser.Id)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.CreatedAt))
            .ToListAsync(ct);

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryInput input, CancellationToken ct)
    {
        var name = input.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<CategoryDto>(Error.Validation("Name is required."));

        var exists = await db.Categories
            .AnyAsync(c => c.UserId == currentUser.Id && c.Name == name, ct);
        if (exists)
            return Result.Failure<CategoryDto>(Error.Conflict($"A category named '{name}' already exists."));

        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.Id,
            Name = name
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return new CategoryDto(category.Id, category.Name, category.CreatedAt);
    }

    public async Task<Result<CategoryDto>> UpdateAsync(UpdateCategoryInput input, CancellationToken ct)
    {
        var name = input.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<CategoryDto>(Error.Validation("Name is required."));

        // Scoped load: not found *for this user* → NotFound (don't leak others' rows).
        var category = await db.Categories
            .SingleOrDefaultAsync(c => c.Id == input.Id && c.UserId == currentUser.Id, ct);
        if (category is null)
            return Result.Failure<CategoryDto>(Error.NotFound("Category not found."));

        var duplicate = await db.Categories.AnyAsync(
            c => c.UserId == currentUser.Id && c.Name == name && c.Id != input.Id, ct);
        if (duplicate)
            return Result.Failure<CategoryDto>(Error.Conflict($"A category named '{name}' already exists."));

        category.Name = name;
        await db.SaveChangesAsync(ct);

        return new CategoryDto(category.Id, category.Name, category.CreatedAt);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct)
    {
        var category = await db.Categories
            .SingleOrDefaultAsync(c => c.Id == id && c.UserId == currentUser.Id, ct);
        if (category is null)
            return Result.Failure(Error.NotFound("Category not found."));

        db.Categories.Remove(category);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
