namespace DotnetApp.GraphQL;

/// <summary>
/// Root GraphQL mutation type. Intentionally empty — features contribute their
/// mutations via <c>[ExtendObjectType&lt;Mutation&gt;]</c> type extensions, keeping
/// this class free of per-feature edits.
/// </summary>
public sealed class Mutation;
