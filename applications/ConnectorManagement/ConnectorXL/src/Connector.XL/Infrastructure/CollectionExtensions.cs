namespace Connector.XL.Infrastructure;

using System.Collections.Generic;

internal static class CollectionExtensions
{
    public static void AddIfNotNull<T>(this IList<T> list, T item)
        where T : class
    {
        if (item != null)
        {
            list.Add(item);
        }
    }
}
