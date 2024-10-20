using System;
using System.Globalization;
using System.Linq;

namespace DirectoryCore.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        private static readonly CultureInfo[] Cultures = CultureInfo.GetCultures(
            CultureTypes.AllCultures
        );

        public static bool IsCultureCode(this string code)
        {
            return string.IsNullOrEmpty(code)
                || Cultures.Any(
                    c => c.Name.Equals(code, StringComparison.InvariantCultureIgnoreCase)
                );
        }
    }
}
