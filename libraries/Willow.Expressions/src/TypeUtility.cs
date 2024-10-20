using System;
using System.Linq;

namespace Willow.Units.Utility
{
    /// <summary>
    /// Utility class for working with Types
    /// </summary>
    public static class TypeUtility
    {
        /// <summary>
        /// Like .GetType().Name but gives a better string for generics
        /// </summary>
        public static string CreateTypeStringFromObject(object value)
        {
            return CreateTypeStringFromType(value?.GetType());
        }

        /// <summary>
        /// Like Type.Name but gives a better string for generics
        /// </summary>
        public static string CreateTypeStringFromType(Type? type)
        {
            if (type is null) return "NULL TYPE";
            string typeString =
                (type.IsGenericType == true)
                    ? $"{type.Name.Replace("`1", "").Replace("`2", "").Replace("`3", "")}<{string.Join(",", type.GetGenericArguments().Select(a => CreateTypeStringFromType(a)))}>"
                    : type.Name;
            return typeString;
        }
    }
}
