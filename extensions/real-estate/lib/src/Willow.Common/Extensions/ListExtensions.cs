using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Common
{
    public static class ListExtensions
    {
        public static bool Empty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }   
    }
}
