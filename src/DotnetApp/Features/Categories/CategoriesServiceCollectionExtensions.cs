namespace DotnetApp.Features.Categories;

public static class CategoriesServiceCollectionExtensions
{
    public static IServiceCollection AddCategories(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        return services;
    }
}
