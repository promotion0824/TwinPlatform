using CsvHelper;
using CsvHelper.Configuration;
using Kusto.Cloud.Platform.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using Willow.Rules.Cache;
using Willow.Rules.Model;

namespace WillowExpressions.Test
{
	public static class DataHelper
	{
		private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
		{
			Converters = new List<JsonConverter> { new TokenExpressionJsonConverter() },
			Formatting = Formatting.Indented,
			NullValueHandling = NullValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.Auto
		};

		public static bool TryReadRulesFromZip(string filePath, out List<Rule> rules)
		{
			rules = new List<Rule>();

			try
			{
				using (var zipFile = ZipFile.OpenRead(filePath))
				{
					foreach (var entry in zipFile.Entries)
					{
						using (var stream = entry.Open())
						{
							if (TryReadRuleFromStream(stream, out var rule))
							{
								rules.Add(rule);
							}
						}
					}
				}
			}
			catch (Exception)
			{
				rules.Clear();
				return false;
			}

			return rules.Count > 0;
		}

		private static bool TryReadRuleFromStream(Stream stream, out Rule rule)
		{
			rule = null!;

			using (var reader = new StreamReader(stream))
			using (var jsonReader = new JsonTextReader(reader))
			{
				try
				{
					var serializer = JsonSerializer.Create(jsonSettings);
					rule = serializer.Deserialize<Rule>(jsonReader)!;
				}
				catch (Exception)
				{
					//Do nothing and process what is possible
				}

			}

			return rule != null;
		}

		public static void WritePointExpressionsToFile(string filePath, IList<Rule> rules)
		{
			//Get current expressions
			TryReadPointExpressions(filePath, out var pointExpressions);

			foreach (var rule in rules)
			{
				foreach (RuleParameter parameter in rule.Parameters)
				{
					pointExpressions.Add(parameter.PointExpression);
				}
			}

			//Now distinct to remove duplicates
			var distinctPointExpressions = pointExpressions.Distinct().Order();

			File.WriteAllLines(filePath, distinctPointExpressions);
		}

		public static bool TryReadPointExpressions(string filePath, out IList<string> pointExpressions)
		{
			pointExpressions = new List<string>();

			try
			{
				pointExpressions = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
			}
			catch (Exception)
			{
				pointExpressions.Clear();
				return false;
			}

			return pointExpressions.Count > 0;
		}
	}
}
