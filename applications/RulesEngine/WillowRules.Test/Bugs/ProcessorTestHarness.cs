using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RulesEngine.Processor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.CognitiveSearch;
using Willow.Rules.Cache;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Processor;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.ServiceBus;
using WillowRules.Services;
using WillowRules.Test.Bugs.Mocks;
using WillowRules.Visitors;

namespace WillowRules.Test.Bugs;

public record TwinOverride(string modelId, string twinId, string? trendId = null, string? unit = null, string? connectorId = null, string? externalId = null, string? valueExpression = null, string? timeZone = null, Dictionary<string, object>? contents = null);

public class ProcessorTestHarness
{
	public readonly RepositoryRuleInstancesMock repositoryRuleInstances;
	public readonly RepositoryRuleInstanceMetadataMock repositoryRuleInstanceMetadata;
	public readonly RepositoryRulesMock repositoryRules;
	public readonly RepositoryCalculatedPointMock repositoryCalculatedPoint;
	public readonly RepositoryGlobalVariableMock repositoryGlobalVariable;
	public readonly RepositoryMLModelMock repositoryMLModel;
	public readonly RepositoryRuleMetadataMock repositoryRuleMetadata;
	public readonly RepositoryInsightMock repositoryInsight;
	public readonly RepositoryCommandMock repositoryCommand;
	public readonly RepositoryRuleExecutionsMock repositoryRuleExecutions;
	public readonly RepositoryActorStateMock repositoryActorState;
	public readonly RepositoryTimeSeriesBufferMock repositoryTimeSeriesBuffer;
	public readonly RepositoryTimeSeriesMappingMock repositoryTimeSeriesMapping;
	public readonly EventHubServiceMock eventHubService;
	public readonly CommandInsightServiceMock commandInsightService;
	public readonly MLServiceMock mlService;
	public DataCacheFactoryMock dataCacheFactory;
	public readonly TwinServiceMock twinService;
	public readonly ITwinSystemService twinSystemService;
	public readonly IRulesService ruleService;
	public readonly IModelService modelService;
	public readonly IMetaGraphService metaGraphService;
	public readonly RuleTemplateRegistry ruleTemplateRegistry;
	public readonly Mock<IRepositoryADTSummary> adtSummaryMock;
	public readonly IMemoryCache memoryCache;

	public ProcessorTestHarness()
	{
		repositoryRuleInstances = new RepositoryRuleInstancesMock();
		repositoryRules = new RepositoryRulesMock();
		dataCacheFactory = new DataCacheFactoryMock();
		repositoryRuleMetadata = new RepositoryRuleMetadataMock();
		repositoryCalculatedPoint = new RepositoryCalculatedPointMock();
		repositoryInsight = new RepositoryInsightMock(repositoryRuleInstances);
		repositoryRuleInstanceMetadata = new RepositoryRuleInstanceMetadataMock();
		repositoryRuleExecutions = new RepositoryRuleExecutionsMock();
		repositoryActorState = new RepositoryActorStateMock();
		repositoryTimeSeriesBuffer = new RepositoryTimeSeriesBufferMock();
		repositoryTimeSeriesMapping = new RepositoryTimeSeriesMappingMock();
		repositoryGlobalVariable = new RepositoryGlobalVariableMock();
		repositoryMLModel = new RepositoryMLModelMock();
		repositoryCommand = new RepositoryCommandMock();
		eventHubService = new EventHubServiceMock();
		commandInsightService = new CommandInsightServiceMock();
		var willowEnvironment = MockObjects.WillowEnvironment;

		var realTwinService = new TwinService(
			Mock.Of<IADTService>(),
			willowEnvironment,
			Mock.Of<IRetryPolicies>(),
			dataCacheFactory,
			new ConsoleLogger<TwinService>());

		twinService = new TwinServiceMock(realTwinService);

		twinSystemService = new TwinSystemService(
			dataCacheFactory,
			twinService,
			willowEnvironment,
			new ConsoleLogger<TwinSystemService>()
			);

		modelService = new ModelService(
			dataCacheFactory,
			Mock.Of<IADTService>(),
			willowEnvironment,
			MockObjects.GetLogger<ModelService>());

		var memoryCacheMock = new Mock<IMemoryCache>();
		memoryCacheMock
			 .Setup(v => v.CreateEntry(It.IsAny<object>()))
			 .Returns(() => Mock.Of<ICacheEntry>());

		memoryCache = memoryCacheMock.Object;

		mlService = new MLServiceMock(new MLService(repositoryMLModel, MockObjects.GetLogger<MLService>()));

		metaGraphService = new MetaGraphService(
			willowEnvironment,
			modelService,
			twinService,
			Mock.Of<ITwinGraphService>(),
			dataCacheFactory,
			MockObjects.GetLogger<MetaGraphService>());

		ruleService = new RulesService(twinService,
			twinSystemService,
			modelService,
			mlService,
			repositoryCalculatedPoint,
			repositoryRuleInstances,
			repositoryGlobalVariable,
			repositoryMLModel,
			memoryCache,
			willowEnvironment,
			new ConsoleLogger<RulesService>(),
			MockObjects.GetLogger<BindToTwinsVisitor>());


		ruleTemplateRegistry = new RuleTemplateRegistry(MockObjects.GetLogger<RuleTemplateRegistry>());

		//Repository interface updated to resturn object
		adtSummaryMock = new Mock<IRepositoryADTSummary>();
		adtSummaryMock.Setup(v => v.GetLatest()).ReturnsAsync(() => new ADTSummary { Id = "Summary" });
	}

	public async Task<List<RuleInstance>> GenerateRuleInstances(bool optimizeExpressions = true)
	{
		var willowEnvironment = MockObjects.WillowEnvironment;

		var ruleInstancesService = new RuleInstancesService(MockObjects.GetLogger<RuleInstancesService>(),
			repositoryRuleMetadata,
			repositoryRules);

		var ruleInstanceProcessor = new RuleInstanceProcessor(
			willowEnvironment,
			new HealthCheckSearch(),
			repositoryRuleInstances,
			ruleInstancesService,
			repositoryRuleMetadata,
			new RuleServiceMock(ruleService, optimizeExpressions),
			adtSummaryMock.Object,
			Mock.Of<ILoadMemoryGraphService>(),
			Mock.Of<IRepositoryProgress>(),
			dataCacheFactory,
			twinSystemService,
			twinService,
			Mock.Of<IADTCacheService>(),
			repositoryRules,
			repositoryRuleInstanceMetadata,
			Mock.Of<IRepositoryRuleTimeSeriesMapping>(),
			Mock.Of<IRepositoryTimeSeriesMapping>(),
			repositoryActorState,
			repositoryCalculatedPoint,
			Mock.Of<IRepositoryRuleExecutionRequest>(),
			MockObjects.CustomerOptions,
			new ConsoleLogger<RuleInstanceProcessor>());

		await ruleInstanceProcessor.RebuildRules(new RuleExecutionRequest()
		{
			Id = Guid.NewGuid().ToString(),
			Command = RuleExecutionCommandType.BuildRule,
		}, CancellationToken.None);

		repositoryRuleInstances.Data.Count.Should().BeGreaterThan(0);

		return repositoryRuleInstances.Data.ToList();
	}

	public Task AddCalculatedPoint(CalculatedPoint calculatedPoint)
	{
		return repositoryCalculatedPoint.UpsertOne(calculatedPoint);
	}

	public Task AddTwinCache(BasicDigitalTwinPoco twin)
	{
		return dataCacheFactory.TwinCache.AddOrUpdate(MockObjects.WillowEnvironment.Id, twin.Id, twin);
	}

	public async Task AddRule(Rule rule)
	{
		await repositoryRuleMetadata.UpsertOne(new RuleMetadata(rule.Id));
		await repositoryRules.UpsertOne(rule);
	}

	public async Task AddBackwardEdge(string twinId, Edge edge)
	{
		var entry = await dataCacheFactory.BackEdgeCache.GetOrCreateAsync(MockObjects.WillowEnvironment.Id, twinId, () => Task.FromResult<CollectionWrapper<Edge>?>(new CollectionWrapper<Edge>(new List<Edge>())));
		entry!.Items.Add(edge);
	}

	public async Task AddForwardEdge(string twinId, Edge edge)
	{
		var entry = await dataCacheFactory.ForwardEdgeCache.GetOrCreateAsync(MockObjects.WillowEnvironment.Id, twinId, () => Task.FromResult<CollectionWrapper<Edge>?>(new CollectionWrapper<Edge>(new List<Edge>())));
		entry!.Items.Add(edge);
	}

	public Task AddToModelGraph(TwinOverride twin, string extends)
	{
		return AddToModelGraph(new ModelData()
		{
			Id = twin.modelId,
			DtdlModel = new DtdlModel()
			{
				extends = new StringList()
				{
					extends
				}
			}
		});
	}

	public async Task AddToModelGraph(ModelData modelData)
	{
		modelData.DtdlModel.extends ??= new StringList();

		var allModelsData = (await dataCacheFactory.AllModelsCache.GetAll(MockObjects.WillowEnvironment.Id).ToListAsync()).FirstOrDefault()?.Items ?? new List<ModelData>();

		var modelIndex = allModelsData.IndexOf(v => v.Id == modelData.Id);

		if (modelIndex >= 0)
		{
			var existing = allModelsData[modelIndex];

			modelData.DtdlModel.extends = new StringList(existing.DtdlModel.extends.Union(modelData.DtdlModel.extends).ToArray());

			allModelsData[modelIndex] = modelData;
		}
		else
		{
			allModelsData.Add(modelData);
		}

		await dataCacheFactory.AllModelsCache.AddOrUpdate(MockObjects.WillowEnvironment.Id, "allmodels4", new CollectionWrapper<ModelData>(allModelsData));
	}

	public Task AddTwinMapping(TwinOverride sensor)
	{
		return AddTwinMapping(new BasicDigitalTwinPoco()
		{
			Id = !string.IsNullOrEmpty(sensor.twinId) ? sensor.twinId : sensor.trendId,
			trendID = sensor.trendId,
			trendInterval = 900,
			name = sensor.twinId ?? sensor.trendId,
			connectorID = sensor.connectorId,
			externalID = sensor.externalId,
			TimeZone = !string.IsNullOrEmpty(sensor.timeZone) ? new Willow.Rules.Model.TimeZone() { Name = sensor.timeZone } : null,
			ValueExpression = sensor.valueExpression,
			Contents = sensor.contents ?? new Dictionary<string, object>(),
			Metadata = new DigitalTwinMetadataPoco()
			{
				ModelId = sensor.modelId
			},
			unit = sensor.unit
		});
	}

	public Task AddTwinMapping(BasicDigitalTwinPoco sensor)
	{
		return repositoryTimeSeriesMapping.UpsertOne(new TimeSeriesMapping()
		{
			Id = sensor.Id,
			TrendId = sensor.trendID,
			TrendInterval = sensor.trendInterval,
			DtId = sensor.Id,
			ConnectorId = sensor.connectorID ?? Guid.NewGuid().ToString(),
			ExternalId = sensor.externalID ?? Guid.NewGuid().ToString(),
			LastUpdate = DateTimeOffset.UtcNow
		});
	}

	public RuleSimulationService CreateRuleSimulationService(string adxFilePath, DateTime? endDate = null, int maxDaysToKeep = 1000)
	{
		var adxService = new FileBasedADXService(adxFilePath, MockObjects.GetLogger<FileBasedADXService>(), endDate);

		var repositoryTimeseriesSim = new RepositoryTimeSeriesBufferMock();
		var timeSeriesManagerSim = new TimeSeriesManager(repositoryTimeseriesSim, repositoryTimeSeriesMapping, Mock.Of<ITelemetryCollector>(), modelService, MockObjects.GetLogger<TimeSeriesManager>(), maxDaysToKeep);
		var timeSeriesManagerMockSim = new TimeSeriesManagerMock(timeSeriesManagerSim);

		return new RuleSimulationService(
						timeSeriesManagerMockSim,
						ruleService,
						twinService,
						twinSystemService,
						modelService,
						ruleTemplateRegistry,
						repositoryActorState,
						repositoryTimeSeriesBuffer,
						repositoryRuleInstances,
						repositoryGlobalVariable,
						adxService,
						mlService,
						repositoryRules,
						MockObjects.GetLogger<RuleSimulationService>(),
						maxTimeRange: TimeSpan.FromDays(10 * 365));
	}

	public async Task<(List<Insight> insights, List<ActorState> actors, List<TimeSeries> timeseries)> ExecuteRules(
		string adxFilePath,
		DateTime? startDate = null,
		DateTime? endDate = null,
		bool assertSimulation = true,
		int maxDaysToKeep = 1000,
		int maxOutputDaysToKeep = 1000,
		int maxOutputvaluesToKeep = 250,
		string? ruleId = null,
		bool limitUntracked = true,
		bool enableCompression = true,
		bool isRealtime = true,
		bool applyLimits = true)
	{
		startDate = DateTime.SpecifyKind((startDate ?? DateTime.Now.AddYears(-10)).ToUniversalTime(), DateTimeKind.Utc);
		endDate = DateTime.SpecifyKind((endDate ?? DateTime.Now).ToUniversalTime(), DateTimeKind.Utc);
		var willowEnvironment = MockObjects.WillowEnvironment;

		var actorManager = new ActorManager(repositoryActorState, Mock.Of<ITelemetryCollector>(), MockObjects.GetLogger<ActorManager>(), maxDaysToKeep: maxDaysToKeep, maxOutputValuesToKeep: maxOutputvaluesToKeep);
		var actorManagerMock = new ActorManagerMock(actorManager);
		var timeSeriesManager = new TimeSeriesManager(repositoryTimeSeriesBuffer, repositoryTimeSeriesMapping, Mock.Of<ITelemetryCollector>(), modelService, MockObjects.GetLogger<TimeSeriesManager>(), maxDaysToKeep);
		var timeSeriesManagerMock = new TimeSeriesManagerMock(timeSeriesManager);
		var insightsManager = new InsightsManager(repositoryInsight, Mock.Of<ITelemetryCollector>(), MockObjects.GetLogger<InsightsManager>(), maxOccurenceLiftime: DateTime.UtcNow - new DateTime(1978, 1, 1));
		var commandsManager = new CommandsManager(repositoryCommand, Mock.Of<ICommandService>(), Mock.Of<ITelemetryCollector>(), MockObjects.GetLogger<CommandsManager>());
		var ruleOrchestrator = new RuleOrchestrator(willowEnvironment, MockObjects.GetLogger<RuleOrchestrator>());
		var rulesManager = new RulesManager(repositoryRules, repositoryRuleInstances, repositoryRuleInstanceMetadata, repositoryRuleMetadata, timeSeriesManager, MockObjects.GetLogger<RulesManager>());
		var queueService = new InsightBackgroundService(repositoryInsight, commandInsightService, insightsManager, Mock.Of<ITelemetryCollector>(), MockObjects.GetLogger<InsightBackgroundService>(), cleanupPeriod: 0);

		//var queueTask = queueService.StartAsync(CancellationToken.None);

		var adxService = new FileBasedADXService(adxFilePath, MockObjects.GetLogger<FileBasedADXService>(), endDate);

		//only assert simulation service for the first execution. Subsequent executions has too many different changes to compare
		assertSimulation = assertSimulation && repositoryActorState.Data.Count == 0 && repositoryTimeSeriesBuffer.Data.Count == 0;

		//eventHubServiceMock.SetupGet(v => v.Writer
		var ruleExecutionProcessor = new RuleExecutionProcessor(
			willowEnvironment,
			adxService,
			repositoryRuleExecutions,
			Mock.Of<IRepositoryProgress>(),
			repositoryRules,
			Mock.Of<IRepositoryLogEntry>(),
			adtSummaryMock.Object,
			ruleTemplateRegistry,
			Mock.Of<IMessageSenderBackEnd>(),
			Mock.Of<IDataQualityService>(),
			rulesManager,
			actorManagerMock,
			insightsManager,
			commandsManager,
			timeSeriesManagerMock,
			Mock.Of<ITelemetryCollector>(),
			ruleOrchestrator,
			eventHubService,
			mlService,
			modelService,
			Mock.Of<IMemoryCache>(),
			MockObjects.CustomerOptions,
			new RulesEngine.Processor.Services.HealthCheckProcessor(),
			//MockObjects.GetLogger<RuleExecutionProcessor>>()
			MockObjects.GetLogger(new ConsoleLogger<RuleExecutionProcessor>()),
			thrashingDelay: 0);

		var execution = ruleExecutionProcessor.Execute(new RuleExecutionRequest()
		{
			Id = Guid.NewGuid().ToString(),
			ProgressId = Progress.RuleExecutionId,
			ExtendedData = new RuleExecutionRequestExtendedData()
			{
				StartDate = startDate,
				TargetEndDate = endDate,
				RuleId = ruleId ?? ""
			},
			Command = RuleExecutionCommandType.ProcessDateRange
		},
		isRealtime: isRealtime,
		CancellationToken.None);

		if (isRealtime)
		{
			await ruleOrchestrator.Send.WriteAsync(new RuleExecutionRequest());

			ruleOrchestrator.Send.Complete();
		}

		await execution;

		insightsManager.Writer.Complete();

		await queueService.ReadFromChannelAndEnqueue(CancellationToken.None);

		queueService.CompleteInnerQueue();

		await queueService.ProcessInsights(CancellationToken.None);

		var insights = repositoryInsight.Data;
		var actors = repositoryActorState.Data;
		var timeSeries = repositoryTimeSeriesBuffer.Data;

		//try to assert simulation service to ensure it is generally aligned to the execution processor
		if (assertSimulation || !enableCompression)
		{
			foreach (var rule in repositoryRules.Data)
			{
				foreach (var ruleInstance in repositoryRuleInstances.Data.Where(v => v.RuleId == rule.Id))
				{
					if (!actors.Any(v => v.Id == ruleInstance.Id))
					{
						continue;
					}
					var simulationService = CreateRuleSimulationService(adxFilePath, endDate, maxDaysToKeep);

					(var pointLog, var instanceSim, var actorSim, var insightSim, var commands, _) = await simulationService.ExecuteRule(
						rule,
						ruleInstance.EquipmentId,
						startDate.Value,
						endDate.Value,
						maxTimeToKeep: TimeSpan.FromDays(maxDaysToKeep),
						limitUntracked: limitUntracked,
						enableCompression: enableCompression,
						applyLimits: applyLimits);

					var insight = insights.Single(v => v.Id == insightSim.Id);
					var actor = actors.Single(v => v.Id == actorSim.Id);

					if (!enableCompression)
					{
						return (new Insight[] { insightSim }.ToList(), new ActorState[] { actorSim }.ToList(), timeSeries);
					}

					var minDate = actorSim.OutputValues.Points.Min(v => v.StartTime);

					actorSim.OutputValues.ApplyLimits(5000, minDate.UtcDateTime);

					Assert(actor, actorSim);
					Assert(insight, insightSim);
				}
			}
		}

		return (insights, actors, timeSeries);
	}

	public void OverrideCaches(Rule rule, string twinId, string sensorModellId, string sensorTrendId)
	{
		OverrideCaches(rule, new TwinOverride(rule.PrimaryModelId, twinId, ""), new List<TwinOverride>() { new TwinOverride(sensorModellId, sensorTrendId, sensorTrendId) });
	}

	public void OverrideCaches(Rule rule, TwinOverride twin, params TwinOverride[] sensors)
	{
		repositoryRules.UpsertOne(rule).Wait();

		OverrideCaches(twin, sensors.ToList());
	}

	public void OverrideCaches(Rule rule, TwinOverride twin, List<TwinOverride> sensors)
	{
		repositoryRules.UpsertOne(rule).Wait();

		OverrideCaches(twin, sensors);
	}

	public void OverrideCaches(TwinOverride twin, List<TwinOverride> sensors)
	{
		List<BasicDigitalTwinPoco> sensorTwins = new();

		var equipment = new BasicDigitalTwinPoco()
		{
			Id = twin.twinId,
			name = twin.twinId,
			trendID = twin.trendId,
			trendInterval = 900,
			connectorID = twin.connectorId,
			externalID = twin.externalId,
			TimeZone = !string.IsNullOrEmpty(twin.timeZone) ? new Willow.Rules.Model.TimeZone() { Name = twin.timeZone } : null,
			ValueExpression = twin.valueExpression,
			Contents = twin.contents ?? new Dictionary<string, object>(),
			Metadata = new DigitalTwinMetadataPoco()
			{
				ModelId = twin.modelId
			}
		};

		var edges = new List<Edge>();

		foreach (var sensor in sensors)
		{
			var sensorTwin = new BasicDigitalTwinPoco()
			{
				Id = !string.IsNullOrEmpty(sensor.twinId) ? sensor.twinId : sensor.trendId,
				trendID = sensor.trendId,
				trendInterval = 900,
				name = sensor.twinId ?? sensor.trendId,
				connectorID = sensor.connectorId,
				externalID = sensor.externalId,
				TimeZone = !string.IsNullOrEmpty(sensor.timeZone) ? new Willow.Rules.Model.TimeZone() { Name = sensor.timeZone } : null,
				ValueExpression = sensor.valueExpression,
				Contents = sensor.contents ?? new Dictionary<string, object>(),
				Metadata = new DigitalTwinMetadataPoco()
				{
					ModelId = sensor.modelId
				},
				unit = sensor.unit
			};

			sensorTwins.Add(sensorTwin);

			edges.Add(new Edge()
			{
				RelationshipType = "isCapabilityOf",
				Destination = sensorTwin
			});
		}

		OverrideCaches(equipment, sensorTwins);
	}

	private void OverrideCaches(
		BasicDigitalTwinPoco equipment,
		List<BasicDigitalTwinPoco> sensors)
	{
		var willowEnvironment = MockObjects.WillowEnvironment;

		var backwardEdges = new Dictionary<string, List<Edge>>()
		{
			[equipment.Id] = new List<Edge>()
		};

		var forwardEdges = new Dictionary<string, List<Edge>>();
		var models = new List<ModelData>()
		{
			new ModelData()
			{
				Id = equipment.Metadata.ModelId,
				DtdlModel = new DtdlModel()
				{
					contents = sensors.Select(v => v.Metadata.ModelId).Distinct().Select(v => new Content()
					{
						name = v,
						target = v,
						type = "Relationship"
					}).ToArray()
				}
			}
		};

		if (!models.Any(v => v.Id == "dtmi:com:willowinc:Capability;1"))
		{
			models.Add(new ModelData()
			{
				Id = "dtmi:com:willowinc:Capability;1",
				DtdlModel = new DtdlModel()
				{
				}
			});
		}

		if (!models.Any(v => v.Id == "dtmi:com:willowinc:Event;1"))
		{
			models.Add(new ModelData()
			{
				Id = "dtmi:com:willowinc:Event;1",
				DtdlModel = new DtdlModel()
				{
				}
			});
		}

		foreach (var sensor in sensors)
		{
			var sensorModel = models.FirstOrDefault(v => v.Id == sensor.Metadata.ModelId);

			if (sensorModel is null)
			{
				sensorModel = new ModelData()
				{
					Id = sensor.Metadata.ModelId,
					DtdlModel = new DtdlModel()
					{
						extends = new string[]
						{
							"dtmi:com:willowinc:Capability;1"
						}
					}
				};

				models.Add(sensorModel);
			}

			backwardEdges[equipment.Id].Add(new Edge()
			{
				RelationshipType = "isCapabilityOf",
				Destination = sensor
			});

			forwardEdges[sensor.Id] = new List<Edge>()
			{
				new Edge()
				{
					RelationshipType = "isCapabilityOf",
					Destination = equipment
				}
			};
		}

		var twinsByInheritance = dataCacheFactory.DiskCacheTwinsByModelWithInheritance.GetOrCreateAsync(willowEnvironment.Id, equipment.Metadata.ModelId,
			() => Task.FromResult<CollectionWrapper<BasicDigitalTwinPoco>?>(new CollectionWrapper<BasicDigitalTwinPoco>(new List<BasicDigitalTwinPoco>()))).Result;

		if (!twinsByInheritance!.Items.Any(v => v.Id == equipment.Id))
		{
			twinsByInheritance!.Items.Add(equipment);
		}

		dataCacheFactory.TwinCache.AddOrUpdate(willowEnvironment.Id, equipment.Id, equipment).Wait();

		foreach (var model in models)
		{
			AddToModelGraph(model).Wait();

			if (equipment.Metadata.ModelId == model.Id && model.DtdlModel.extends is not null)
			{
				foreach (var id in model.DtdlModel.extends)
				{
					twinsByInheritance = dataCacheFactory.DiskCacheTwinsByModelWithInheritance.GetOrCreateAsync(willowEnvironment.Id, id,
			() => Task.FromResult<CollectionWrapper<BasicDigitalTwinPoco>?>(new CollectionWrapper<BasicDigitalTwinPoco>(new List<BasicDigitalTwinPoco>()))).Result;

					if (!twinsByInheritance!.Items.Any(v => v.Id == equipment.Id))
					{
						twinsByInheritance!.Items.Add(equipment);
					}
				}
			}
		}

		var modelData = dataCacheFactory.AllModelsCache.GetAll(MockObjects.WillowEnvironment.Id).ToListAsync().Result;


		foreach (var sensor in sensors)
		{
			dataCacheFactory.TwinCache.AddOrUpdate(willowEnvironment.Id, sensor.Id, sensor).Wait();
		}

		foreach (var entry in backwardEdges)
		{
			dataCacheFactory.BackEdgeCache.AddOrUpdate(willowEnvironment.Id, entry.Key, new CollectionWrapper<Edge>(entry.Value.ToList())).Wait();
		}

		foreach (var entry in forwardEdges)
		{
			dataCacheFactory.ForwardEdgeCache.AddOrUpdate(willowEnvironment.Id, entry.Key, new CollectionWrapper<Edge>(entry.Value.ToList())).Wait();
		}

		if (!string.IsNullOrEmpty(equipment.trendID))
		{
			repositoryTimeSeriesMapping.UpsertOne(new TimeSeriesMapping()
			{
				Id = equipment.Id,
				TrendId = equipment.trendID,
				TrendInterval = equipment.trendInterval,
				DtId = equipment.Id,
				ConnectorId = equipment.connectorID ?? Guid.NewGuid().ToString(),
				ExternalId = equipment.externalID ?? Guid.NewGuid().ToString(),
				ModelId = equipment.Metadata.ModelId,
				LastUpdate = DateTimeOffset.UtcNow
			}).Wait();
		}

		foreach (var sensor in sensors)
		{
			repositoryTimeSeriesMapping.UpsertOne(new TimeSeriesMapping()
			{
				Id = sensor.Id,
				TrendId = sensor.trendID,
				TrendInterval = sensor.trendInterval,
				DtId = sensor.Id,
				ConnectorId = sensor.connectorID ?? Guid.NewGuid().ToString(),
				ExternalId = sensor.externalID ?? Guid.NewGuid().ToString(),
				ModelId = sensor.Metadata.ModelId,
				LastUpdate = DateTimeOffset.UtcNow
			}).Wait();
		}
	}

	private static void Assert(Insight insight1, Insight insight2)
	{
		insight1.Occurrences.Count.Should().Be(insight2.Occurrences.Count);

		int index = 0;
		var insight2Occurrences = insight2.Occurrences.ToList();

		foreach (var occ1 in insight1.Occurrences)
		{
			var occ2 = insight2Occurrences[index];

			occ1.IsFaulted.Should().Be(occ2.IsFaulted);
			occ1.IsValid.Should().Be(occ2.IsValid);
			occ1.Ended.ToString().Should().Be(occ2.Ended.ToString());
			occ1.Started.ToString().Should().Be(occ2.Started.ToString());
			occ1.Text.Should().Be(occ2.Text);
			index++;
		}
	}

	private static void Assert(ActorState actor1, ActorState actor2)
	{
		actor1.TimedValues.Keys.Should().BeEquivalentTo(actor2.TimedValues.Keys);

		int index = 0;
		foreach ((var key, var occ1) in actor1.TimedValues)
		{
			var occ2 = actor2.TimedValues[key];
			Assert(occ1, occ2);
			index++;
		}

		index = 0;

		foreach (var occ1 in actor1.OutputValues.Points)
		{
			var occ2 = actor2.OutputValues.Points[index];
			occ1.EndTime.ToString().Should().Be(occ2.EndTime.ToString());
			occ1.Faulted.Should().Be(occ2.Faulted);
			occ1.IsValid.Should().Be(occ2.IsValid);
			occ1.StartTime.ToString().Should().Be(occ2.StartTime.ToString());
			occ1.Text.Should().Be(occ2.Text);
			index++;
		}

		actor1.OutputValues.Points.Count.Should().Be(actor2.OutputValues.Points.Count);
	}

	private static void Assert(TimeSeriesBuffer timeseries1, TimeSeriesBuffer timeseries2)
	{
		int index = 0;
		//if a time series assertion for an actor fails here it may well be due to the condensor logic in the processor and
		//the simulator not doing that. Spread the timeseries data for the test over larger periods
		//OR skip simluation assertion for the test
		foreach (var occ1 in timeseries1.Points)
		{
			var occ2 = timeseries2.Points.ToList()[index];

			occ1.Timestamp.ToString().Should().Be(occ2.Timestamp.ToString());
			occ1.ValueBool.Should().Be(occ2.ValueBool);
			Math.Round(occ1.ValueDouble ?? 0, 2).Should().Be(Math.Round(occ2.ValueDouble ?? 0, 2));
			index++;
		}

		timeseries1.Points.Count().Should().Be(timeseries2.Points.Count());
	}
}
