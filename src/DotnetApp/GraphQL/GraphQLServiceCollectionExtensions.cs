namespace DotnetApp.GraphQL;

/// <summary>
/// Registers the Hot Chocolate GraphQL server. Authorization integrates with the
/// existing ASP.NET Core auth (Step 4), so `[Authorize]` resolvers reuse the same JWT.
/// </summary>
public static class GraphQLServiceCollectionExtensions
{
    public static IServiceCollection AddGraphQLApi(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddAuthorization()       // wires [Authorize] on resolvers to ASP.NET Core auth
            .AddQueryType<Query>();   // root read type (Mutation added in Step 7)

        return services;
    }
}
