using DotnetApp.Common;
using HotChocolate;

namespace DotnetApp.GraphQL;

/// <summary>
/// Bridges the domain <see cref="Result"/> to GraphQL. A failed result becomes a
/// <see cref="GraphQLException"/> carrying the error code/message, which Hot
/// Chocolate renders in the response <c>errors</c> array. (Formalized in Step 9.)
/// </summary>
public static class ResultGraphQLExtensions
{
    public static T ValueOrThrow<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return result.Value;

        throw new GraphQLException(ErrorBuilder.New()
            .SetMessage(result.Error!.Message)
            .SetCode(result.Error.Type.ToString().ToUpperInvariant())
            .Build());
    }

    public static void EnsureSuccess(this Result result)
    {
        if (result.IsSuccess)
            return;

        throw new GraphQLException(ErrorBuilder.New()
            .SetMessage(result.Error!.Message)
            .SetCode(result.Error.Type.ToString().ToUpperInvariant())
            .Build());
    }
}
