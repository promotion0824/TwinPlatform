using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Willow.Rules.Model;
using Willow.Rules.Processor;
using Willow.Rules.Services;

namespace WillowRules.Test.Bugs.Mocks
{
	/// <summary>
	/// Wraps time actor manager and intercepts the methods for pre-execution assertions
	/// </summary>
	public class ActorManagerMock : IActorManager
	{
		private readonly ActorManager actorManager;

		public ActorManagerMock(ActorManager actorManager)
		{
			this.actorManager = actorManager ?? throw new ArgumentNullException(nameof(actorManager));
		}

		public (int removed, int totalTracked) ApplyLimits(ActorState actor, RuleInstance ruleInstance, DateTime nowUtc)
		{
			return actorManager.ApplyLimits(actor, ruleInstance, nowUtc);
		}

		public Task FlushActorsToDatabase(ConcurrentDictionary<string, ActorState> actors, Dictionary<string, List<RuleInstance>> instanceLookup, DateTime nowUtc, ProgressTrackerForRuleExecution progressTracker)
		{
			actors.Values.Count.Should().BeGreaterOrEqualTo(1);
			actors.Values.All(a => a.HasOverlappingOutputValues()).Should().BeFalse();
			actors.Values.All(a => a.OutputValues.IsInOrder()).Should().BeTrue();
			actors.Values.All(a => a.TimedValues.Values.All(tv => tv.CheckTimeSeriesIsInOrder())).Should().BeTrue();

			nowUtc = actors.Values.Max(v => v.Timestamp).DateTime;

			return actorManager.FlushActorsToDatabase(actors, instanceLookup, nowUtc, progressTracker);
		}

		public Task<ConcurrentDictionary<string, ActorState>> LoadActorState(Dictionary<string, List<RuleInstance>> ruleInstanceLookup, DateTimeOffset earliest, ProgressTrackerForRuleExecution progressTracker)
		{
			return actorManager.LoadActorState(ruleInstanceLookup, earliest, progressTracker);
		}
	}
}
