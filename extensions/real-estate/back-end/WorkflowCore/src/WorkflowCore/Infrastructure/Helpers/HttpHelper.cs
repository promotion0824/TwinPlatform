using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkflowCore.Infrastructure.Helpers;

public class HttpHelper
{
	public static string ToQueryString(object parameters)
	{
		var fields = new List<(string key, object value, bool isEnumerable)>();

		parameters.GetType().GetProperties()
			.ToList()
			.ForEach(pi =>
			{
				var key = pi.Name;
				var value = pi.GetValue(parameters, null);
				var isEnumerable = pi.PropertyType.IsArray ||
				                   pi.PropertyType.IsAssignableTo(typeof(Enumerable)) ||
				                   pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>);

				if (value != null)
				{
					fields.Add((key, value, isEnumerable));
				}
			});
		var array = fields.SelectMany(field =>
		{
			var (key, value, isEnumerable) = field;
			if (isEnumerable)
			{
				return ((IEnumerable)value).Cast<object>()
					.Where(x => !string.IsNullOrWhiteSpace(x.ToString()))
					.Select(x => ToEncodedKeyValue(key, x));
			}
			return !string.IsNullOrWhiteSpace(value.ToString()) ? new[] { ToEncodedKeyValue(key, value) } : System.Array.Empty<string>();
		}).ToArray();
		return string.Join("&", array);
	}
	private static string ToEncodedKeyValue(object key, object value)
	{
		return $"{HttpUtility.UrlEncode(key.ToString())}={HttpUtility.UrlEncode(value.ToString())}";
	}
}
