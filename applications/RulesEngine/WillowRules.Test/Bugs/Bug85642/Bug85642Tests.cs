using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Processor;
using Willow.Rules.Repository;
using Willow.Rules.Services;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug85642Tests
{
	[TestMethod]
	public async Task ActorMustRemoveReallyOldPoints()
	{
		var harness = new ProcessorTestHarness();

		var actor = new ActorState()
		{
			TimedValues = new Dictionary<string, TimeSeriesBuffer>(),
			OutputValues = new OutputValues()
		};

		ActorStateExtensions.PruneAndCheckValid(actor.TimedValues, new TimedValue(DateTimeOffset.Now.AddMonths(-1), 1), "old", "");
		ActorStateExtensions.PruneAndCheckValid(actor.TimedValues, new TimedValue(DateTimeOffset.Now, 1), "ok", "");

		harness.repositoryActorState.Data.Add(actor);

		var actorManager = new ActorManager(harness.repositoryActorState, Mock.Of<ITelemetryCollector>(), Mock.Of<ILogger<ActorManager>>());

		await actorManager.FlushActorsToDatabase(new System.Collections.Concurrent.ConcurrentDictionary<string, ActorState>()
		{
			["a"] = actor
		},
		new Dictionary<string, List<RuleInstance>>()
		{
			["a"] = new List<RuleInstance>()
			{
				new RuleInstance()
				{
					Id = "a"
				}
			}
		}, DateTime.Now,
		new Willow.Rules.Services.ProgressTrackerForRuleExecution("","", ProgressType.Cache,"", Mock.Of<IRepositoryProgress>(), "", DateTimeOffset.Now, Mock.Of<ILogger>()));

		actor.TimedValues.Count.Should().Be(1);

		actor.TimedValues["ok"].Count.Should().Be(1);	
	}
}
