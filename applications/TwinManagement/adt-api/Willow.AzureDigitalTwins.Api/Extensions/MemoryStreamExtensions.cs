using System.IO;

namespace Willow.AzureDigitalTwins.Api.Extensions
{
	public static class MemoryStreamExtensions
	{
		public static MemoryStream FromString(this MemoryStream stream, string s)
		{
			var writer = new StreamWriter(stream);
			writer.Write(s);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		public static string ConvertString(this Stream stream)
		{
			using StreamReader reader = new StreamReader(stream);
			return reader.ReadToEnd();
		}
	}
}
