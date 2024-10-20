using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Models;

[TestClass]
public class InsightsTests
{
	[TestMethod]
	public void UpdateFaultedProperties()
	{
		var env = Env.Empty.Push();
		var insight = new Insight()
		{
			Occurrences = new List<InsightOccurrence>()
		};
		var actor = new ActorState("rule", "ruleInstance", DateTimeOffset.Now, 1);

		var rule = new Rule();
		var ruleInstance = new RuleInstance()
		{
			RuleImpactScoresBound = new List<RuleParameterBound>()
		};

		var startDate = DateTimeOffset.Now;
		var faultedStart = startDate.AddHours(1);
		var faultedEnd = startDate.AddHours(2);
		var healthyStart = startDate.AddHours(3);
		var healthyEnd = startDate.AddHours(4);

		actor.ValidOutput(faultedStart, true, env);
		actor.ValidOutput(faultedEnd, true, env);
		insight.UpdateValues(actor, ruleInstance);

		insight.IsFaulty.Should().BeTrue();

		actor.ValidOutput(healthyStart, false, env);
		actor.ValidOutput(healthyEnd, false, env);
		insight.UpdateValues(actor, ruleInstance);
		insight.UpdateOccurrences(actor, ruleInstance);

		insight.IsFaulty.Should().BeFalse();
		insight.EarliestFaultedDate.Should().Be(faultedStart);
		insight.LastFaultedDate.Should().Be(faultedEnd);
		insight.FaultedCount.Should().Be(1);
	}

	[TestMethod]
	public void FaultedCountShouldAlignToOccurrences()
	{
		var env = Env.Empty.Push();
		var insight = new Insight()
		{
			Occurrences = new List<InsightOccurrence>()
		};
		var actor = new ActorState("rule", "ruleInstance", DateTimeOffset.Now, 1);

		var rule = new Rule();
		var ruleInstance = new RuleInstance()
		{
			RuleImpactScoresBound = new List<RuleParameterBound>()
		};

		var startDate = DateTimeOffset.Now;
		var faultedStart = startDate.AddHours(1);

		for (int i = 0; i <= 50; i++)
		{
			if (i % 2 == 0)
			{
				actor.ValidOutput(faultedStart.AddHours(i), false, env);
			}
			else
			{
				actor.InvalidOutput(faultedStart.AddHours(i), "invalid");
			}
		}

		insight.UpdateValues(actor, ruleInstance);

		insight.UpdateOccurrences(actor, ruleInstance);

		insight.Occurrences.All(v => !v.IsFaulted).Should().BeTrue();
		insight.FaultedCount.Should().Be(0);

		for (int i = 0; i < 50; i++)
		{
			if (i % 3 == 0)
			{
				actor.ValidOutput(faultedStart.AddHours(i), true, env);
			}
			else
			{
				actor.InvalidOutput(faultedStart.AddHours(i), "invalid");
			}
		}

		insight.UpdateValues(actor, ruleInstance);

		insight.UpdateOccurrences(actor, ruleInstance);

		insight.FaultedCount.Should().BeGreaterThan(0);

		insight.Occurrences.Count(v => v.IsFaulted).Should().Be(insight.FaultedCount);
	}

	[TestMethod]
	public void UpdateDescriptionBasedOnFaulty()
	{
		var insight = new Insight()
		{
			Occurrences = new List<InsightOccurrence>()
		};
		var rule = new Rule()
		{
			Id = "rule"

		};
		var ruleInstance = new RuleInstance()
		{
			Id = "ri",
			RuleId = "rule",
			Description = @"Some text FAULTYTEXT(Not looking great.) More text NONFAULTYTEXT(Looking awesome.) Another paragraph FAULTYTEXT(???) NONFAULTYTEXT(...)",
			Recommendations = @"Reccommend FAULTYTEXT(Not looking great. Do something.) OK text NONFAULTYTEXT(Looking awesome. Leave it alone) Another paragraph FAULTYTEXT(???) NONFAULTYTEXT(...)",
			RuleImpactScoresBound = new List<RuleParameterBound>()
		};


		var actor = new ActorState(ruleInstance, DateTimeOffset.Now, 1);

		var dependencies = Mock.Of<IRuleTemplateDependencies>();
		var startDate = DateTimeOffset.Now;
		var faultedStart = startDate.AddHours(1);
		var faultedEnd = startDate.AddHours(2);
		var healthyStart = startDate.AddHours(3);
		var healthyEnd = startDate.AddHours(4);

		var env = actor.RecentValues(Env.Empty.Push(), ruleInstance, dependencies);

		actor.ValidOutput(faultedStart, true, env);
		actor.ValidOutput(faultedEnd, true, env);
		insight.UpdateValues(actor, ruleInstance);

		insight.IsFaulty.Should().BeTrue();
		insight.Text.Should().Be("Some text Not looking great. More text Another paragraph ???");
		insight.RuleRecomendations.Should().Be("Reccommend Not looking great. Do something. OK text Another paragraph ???");

		env = actor.RecentValues(Env.Empty.Push(), ruleInstance, dependencies);
		actor.ValidOutput(healthyStart, false, env);
		actor.ValidOutput(healthyEnd, false, env);

		insight = new Insight(ruleInstance, actor);

		insight.IsFaulty.Should().BeFalse();
		insight.Occurrences.Last().Text.Should().Be("Some text More text Looking awesome. Another paragraph...");
		insight.FaultedCount.Should().BeGreaterThan(0);
		insight.RuleRecomendations.Should().Be("Reccommend Not looking great. Do something. OK text Another paragraph ???");

		//Lower case function
		ruleInstance = new RuleInstance()
		{
			Id = "ri",
			RuleId = "rule",
			Description = @"Some text faultytext(Not looking great.) More text nonfaultytext(Looking awesome.) Another paragraph FAULTYTEXT(???) NONFAULTYTEXT(...)",
			Recommendations = @"Reccommend FAULTYTEXT(Not looking great. Do something.) OK text NONFAULTYTEXT(Looking awesome. Leave it alone) Another paragraph FAULTYTEXT(???) NONFAULTYTEXT(...)",
			RuleImpactScoresBound = new List<RuleParameterBound>()
		};

		actor = new ActorState(ruleInstance, DateTimeOffset.Now, 1);

		env = actor.RecentValues(Env.Empty.Push(), ruleInstance, dependencies);
		actor.ValidOutput(faultedStart, true, env);
		actor.ValidOutput(faultedEnd, true, env);
		actor.ValidOutput(faultedEnd.AddMinutes(1), false, env);
		//text of previous point committed
		insight = new Insight(ruleInstance, actor);
		insight.Occurrences.ToList()[0].Text.Should().Be("Some text Not looking great. More text Another paragraph ???");
		actor.ValidOutput(faultedEnd.AddMinutes(2), true, env);
		insight = new Insight(ruleInstance, actor);
		insight.Occurrences.ToList()[0].Text.Should().Be("Some text Not looking great. More text Another paragraph ???");
		insight.Occurrences.ToList()[1].Text.Should().Be("Some text More text Looking awesome. Another paragraph...");
		insight = new Insight(ruleInstance, actor);

		insight.IsFaulty.Should().BeTrue();
		insight.Text.Should().Be("Some text Not looking great. More text Another paragraph ???");
		insight.RuleRecomendations.Should().Be("Reccommend Not looking great. Do something. OK text Another paragraph ???");

		env = actor.RecentValues(Env.Empty.Push(), ruleInstance, dependencies);
		actor.ValidOutput(healthyStart, false, env);
		actor.ValidOutput(healthyEnd, false, env);
		insight = new Insight(ruleInstance, actor);

		insight.IsFaulty.Should().BeFalse();
		insight.Occurrences.Last().Text.Should().Be("Some text More text Looking awesome. Another paragraph...");
		insight.Text.Should().Be("Some text Not looking great. More text Another paragraph ???");
		insight.RuleRecomendations.Should().Be("Reccommend Not looking great. Do something. OK text Another paragraph ???");

		//No replacement
		ruleInstance = new RuleInstance()
		{
			Id = "ri",
			RuleId = "rule",
			Description = @"Some text with no replacement.",
			Recommendations = @"Some recommendations with no replacement.",
			RuleImpactScoresBound = new List<RuleParameterBound>()
		};

		actor = new ActorState(ruleInstance, DateTimeOffset.Now, 1);

		env = actor.RecentValues(Env.Empty.Push(), ruleInstance, dependencies);
		actor.ValidOutput(faultedStart, true, env);
		actor.ValidOutput(faultedEnd, true, env);
		insight = new Insight(ruleInstance, actor);

		insight.IsFaulty.Should().BeTrue();
		insight.Text.Should().Be("Some text with no replacement.");
		insight.RuleRecomendations.Should().Be("Some recommendations with no replacement.");

		//Brackets inside
		ruleInstance = new RuleInstance()
		{
			Id = "ri",
			RuleId = "rule",
			Description = @"Some text FAULTYTEXT(Not (looking) great) NONFAULTYTEXT((Looks) great).",
			Recommendations = @"Recommendations FAULTYTEXT(Not (looking) great) NONFAULTYTEXT((Looks) great).",
			RuleImpactScoresBound = new List<RuleParameterBound>()
		};

		actor = new ActorState(ruleInstance, DateTimeOffset.Now, 1);

		env = actor.RecentValues(Env.Empty.Push(), ruleInstance, dependencies);
		actor.ValidOutput(faultedStart, true, env);
		actor.ValidOutput(faultedEnd, true, env);
		insight = new Insight(ruleInstance, actor);

		insight.IsFaulty.Should().BeTrue();
		insight.Text.Should().Be("Some text Not (looking) great.");
		insight.RuleRecomendations.Should().Be("Recommendations Not (looking) great.");

		env = actor.RecentValues(Env.Empty.Push(), ruleInstance, dependencies);
		actor.ValidOutput(healthyStart, false, env);
		actor.ValidOutput(healthyEnd, false, env);
		insight = new Insight(ruleInstance, actor);

		insight.IsFaulty.Should().BeFalse();
		insight.Occurrences.Last().Text.Should().Be("Some text (Looks) great.");
		insight.Text.Should().Be("Some text Not (looking) great.");
		insight.RuleRecomendations.Should().Be("Recommendations Not (looking) great.");

		//Brackets inside
		ruleInstance = new RuleInstance()
		{
			Id = "ri",
			RuleId = "rule",
			Description = @"Some broken text FAULTYTEXT(Not NONFAULTYTEXT(Looks great).",
			Recommendations = @"Recommendations broken text FAULTYTEXT(Not NONFAULTYTEXT(Looks great).",
			RuleImpactScoresBound = new List<RuleParameterBound>()
		};

		actor = new ActorState(ruleInstance, DateTimeOffset.Now, 1);

		env = actor.RecentValues(Env.Empty.Push(), ruleInstance, dependencies);
		actor.ValidOutput(faultedStart, true, env);
		actor.ValidOutput(faultedEnd, true, env);
		insight = new Insight(ruleInstance, actor);

		insight.IsFaulty.Should().BeTrue();
		insight.Text.Should().Be("Some broken text Not NONFAULTYTEXT(Looks great.");
		insight.RuleRecomendations.Should().Be("Recommendations broken text Not NONFAULTYTEXT(Looks great.");

		//Brackets inside
		ruleInstance = new RuleInstance()
		{
			Id = "ri",
			RuleId = "rule",
			Description = @"Some broken text FAULTYTEXT(Not NONFAULTYTEXT(Looks great).",
			Recommendations = @"Recommendations broken text NONFAULTYTEXT(Looks great)FAULTYTEXT(Not NONFAULTYTEXT(Looks great).",
			RuleImpactScoresBound = new List<RuleParameterBound>()
		};

		actor = new ActorState(ruleInstance, DateTimeOffset.Now, 1);

		env = actor.RecentValues(Env.Empty.Push(), ruleInstance, dependencies);
		actor.ValidOutput(faultedStart, false, env);
		actor.InvalidOutput(faultedEnd, "invalid text");
		insight = new Insight(ruleInstance, actor);

		insight.Text.Should().Be("Some broken text FAULTYTEXT(Not NONFAULTYTEXT(Looks great).");
		insight.RuleRecomendations.Should().Be("Recommendations broken text.");//faulty/nonfaulty area totally removed
	}
}
