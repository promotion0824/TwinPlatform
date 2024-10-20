namespace Willow.DataAccess.CosmosDb;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
public record PagedItems<T>(IReadOnlyCollection<T> Items, string? ContinuationToken);
