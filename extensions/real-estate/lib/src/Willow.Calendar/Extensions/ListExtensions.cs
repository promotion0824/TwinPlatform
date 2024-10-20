using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Calendar
{
    internal static class ListExtensions
    {
        internal static bool Empty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }
    }
}
