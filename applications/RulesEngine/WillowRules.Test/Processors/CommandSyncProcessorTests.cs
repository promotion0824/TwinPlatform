using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RulesEngine.Processor.Services;
using Willow.RealEstate.Command.Generated;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.ServiceBus;

namespace WillowRules.Test.Processors;

[TestClass]
public class CommandSyncProcessorTests
{
	[TestMethod]
	public async Task ReverseSyncCommandInsights_MustUpdateCommandId()
	{
		var siteId1 = Guid.NewGuid();
		var siteId2 = Guid.NewGuid();
		var commandId1 = Guid.NewGuid();
		var commandId2 = Guid.NewGuid();
		var insightId1 = "insight1";
		var insightId2 = "insight2";

		IEnumerable<InsightSimpleDto> commandInsightList1 = new List<InsightSimpleDto>()
		{
			new InsightSimpleDto()
			{
				ExternalId = insightId1,
				Id = commandId1
			}
		};

		IEnumerable<InsightSimpleDto> commandInsightList2 = new List<InsightSimpleDto>()
		{
			new InsightSimpleDto()
			{
				ExternalId = insightId2,
				Id = commandId2
			}
		};

		var repositoryInsight = new Mock<IRepositoryInsight>();

		repositoryInsight
			.Setup(v => v.GetSiteIds())
			.Returns(Task.FromResult((IEnumerable<(Guid, int)>)new List<(Guid siteId, int count)>()
			{
				(siteId1, 1),
				(siteId2, 1),
			}));

		var insightService = new Mock<ICommandInsightService>();

		insightService
			.SetupSequence(v => v.GetInsightsForSiteId(It.IsIn(siteId1, siteId2)))
			.Returns(Task.FromResult(commandInsightList1))
			.Returns(Task.FromResult(commandInsightList2));


		var processor = new CommandSyncProcessor(
			Mock.Of<ILogger<CommandSyncProcessor>>(),
			repositoryInsight.Object,
			Mock.Of<IRepositoryActorState>(),
			Mock.Of<IRepositoryTimeSeriesBuffer>(),
			insightService.Object,
			Mock.Of<ICommandService>(),
			Mock.Of<IRepositoryProgress>(),
			Mock.Of<IRepositoryCommand>(),
			Mock.Of<IRepositoryRules>(),
			Mock.Of<IRepositoryRuleInstances>(),
			Mock.Of<IMessageSenderBackEnd>(),
			MockObjects.WillowEnvironment);

		await processor.ReverseSyncCommandInsights(new RuleExecutionRequest());

		repositoryInsight.Verify(mock => mock.SetCommandInsightId(It.Is<string>(v => v == insightId1), It.Is<Guid>(v => v == commandId1), It.IsAny<Willow.Rules.Model.InsightStatus>()), Times.Once());
		repositoryInsight.Verify(mock => mock.SetCommandInsightId(It.Is<string>(v => v == insightId2), It.Is<Guid>(v => v == commandId2), It.IsAny<Willow.Rules.Model.InsightStatus>()), Times.Once());
	}
}
