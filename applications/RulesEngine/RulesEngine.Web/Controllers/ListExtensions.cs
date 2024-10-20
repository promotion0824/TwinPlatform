using System;
using System.Collections.Generic;

namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Extension methods for lists
/// </summary>
public static class ListExtensions
{
    /// <summary>
    /// Split a list - items before a string and items after
    /// </summary>
    public static (int before, int after, List<T> beforeInstances, List<T> afterInstances) Split<T>
        (this IEnumerable<T> values, string split, Func<T, string, bool> equalityComparer)
    {
        List<T> beforeInstances = new();
        List<T> afterInstances = new();

        int state = split == "start" ? 0 : split == "empty" ? 0 : -1;

        foreach (var item in values)
        {
            if (equalityComparer(item, split)) state = 0;  // should fire just once!
            else if (state == 0) state = 1;

            switch (state)
            {
                case 0: continue;
                case -1: beforeInstances.Add(item); continue;
                case 1: afterInstances.Add(item); continue;
                default: continue;
            }
        }

        return (beforeInstances.Count, afterInstances.Count, beforeInstances, afterInstances);
    }
}
