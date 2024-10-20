using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WillowRules.Extensions;

/// <summary>
/// Extension methods for use with strings
/// </summary>
public static class StringExtensions
{
	/// <summary>
	/// Limits a string to a defined length adding ellipses if necessary
	/// </summary>
	public static string LimitWithEllipses(this string input, int length)
	{
		if (input is null) return "";
		if (input.Length <= length) return input;
		if (length < 3) return input.Substring(0, length);
		return input.Substring(0, length - 3) + new string('.', 3);
	}

	/// <summary>
	/// Trim Model Id to name only, e.g. dtmi:com:willowinc:TerminalUnit;1 becomes TerminalUnit
	/// </summary>
	public static string TrimModelId(this string input)
	{
		if (!string.IsNullOrEmpty(input))
		{
			var firstIndex = input.LastIndexOf(':') + 1;
			var lastIndex = input.IndexOf(';');

			if (firstIndex > 0 && lastIndex > 0 && lastIndex > firstIndex)
			{
				return input.Substring(firstIndex, lastIndex - firstIndex);
			}
		}

		return input;
	}

	/// <summary>
	/// Extracts  placeholders for potentially string replacements
	/// </summary>
	public static string[] ExtractExpressionsFromText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			//skip out early if not needed
			return Array.Empty<string>();
		}

		return Regex.Matches(text, "\\{(.*?)\\}").Select(v => v.Value.TrimStart('{').TrimEnd('}')).ToArray();
	}

	/// <summary>
	/// Replaces a placeholder in a string with a new values, e.g. {result}
	/// </summary>
	public static string ReplaceExpressionsFromText(string text, string expression, string newExpression)
	{
		return text.Replace($"{{{expression}}}", newExpression);
	}

	/// <summary>
	/// Get the localized string, fallback to "en" and then to the simple string
	/// </summary>
	public static string GetLocalLanguage(IReadOnlyDictionary<string, string> localized,
		string normal, string language)
	{
		//english should use the normal value
		if (string.IsNullOrEmpty(language) || string.Equals(language, "en", StringComparison.OrdinalIgnoreCase)) return normal;

		//should be any other language
		if (localized.TryGetValue(language, out var value) && !string.IsNullOrEmpty(value)) return value;

		return normal;
	}
}
