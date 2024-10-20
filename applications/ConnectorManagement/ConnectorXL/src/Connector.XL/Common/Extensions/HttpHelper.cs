namespace Connector.XL.Common.Extensions;

using System.Collections.Specialized;
using System.Linq;
using System.Web;

internal static class HttpHelper
{
    public static string ToQueryString(object parameters, bool urlEncodeValue = true, bool urlEncodeKey = false)
    {
        var fields = new NameValueCollection();

        parameters.GetType()
            .GetProperties()
            .ToList()
            .ForEach(pi => fields.Add(pi.Name, pi.GetValue(parameters, null)?.ToString() ?? string.Empty));
        var array = (from key in fields.AllKeys
                     from value in fields.GetValues(key)
                     where !string.IsNullOrEmpty(value)
                     select string.Format("{0}={1}", urlEncodeKey ? HttpUtility.UrlEncode(key) : key, urlEncodeValue ? HttpUtility.UrlEncode(value) : value)).ToArray();
        return string.Join("&", array);
    }
}
