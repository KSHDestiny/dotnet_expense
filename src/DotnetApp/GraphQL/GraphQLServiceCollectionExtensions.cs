using DotnetApp.Features.Categories;
using HotChocolate.Execution.Configuration;

namespace DotnetApp.GraphQL;

/// <summary>
/// Registers the Hot Chocolate GraphQL server. Authorization integrates with the
/// existing ASP.NET Core auth (Step 4), so `[Authorize]` resolvers reuse the same JWT.
/// Each feature contributes resolvers via [ExtendObjectType] type extensions.
/// </summary>
public static class GraphQLServiceCollectionExtensions
{
    public static IServiceCollection AddGraphQLApi(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddAuthorization()           // wires [Authorize] resolvers to ASP.NET Core auth
            .AddQueryType<Query>()        // root read type
            .AddMutationType<Mutation>()  // root write type
            .AddCategoryTypes();          // feature type extensions

        return services;
    }

    // Feature registration kept beside the feature it serves (see Features/Categories).
    private static IRequestExecutorBuilder AddCategoryTypes(this IRequestExecutorBuilder builder) =>
        builder
            .AddTypeExtension<CategoryQueries>()
            .AddTypeExtension<CategoryMutations>();
}
