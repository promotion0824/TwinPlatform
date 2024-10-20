using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using RulesEngine.UtilsApi.DTO;
using RulesEngineUtils.Api.Mocks;
using Willow.Expressions;
using Willow.Rules.Cache;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using WillowRules.Visitors;

namespace RulesEngine.UtilsApi.Controllers;

/// <summary>
/// An api for overriding local caches to simulate a specifc production scenario for a rule
/// </summary>
[ApiController]
[Route("[controller]")]
public class DataOverrideController : ControllerBase
{
	private readonly IHttpClientFactory httpClientFactory;
	private readonly IRepositoryRules repositoryRules;
	private readonly IRepositoryRuleInstances repositoryRuleInstances;
	private readonly IRepositoryCalculatedPoint repositoryCalculatedPoint;
	private readonly IRepositoryGlobalVariable repositoryGlobalVariable;
	private readonly IRepositoryMLModel repositoryMLModel;
	private readonly IDataCacheFactory dataCacheFactory;
	private readonly WillowEnvironment willowEnvironment;
	private readonly IFileService fileService;
	private readonly ILoggerFactory loggerFactory;
	private readonly IMemoryCache memoryCache;
	private readonly IMLService mlService;

	/// <summary>
	/// Consutructor
	/// </summary>
	public DataOverrideController(
		IHttpClientFactory httpClientFactory,
		IRepositoryRules repositoryRules,
		IDataCacheFactory dataCacheFactory,
		IRepositoryRuleInstances repositoryRuleInstances,
		IRepositoryCalculatedPoint repositoryCalculatedPoint,
		IRepositoryGlobalVariable repositoryGlobalVariable,
		IRepositoryMLModel repositoryMLModel,
		IMLService mlService,
		IFileService fileService,
		WillowEnvironment willowEnvironment,
		IMemoryCache memoryCache,
		ILoggerFactory loggerFactory)
	{
		this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
		this.repositoryRules = repositoryRules ?? throw new ArgumentNullException(nameof(repositoryRules));
		this.repositoryRuleInstances = repositoryRuleInstances ?? throw new ArgumentNullException(nameof(repositoryRuleInstances));
		this.repositoryCalculatedPoint = repositoryCalculatedPoint ?? throw new ArgumentNullException(nameof(repositoryCalculatedPoint));
		this.repositoryGlobalVariable = repositoryGlobalVariable ?? throw new ArgumentNullException(nameof(repositoryGlobalVariable));
		this.repositoryMLModel = repositoryMLModel ?? throw new ArgumentNullException(nameof(repositoryMLModel));
		this.dataCacheFactory = dataCacheFactory ?? throw new ArgumentNullException(nameof(dataCacheFactory));
		this.mlService = mlService ?? throw new ArgumentNullException(nameof(mlService));
		this.fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
	}

	/// <summary>
	/// Override Rule Instance From File
	/// </summary>
	[HttpPost("OverrideRuleInstanceFromFile")]
	public async Task<IActionResult> OverrideRuleInstanceFromFile(string filePath)
	{
		await fileService.UploadRuleInstanceDebugInfo(filePath);

		var rule = (await repositoryRules.Get()).FirstOrDefault();

		if (rule is not null)
		{
			var ruleInstance = (await repositoryRuleInstances.Get()).First();

			(var ok, var equipment) = await dataCacheFactory.TwinCache.TryGetValue(willowEnvironment.Id, ruleInstance.EquipmentId);

			if (ok)
			{
				//write top level cache for ruleexpansion
				await dataCacheFactory.DiskCacheTwinsByModelWithInheritance.AddOrUpdate(willowEnvironment.Id, rule.PrimaryModelId,
					new CollectionWrapper<BasicDigitalTwinPoco>(new List<BasicDigitalTwinPoco>()
					{
					equipment!
					}));
			}
		}

		return Ok();
	}

	/// <summary>
	/// Overrides ADT caches for the specified rule
	/// </summary>
	[HttpPost("OverrideTwins")]
	public async Task<IActionResult> OverrideTwins(TwinOverrideRequest request)
	{
		var client = httpClientFactory.CreateClient();

		client.DefaultRequestHeaders.Add("Authorization", $"Bearer {request.BearerToken}");

		var twinService = new TwinServiceWithHttpClient(willowEnvironment, dataCacheFactory, client, request.SourceUrl!);
		var twinSystemService = new TwinSystemService(dataCacheFactory, twinService, willowEnvironment, loggerFactory.CreateLogger<TwinSystemService>());
		var modelService = new ModelService(dataCacheFactory, new ADTServiceMock(), willowEnvironment, loggerFactory.CreateLogger<ModelService>());
		
		var rulesService = new RulesService(
			twinService,
			twinSystemService,
			modelService,
			mlService,
			repositoryCalculatedPoint,
			repositoryRuleInstances,
			repositoryGlobalVariable,
			repositoryMLModel,
			memoryCache,
			willowEnvironment,
			loggerFactory.CreateLogger<RulesService>(),
			loggerFactory.CreateLogger<BindToTwinsVisitor>());

		var rule = (await repositoryRules.GetOne(request.RuleId!)) ?? throw new ArgumentNullException(nameof(request.RuleId), request.RuleId);

		foreach (var instance in await repositoryRuleInstances.Get(v => true))
		{
			var twin = await twinService.GetCachedTwin(instance.EquipmentId);

			var graph = await twinSystemService.GetTwinSystemGraph(new[] { twin!.Id });

			var twinContext = TwinDataContext.Create(twin, graph);

			var env = await rulesService.AddGlobalsToEnv(Env.Empty.Push());

			env = await rulesService.AddMLModelsToEnv(env);

			await rulesService.ProcessOneTwin(rule, twinContext, env, new Dictionary<string, Rule>());
		}

		return Ok();
	}
}
