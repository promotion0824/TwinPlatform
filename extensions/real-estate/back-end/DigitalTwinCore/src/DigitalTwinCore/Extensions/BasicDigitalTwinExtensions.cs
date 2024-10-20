using System;
using System.Text.Json;
using Azure.DigitalTwins.Core;

namespace DigitalTwinCore.Extensions
{
	public static class BasicDigitalTwinExtensions
	{
		public static BasicDigitalTwin GetValue(this BasicDigitalTwin twin, string alias)
		{
			return JsonSerializer.Deserialize<BasicDigitalTwin>(twin.Contents[alias].ToString());
		}

		public static Guid GetGuidValue(this BasicDigitalTwin twin, string alias)
		{
			return Guid.Parse(twin.Contents[alias].ToString());
		}
	}
}
