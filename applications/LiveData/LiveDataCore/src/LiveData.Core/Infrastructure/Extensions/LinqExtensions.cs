namespace Willow.LiveData.Core.Infrastructure.Extensions;

using System.Linq.Expressions;

internal static class LinqExtensions
{
    public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> whereClause)
    {
        if (condition)
        {
            return query.Where(whereClause);
        }

        return query;
    }

    public static List<string> GetInvalidGuids<T>(this List<T> guids, bool excludeEmpty = false)
    {
        var invalidGuids = new List<string>();

        if (guids != null)
        {
            foreach (var item in guids.Distinct())
            {
                if (!Guid.TryParse(item.ToString(), out Guid result))
                {
                    invalidGuids.Add(item.ToString());
                }
            }
        }

        return excludeEmpty ? invalidGuids.Where(x => x.Length > 0).ToList() : invalidGuids;
    }

    public static List<Guid> GetValidGuids(this List<string> guidStrings)
    {
        var validGuids = new List<Guid>();

        if (guidStrings != null)
        {
            foreach (var item in guidStrings.Distinct())
            {
                if (Guid.TryParse(item, out Guid result))
                {
                    validGuids.Add(result);
                }
            }
        }

        return validGuids;
    }

    public static string GetErrorMessageOnInvalidGuids<T>(this List<T> guids, string entityName)
    {
        var invalidGuids = GetInvalidGuids(guids);
        return invalidGuids.Count > 0 ?
                            GetGuidErrorMessage(invalidGuids, entityName)
                            : string.Empty;
    }

    private static string GetGuidErrorMessage(List<string> validGuids, string entityName)
    {
        string errorMessage = $"The following {entityName} are not valid GUIDs";
        string emptyErrorMessage = $"The rest of the {entityName} are empty";
        return !validGuids.Any(x => x.Length > 0) ? emptyErrorMessage : errorMessage;
    }
}
