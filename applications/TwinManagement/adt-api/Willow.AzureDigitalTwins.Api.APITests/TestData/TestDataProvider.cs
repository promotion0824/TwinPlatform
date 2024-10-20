
using System;
using System.IO;
using System.Text.Json;
using Azure.DigitalTwins.Core;

namespace Willow.AzureDigitalTwins.Api.APITests.TestData
{
	public static class TestDataProvider
	{

		private static readonly string SampleBaseDirectory = Path.Combine( Directory.GetCurrentDirectory(),"TestData","Sample");

		private static JsonDocumentOptions documentOptions => new JsonDocumentOptions() { };

		public static JsonDocument GetSampleModel()
		{
			string fileContent = GetFileContentAsString("SampleModel.json");
			return JsonDocument.Parse(fileContent,documentOptions);
		}

		public static BasicDigitalTwin GetSampleTwinOne()
		{
			string fileContent = GetFileContentAsString("SampleTwin1.json");
			return JsonSerializer.Deserialize<BasicDigitalTwin>(fileContent) ?? throw new ArgumentNullException();
		}

		public static BasicDigitalTwin GetSampleTwinTwo()
		{
			string fileContent = GetFileContentAsString("SampleTwin2.json");
			return JsonSerializer.Deserialize<BasicDigitalTwin>(fileContent) ?? throw new ArgumentNullException();
		}

		public static BasicRelationship GetSampleRelationshipOne()
		{
			string fileContent = GetFileContentAsString("SampleRelationship1.json");
			return JsonSerializer.Deserialize<BasicRelationship>(fileContent) ?? throw new ArgumentNullException();
		}

		public static BasicRelationship GetSampleRelationshipTwo()
		{
			string fileContent = GetFileContentAsString("SampleRelationship2.json");
			return JsonSerializer.Deserialize<BasicRelationship>(fileContent) ?? throw new ArgumentNullException();
		}

		private static string GetFileContentAsString(string fileName)
		{
			fileName = SampleBaseDirectory + "\\" + fileName ;
			return File.ReadAllText(fileName);
		}
	}
}
