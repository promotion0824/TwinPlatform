using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Azure.DigitalTwins.Core;
using DTDLParser;
using DTDLParser.Models;
using Willow.Exceptions.Exceptions;
using Willow.Model.Adt;
using Willow.TwinLifecycleManagement.Web.Helpers.Converters;
using Willow.TwinLifecycleManagement.Web.UnitTests.TestExtensions;
using Xunit;
using static Willow.TwinLifecycleManagement.Web.Helpers.ImporterConstants;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Tools.Converter
{
	public class CsvFileConverterTests
	{
		private Assembly _assembly;
		private string _resourceName;
		private string _resourceNameWithSiteId;
		private string _siteId;
		private IReadOnlyDictionary<Dtmi, DTEntityInfo> _modelsData;

		public CsvFileConverterTests()
		{
			_assembly = Assembly.GetExecutingAssembly();
			_resourceName = "csvTestData.csv";
			_resourceNameWithSiteId = "csvTestDataWithSiteId.csv";
			_siteId = "testSiteId";
		}

		[Fact]
		public void GetParsedTwinsWithRelationships_ShouldReturnValidResponse()
		{
			BasicDigitalTwinWithRelationships testTwins;
			string testTwinsSiteId;

			using (Stream stream = TestDataFactory.GetStream(_resourceName))
			{
				_modelsData = TestDataFactory.GetModelsData();
				CsvFileConverter _csvFileConverter = new CsvFileConverter(stream, _modelsData, _siteId);
				testTwins = _csvFileConverter.GetParsedTwinsWithRelationships();
				testTwinsSiteId = (string)testTwins.Twins.First().Contents[Columns.SiteIdColumn];
			}

			Assert.NotNull(testTwins);
			Assert.True(testTwins.Twins.Count == 1);
			Assert.True(testTwins.Relationships.Count == 1);
			Assert.Null(testTwins.Twin);
			Assert.True(testTwins.Twins.First().Contents.Count > 0);
			Assert.True(testTwinsSiteId == _siteId);
			Assert.True(testTwins.Twins.First().Id == "AXA-STO-GFR_A_RCB_001");
			Assert.True(testTwins.Twins.First().Metadata.ModelId == "dtmi:com:willowinc:Chiller;1");
			Assert.True(testTwins.Relationships.First().SourceId == "AXA-STO-GFR_A_RCB_001");
			Assert.True(testTwins.Relationships.First().TargetId == "CARRIER");
			Assert.True(testTwins.Relationships.First().Name == "manufacturedBy");
		}

		[Fact]
		public void GetParsedTwins_ShouldReturnValidResponse()
		{
			IEnumerable<BasicDigitalTwin> testTwins;
			string testTwinsSiteId;

			using (Stream stream = TestDataFactory.GetStream(_resourceName))
			{
				_modelsData = TestDataFactory.GetModelsData();
				CsvFileConverter _csvFileConverter = new CsvFileConverter(stream, _modelsData, _siteId);
				testTwins = _csvFileConverter.GetParsedTwins();
				testTwinsSiteId = (string)testTwins.First().Contents[Columns.SiteIdColumn];
			}

			Assert.NotNull(testTwins);
			Assert.True(testTwins.Count() == 1);
			Assert.True(testTwins.First().Contents.Count > 0);
			Assert.True(testTwinsSiteId == _siteId);
			Assert.True(testTwins.First().Id == "AXA-STO-GFR_A_RCB_001");
			Assert.True(testTwins.First().Metadata.ModelId == "dtmi:com:willowinc:Chiller;1");
		}

		[Fact]
		public void GetParsedTwinsIds_ShouldReturnValidResponse()
		{
			IEnumerable<string> testTwins;

			using (Stream stream = TestDataFactory.GetStream(_resourceName))
			{
				CsvFileConverter _excelFileConverter = new CsvFileConverter(stream);
				testTwins = _excelFileConverter.GetTwinIds();
			}

			Assert.NotNull(testTwins);
			Assert.True(testTwins.Count() == 1);
			Assert.True(testTwins.First() == "AXA-STO-GFR_A_RCB_001");
		}

		[Fact]
		public void GetParsedRelationships_ShouldReturnValidResponse()
		{
			IEnumerable<BasicRelationship> testRelationships;

			using (Stream stream = TestDataFactory.GetStream(_resourceName))
			{
				_modelsData = TestDataFactory.GetModelsData();
				CsvFileConverter _csvFileConverter = new CsvFileConverter(stream, _modelsData, _siteId);
				testRelationships = _csvFileConverter.GetParsedRelationships();
			}

			Assert.True(testRelationships.Count() == 1);
			Assert.True(testRelationships.First().SourceId == "AXA-STO-GFR_A_RCB_001");
			Assert.True(testRelationships.First().TargetId == "CARRIER");
			Assert.True(testRelationships.First().Name == "manufacturedBy");
		}

		[Fact]
		public void GetParsedTwinsWithRelationships_ShouldThrowAnException()
		{
			using (Stream stream = _assembly.GetManifestResourceStream("Wrong test stream"))
			{
				_modelsData = TestDataFactory.GetModelsData();
				CsvFileConverter _csvFileConverter = new CsvFileConverter(stream, _modelsData, _siteId);
				var ex = Assert.Throws<FileContentException>(() => _csvFileConverter.GetParsedTwinsWithRelationships());
				Assert.True(ex.Message == "Unable to parse provided csv file");
			}
		}

		[Fact]
		public void GetParsedTwins_ShouldThrowAnException()
		{
			using (Stream stream = _assembly.GetManifestResourceStream("Wrong test stream"))
			{
				_modelsData = TestDataFactory.GetModelsData();
				CsvFileConverter _csvFileConverter = new CsvFileConverter(stream, _modelsData, _siteId);
				var ex = Assert.Throws<FileContentException>(() => _csvFileConverter.GetParsedTwins());
				Assert.True(ex.Message == "Unable to parse provided csv file");
			}
		}

		[Fact]
		public void GetParsedTwinIds_ShouldThrowAnException()
		{
			using (Stream stream = _assembly.GetManifestResourceStream("Wrong test stream"))
			{
				_modelsData = TestDataFactory.GetModelsData();
				CsvFileConverter _csvFileConverter = new CsvFileConverter(stream);
				var ex = Assert.Throws<FileContentException>(() => _csvFileConverter.GetTwinIds());
				Assert.True(ex.Message == "Unable to parse provided csv file");
			}
		}

		[Fact]
		public void GetParsedRelationships_ShouldThrowAnException()
		{
			using (Stream stream = _assembly.GetManifestResourceStream("Wrong test stream"))
			{
				_modelsData = TestDataFactory.GetModelsData();
				CsvFileConverter _csvFileConverter = new CsvFileConverter(stream, _modelsData, _siteId);
				var ex = Assert.Throws<FileContentException>(() => _csvFileConverter.GetParsedRelationships());
				Assert.True(ex.Message == "Unable to parse provided csv file");
			}
		}

		[Fact]
		public void GetParsedTwinsWithRelationships_ShouldNotOverrideSiteIdFromTheFile()
		{
			BasicDigitalTwinWithRelationships testTwins;

			using (Stream stream = TestDataFactory.GetStream(_resourceNameWithSiteId))
			{
				_modelsData = TestDataFactory.GetModelsData();
				CsvFileConverter _csvFileConverter = new CsvFileConverter(stream, _modelsData, _siteId);
				testTwins = _csvFileConverter.GetParsedTwinsWithRelationships();
			}

			Assert.NotNull(testTwins);
			Assert.True(testTwins.Twins.Count == 1);
			Assert.True(testTwins.Relationships.Count == 1);
			Assert.Null(testTwins.Twin);
			Assert.NotEqual(_siteId, testTwins.Twins.First().Contents[Columns.SiteIdColumn]);
			Assert.Equal("testSiteIdFromTheFile", testTwins.Twins.First().Contents[Columns.SiteIdColumn]);
		}

		[Fact]
		public void GetParsedTwinsWithRelationships_ShouldOverrideSiteIdFromTheFile()
		{
			BasicDigitalTwinWithRelationships testTwins;

			using (Stream stream = TestDataFactory.GetStream(_resourceName))
			{
				_modelsData = TestDataFactory.GetModelsData();
				CsvFileConverter _csvFileConverter = new CsvFileConverter(stream, _modelsData, _siteId);
				testTwins = _csvFileConverter.GetParsedTwinsWithRelationships();
			}

			Assert.NotNull(testTwins);
			Assert.True(testTwins.Twins.Count == 1);
			Assert.True(testTwins.Relationships.Count == 1);
			Assert.Null(testTwins.Twin);
			Assert.Equal(_siteId, testTwins.Twins.First().Contents[Columns.SiteIdColumn]);
			Assert.NotEqual("testSiteIdFromTheFile", testTwins.Twins.First().Contents[Columns.SiteIdColumn]);
		}
	}
}
