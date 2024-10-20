using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Willow.Platform.Localization
{
    public static class StringExtensions
    {
        public static string ConvertDiacrits(this string s)
        {
            var sNormalized = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            for(var i = 0; i < sNormalized.Length; ++i) 
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(sNormalized[i]);

                if(uc != UnicodeCategory.NonSpacingMark) 
                {
                    sb.Append(sNormalized[i]);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }      
    }
}
