using DotnetApp.GraphQL;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Types;

namespace DotnetApp.Features.Categories;

/// <summary>Adds the category read field to the root <see cref="Query"/>.</summary>
[ExtendObjectType<Query>]
public sealed class CategoryQueries
{
    [Authorize]
    public Task<List<CategoryDto>> GetMyCategoriesAsync(
        ICategoryService categories, CancellationToken ct) =>
        categories.GetMineAsync(ct);
}

/// <summary>Adds the category write fields to the root <see cref="Mutation"/>.</summary>
[ExtendObjectType<Mutation>]
public sealed class CategoryMutations
{
    [Authorize]
    public async Task<CategoryDto> CreateCategoryAsync(
        CreateCategoryInput input, ICategoryService categories, CancellationToken ct) =>
        (await categories.CreateAsync(input, ct)).ValueOrThrow();

    [Authorize]
    public async Task<CategoryDto> UpdateCategoryAsync(
        UpdateCategoryInput input, ICategoryService categories, CancellationToken ct) =>
        (await categories.UpdateAsync(input, ct)).ValueOrThrow();

    [Authorize]
    public async Task<bool> DeleteCategoryAsync(
        Guid id, ICategoryService categories, CancellationToken ct)
    {
        (await categories.DeleteAsync(id, ct)).EnsureSuccess();
        return true;
    }
}
