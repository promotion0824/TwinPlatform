using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Cache;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using WillowRules.Test.Bugs.Mocks;

namespace WillowRules.Test;

[TestClass]
public class FileServiceTests
{
	private IFileService? fileService;
	private RepositoryRulesMock rulesRepo = new RepositoryRulesMock();
	private RepositoryRuleMetadataMock rulesMetadataRepo = new RepositoryRuleMetadataMock();
	private RepositoryGlobalVariableMock globalsRepo = new RepositoryGlobalVariableMock();
	private RepositoryMLModelMock mlModelRepo = new RepositoryMLModelMock();

	[TestInitialize]
	public void TestSetup()
	{
		rulesRepo = new RepositoryRulesMock();
		rulesMetadataRepo = new RepositoryRuleMetadataMock();
		globalsRepo = new RepositoryGlobalVariableMock();
		mlModelRepo = new RepositoryMLModelMock();

		fileService = new FileService(
			Mock.Of<IMemoryCache>(),
			rulesRepo,
			Mock.Of<IRepositoryInsight>(),
			rulesMetadataRepo,
			Mock.Of<IRepositoryRuleInstances>(),
			Mock.Of<IRepositoryActorState>(),
			Mock.Of<IRepositoryTimeSeriesMapping>(),
			Mock.Of<IRepositoryTimeSeriesBuffer>(),
			Mock.Of<IRepositoryCalculatedPoint>(),
			globalsRepo,
			Mock.Of<IRepositoryCommand>(),
			mlModelRepo,
			new MLService(mlModelRepo, Mock.Of<ILogger<MLService>>()),
			Mock.Of<IDataCacheFactory>(),
			Mock.Of<ITwinSystemService>(),
			Mock.Of<ITwinService>(),
			Mock.Of<IADXService>(),
			MockObjects.WillowEnvironment,
			Mock.Of<ILogger<FileService>>());
	}

	[TestMethod]
	public async Task AddRulesFromZipFile()
	{
		var zipFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "rulesupload.zip");

		if (!System.IO.File.Exists(zipFilePath)) throw new Exception($"Check test setup has zip file at correct location {zipFilePath}");

		var result = await fileService!.UploadRules(zipFilePath, "");

		Assert.AreEqual(3, result.Rules.Count());

		rulesRepo.Data.Count.Should().Be(3);

		globalsRepo.Data.Count.Should().Be(1);

		mlModelRepo.Data.Count.Should().Be(1);
	}

	[TestMethod]
	public async Task AddRulesFromJsonFile()
	{
		var jsonFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "ruleupload.json");

		var result = await fileService!.UploadRules(jsonFilePath, "");

		Assert.AreEqual(1, result.Rules.Count());

		rulesRepo.Data.Count.Should().Be(1);
		rulesMetadataRepo.Data.Count.Should().Be(1);

		rulesMetadataRepo.LastMergeConfig.Should().NotBeNull();

		var config = rulesMetadataRepo.LastMergeConfig!;
		
		config.PropertiesToIncludeOnUpdate.Should().NotContain(nameof(RuleMetadata.CreatedBy));
		config.PropertiesToIncludeOnUpdate.Should().Contain(nameof(RuleMetadata.ModifiedBy));
		config.PropertiesToIncludeOnUpdate.Should().NotContain(nameof(RuleMetadata.Created));
		config.PropertiesToIncludeOnUpdate.Should().Contain(nameof(RuleMetadata.LastModified));
	}

	[TestMethod]
	public async Task ShouldDeleteRuleWithCorrectLocation()
	{
		var jsonFilePath = Path.Combine(Environment.CurrentDirectory, "Data", "ruleupload.json");

		await fileService!.UploadRules(jsonFilePath, "");
		var result = await fileService!.UploadEntities(Array.Empty<string>(),
			new string[] { Path.Combine("AirHandlingUnit", $"{rulesRepo.Data[0].Id}.json") }, "");

		result.DeletedRules.Count().Should().Be(1);

		rulesRepo.Data.Count.Should().Be(0);
	}

	[TestMethod]
	public async Task ShouldNotDeleteRuleWithIncorrectLocation()
	{
		var jsonFilePath = Path.Combine(Environment.CurrentDirectory, "Data", "ruleupload.json");

		await fileService!.UploadRules(jsonFilePath, "");
		var result = await fileService!.UploadEntities(Array.Empty<string>(),
			new string[] { Path.Combine("Other", $"{rulesRepo.Data[0].Id}.json") }, "");

		result.DeletedRules.Count().Should().Be(0);

		rulesRepo.Data.Count.Should().Be(1);
	}

	[TestMethod]
	public async Task ShouldDeleteGlobal()
	{
		var jsonFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "global.json");

		await fileService!.UploadRules(jsonFilePath, "");
		var result = await fileService!.UploadEntities(Array.Empty<string>(), new string[] { $"{globalsRepo.Data[0].Id}.json" }, "");

		result.DeletedGlobals.Count().Should().Be(1);

		globalsRepo.Data.Count.Should().Be(0);
	}

	[TestMethod]
	public async Task ShouldDeleteMLModel()
	{
		var jsonFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "model.onnx");

		await fileService!.UploadRules(jsonFilePath, "");
		var result = await fileService!.UploadEntities(Array.Empty<string>(), new string[] { $"{mlModelRepo.Data[0].Id}.onnx" }, "");

		result.DeletedMLModels.Count().Should().Be(1);

		mlModelRepo.Data.Count.Should().Be(0);
	}

	[TestMethod]
	public async Task ShouldDeleteSubfolderIfEmpty()
	{
		var jsonFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "ruleupload.json");

		var dataPath = Path.Combine(Environment.CurrentDirectory, "Data", "Sub");

		var result = await fileService!.UploadRules(jsonFilePath, "");

		Directory.CreateDirectory(dataPath);

		await fileService!.WriteEntityToDisk(result.Rules.First().Id, dataPath);

		File.Exists(Path.Combine(dataPath, "AirHandlingUnit", $"{ result.Rules.First().Id}.json")).Should().BeTrue();

		await fileService!.DeleteIdFromDisk(result.Rules.First().Id, dataPath, deleteEmptyDirectories: true);

		Directory.Exists(Path.Combine(dataPath, "AirHandlingUnit")).Should().BeFalse();
	}

	[TestMethod]
	public async Task AddGlobalsFromJsonFile()
	{
		var jsonFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "global.json");

		var result = await fileService!.UploadRules(jsonFilePath, "");

		Assert.AreEqual(1, result.Globals.Count());

		globalsRepo.Data.Count.Should().Be(1);
	}

	[TestMethod]
	public async Task AddMLModelFromFile()
	{
		var jsonFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "model.onnx");

		var result = await fileService!.UploadRules(jsonFilePath, "");

		Assert.AreEqual(1, result.MLModels.Count());

		mlModelRepo.Data.Count.Should().Be(1);
	}

	[TestMethod]
	public async Task EmpyFileMustBeIgnored()
	{
		var jsonFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "invalidruleupload.json");

		var result = await fileService!.UploadRules(jsonFilePath, "");

		Assert.AreEqual(0, result.Rules.Count());
	}

	[TestMethod]
	public async Task AddRulesFromMultiFolderInvalidRulesZipFile()
	{
		var zipFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "multilevelinvalidrules.zip");

		var result = await fileService!.UploadRules(zipFilePath, "");

		result.Rules.Count().Should().BeGreaterThan(0);
		result.Failures.Contains("").Should().BeFalse();
	}

	[TestMethod]
	public async Task AddDuplicateRulesFromZipFile()
	{
		var zipFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "duplicateRules.zip");

		var result = await fileService!.UploadRules(zipFilePath, "");

		result.Rules.Count().Should().Be(1);
		result.DuplicateCount.Should().Be(2);

		rulesRepo.Data.Count.Should().Be(1);
	}

	[TestMethod]
	public async Task UploadFromDirectory()
	{
		var directory = System.IO.Path.Combine(Environment.CurrentDirectory, "Data");

		var result = await fileService!.UploadRulesFromDirectory(directory, "");

		result.Rules.Count().Should().BeGreaterThan(0);
		result.Globals.Count().Should().Be(1);
		result.MLModels.Count().Should().Be(1);

		rulesRepo.Data.Count.Should().BeGreaterThan(0);
		globalsRepo.Data.Count.Should().Be(1);
		mlModelRepo.Data.Count.Should().Be(1);

		var dataPath = Path.Combine(Environment.CurrentDirectory, "Data", "Updated");

		if(Directory.Exists(dataPath))
		{
			Directory.Delete(dataPath, true);
		}

		Directory.CreateDirectory(dataPath);

		await fileService!.WriteAllEntitiesToDisk(dataPath);

		rulesRepo.Data.Clear();
		globalsRepo.Data.Clear();
		mlModelRepo.Data.Clear();

		result = await fileService!.UploadRulesFromDirectory(dataPath, "");

		Directory.Exists(Path.Combine(dataPath, "globals")).Should().BeTrue();
		Directory.Exists(Path.Combine(dataPath, "ml_models")).Should().BeTrue();

		result.Rules.Count().Should().BeGreaterThan(0);
		result.Globals.Count().Should().Be(1);
		result.MLModels.Count().Should().Be(1);
		result.Failures.Any(v => v == "model.onnx").Should().BeFalse();

		rulesRepo.Data.Count.Should().BeGreaterThan(0);
		globalsRepo.Data.Count.Should().Be(1);
		mlModelRepo.Data.Count.Should().Be(1);
	}
}
