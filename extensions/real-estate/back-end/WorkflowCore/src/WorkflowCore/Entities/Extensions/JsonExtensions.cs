using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;

namespace WorkflowCore.Entities.Extensions
{
    public static class JsonExtensions
    {
        public static string JsonValue(string column, [NotParameterized] string path)
        {
            var json = JObject.Parse(column);
            var selectedToken = json.SelectToken(path);
            return selectedToken.ToString();
        }
    }
}
