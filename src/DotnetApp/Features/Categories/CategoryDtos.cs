namespace DotnetApp.Features.Categories;

/// <summary>GraphQL projection of a category.</summary>
public sealed record CategoryDto(Guid Id, string Name, DateTime CreatedAt);

/// <summary>Input for <c>createCategory</c>.</summary>
public sealed record CreateCategoryInput(string Name);

/// <summary>Input for <c>updateCategory</c>.</summary>
public sealed record UpdateCategoryInput(Guid Id, string Name);
