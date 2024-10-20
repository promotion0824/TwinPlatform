using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RulesEngine.Processor;

/// <summary>
/// A custom Serilog formatter for the LogEvent data column. Primarily to remove properties to minimise payload, e.g. "Renderings" and "Scope", etc
/// </summary>
public class CustomLogEventFormatter : ITextFormatter
{
	private static readonly JsonValueFormatter ValueFormatter = new JsonValueFormatter(typeTagName: null);
	private const string COMMA_DELIMITER = ",";

	private const string TIMESTAMP_FIELD = "Timestamp";
	private const string LEVEL_FIELD = "Level";
	private const string MESSAGE_FIELD = "Message";
	private const string EXCEPTION_FIELD = "Exception";

	/// <summary>
	/// Format
	/// </summary>
	public void Format(LogEvent logEvent, TextWriter output)
	{
		if (logEvent == null)
		{
			throw new ArgumentNullException(nameof(logEvent));
		}

		if (output == null)
		{
			throw new ArgumentNullException(nameof(output));
		}

		output.Write("{");

		var precedingDelimiter = string.Empty;

		//not really necessary, but commented in case
		//Write(TIMESTAMP_FIELD, logEvent.Timestamp.ToUniversalTime().DateTime.ToString("o"));    // Convert to UTC
		//Write(LEVEL_FIELD, logEvent.Level.ToString());
		//Write(MESSAGE_FIELD, logEvent.RenderMessage());
		//if (logEvent.Exception != null)
		//{
		//	Write(EXCEPTION_FIELD, logEvent.Exception.ToString());
		//}

		if (logEvent.Properties.Any())
		{
			output.Write(precedingDelimiter);
			WriteProperties(logEvent.Properties, output);
			precedingDelimiter = COMMA_DELIMITER;
		}

		output.Write("}");
	}

	private static string[] ignoredProperties = new string[]
	{
		"scope",
		"correlationid",
		"progressid"
	};

	private static void WriteProperties(IReadOnlyDictionary<string, LogEventPropertyValue> properties, TextWriter output)
	{
		//output.Write("\"Properties\":{");

		var precedingDelimiter = string.Empty;
		foreach (var property in properties)
		{
			if (ignoredProperties.Any(v => string.Equals(property.Key, v, StringComparison.OrdinalIgnoreCase)))
			{
				continue;
			}

			output.Write(precedingDelimiter);
			precedingDelimiter = COMMA_DELIMITER;
			JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
			output.Write(':');
			ValueFormatter.Format(property.Value, output);
		}

		//output.Write('}');
	}
}
