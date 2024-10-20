using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Dynamic;
using System.Linq;

namespace PlatformPortalXL
{
    public static class StringExtensions
    {
        private readonly static char[] Delimiters = { ',', ':' };

        public static string CleanEmail(this string email)
        {
            return email == null ? null : string.Concat(email.Where(c => !char.IsWhiteSpace(c)));
        }

        public static string[] SplitModulePaths(this string moduleTypeNamePath)
        {
            return moduleTypeNamePath?.Split(Delimiters);
        }

        public static ExpandoObject ToCamelCaseExpandoObject(this string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new ExpandoObject();
            }

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var converter = new ExpandoObjectConverter();
            var a = JsonConvert.DeserializeObject<ExpandoObject>(json, converter);
            var b = JsonConvert.SerializeObject(a, settings);
            var c = JsonConvert.DeserializeObject<ExpandoObject>(b); // ensures both System.Text.Json and Newtonsoft.Json will successfully serialize it 

            return c;
        }
    }
}
