using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using WillowRules.Services;
using WillowRules.Test.Bugs;

namespace WillowRules.Test.Templates;

[TestClass]
public class RuleTemplateUnchangingTests
{
	[TestMethod]
	public async Task MustFaultBetweenEarliestAndLatest()
	{
		var logger = new Mock<ILogger>().Object;

		var pointId = Guid.NewGuid().ToString();
		var twinId = "airflow-RAT-1";
		var variableName = "flow";
		var ruleInstance = new RuleInstance()
		{
			PointEntityIds = new List<NamedPoint>()
			{
				new NamedPoint(twinId, variableName, "deg", "dtmi:com:willowinc:Model;1", new List<TwinLocation>())
			},
			RuleParametersBound = new List<RuleParameterBound>()
			{
				new RuleParameterBound("sensor", new TokenExpressionVariableAccess(twinId), "sensor", "deg"),
				new RuleParameterBound("result", new TokenExpressionVariableAccess("sensor"), "result", "deg")
			},
			RuleImpactScoresBound = new List<RuleParameterBound>()
		};

		var template = new RuleTemplateUnchanging();
		var actor = new ActorState("rule", "ruleInstance", DateTimeOffset.Now, 1);
		var timeSeries = new TimeSeries(pointId, "")
		{
			DtId = twinId,
			TrendInterval = 15 * 60
		};
		timeSeries.TrendInterval = 15 * 60;  // 15 min

		var timeSeriesManager = new TimeSeriesManager(
			Mock.Of<IRepositoryTimeSeriesBuffer>(),
			Mock.Of<IRepositoryTimeSeriesMapping>(),
			Mock.Of<ITelemetryCollector>(),
			Mock.Of<IModelService>(),
			new ConsoleLogger<TimeSeriesManager>());

		timeSeriesManager.AddToBuffers(timeSeries);
		timeSeriesManager.BufferList.Count().Should().Be(1, because: "we just loaded one");
		timeSeriesManager.TryGetByTwinId(twinId, out var ts2).Should().BeTrue(because: "It's indexed by twin id");

		var reader = new RuleTemplateDependencies(ruleInstance, timeSeriesManager, Mock.Of<IEventHubService>(), new Dictionary<string, IMLRuntime>());

		reader.Count.Should().Be(1);

		DateTimeOffset zeroTime = DateTimeOffset.Now.Date;

		async Task ingest(TimeSeries ts, double offset, double value)
		{
			var timestamp = zeroTime.AddHours(offset);
			ts.AddPoint(new TimedValue(timestamp, value), applyCompression: true);
			//forcing ts to use trendinterval to make the test more predictable
			ts.EstimatedPeriod = TimeSpan.Zero;
			actor = await template.Trigger(timestamp, Env.Empty.Push(), ruleInstance, actor, reader, logger);

			Console.WriteLine();
			foreach (var output in actor.OutputValues.Points)
			{
				Console.WriteLine($"{output.StartTime:HH:mm} - {output.EndTime:HH:mm} : {(output.IsValid ? "          " : "Not valid ")}{(!output.Faulted ? "        " : "Faulted ")}{output.Text}");
			}
		}

		//run in 2 points for rule to begin execution
		await ingest(timeSeries, -11.5, 5);
		await ingest(timeSeries, -11, 5);

		actor.OutputValues.Count.Should().Be(1);
		actor.OutputValues.Points.Last().Text.Should().Be("Missing value: flow 0.0 min ago");

		// nothing yet

		await ingest(timeSeries, -10.5, 2);
		actor.OutputValues.Count.Should().Be(2);
		actor.OutputValues.Points.Last().IsValid.Should().BeTrue();
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		//Include another changing entry as the first entry's start and end is the same and will be replaced on Add of output value
		await ingest(timeSeries, -10, 2);
		actor.OutputValues.Count.Should().Be(2);
		actor.OutputValues.Points.Last().IsValid.Should().BeTrue();
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		await ingest(timeSeries, -8, 2);
		actor.OutputValues.Count.Should().Be(3);
		actor.OutputValues.Points.Last().Text.Should().Be("Missing value: flow 0.0 min ago");

		await ingest(timeSeries, -7.5, 2);
		actor.OutputValues.Count.Should().Be(4);
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		await ingest(timeSeries, -7, 2);
		actor.OutputValues.Count.Should().Be(4);
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		await ingest(timeSeries, -6.5, 2);
		actor.OutputValues.Count.Should().Be(4);
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		await ingest(timeSeries, -6, 2);
		actor.OutputValues.Count.Should().Be(4);
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		await ingest(timeSeries, -5.5, 2);
		actor.OutputValues.Count.Should().Be(4);
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		await ingest(timeSeries, -5, 2);
		actor.OutputValues.Count.Should().Be(4);
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		await ingest(timeSeries, -4.5, 2);
		actor.OutputValues.Count.Should().Be(4);
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		await ingest(timeSeries, -4, 2);
		actor.OutputValues.Count.Should().Be(5);
		actor.OutputValues.Points.Last().Faulted.Should().BeTrue();

		await ingest(timeSeries, -3.5, 2);
		actor.OutputValues.Count.Should().Be(5);
		actor.OutputValues.Points.Last().Faulted.Should().BeTrue();

		await ingest(timeSeries, -3, 2);
		actor.OutputValues.Count.Should().Be(5);
		actor.OutputValues.Points.Last().Faulted.Should().BeTrue();

		await ingest(timeSeries, -2.5, 5);
		actor.OutputValues.Count.Should().Be(6);
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		await ingest(timeSeries, -2, 7);
		actor.OutputValues.Count.Should().Be(6);
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		await ingest(timeSeries, -1.5, 7);
		actor.OutputValues.Count.Should().Be(6);
		actor.OutputValues.Points.Last().Faulted.Should().BeFalse();

		actor.TimedValues.Should().HaveCount(4);

		var first = actor.TimedValues.First();
		var second = actor.TimedValues.Skip(1).First();
		var third = actor.TimedValues.Skip(2).First();
		var fourth = actor.TimedValues.Last();

		first.Key.Should().Be("sensor");
		second.Key.Should().Be("result");
		third.Key.Should().Be("RESULT2");
		fourth.Key.Should().Be("TIME");

		first.Value.Points.Should().HaveCount(7);
		second.Value.Points.Should().HaveCount(6);
		third.Value.Points.Should().HaveCount(6);
		fourth.Value.Points.Should().HaveCount(2);

		Console.WriteLine();
		Console.WriteLine("sensor  = " + string.Join(",", first.Value.Points.Select(x => x.ValueDouble)));
		Console.WriteLine("result  = " + string.Join(",", second.Value.Points.Select(x => x.ValueDouble)));
		Console.WriteLine("RESULT2 = " + string.Join(",", third.Value.Points.Select(x => x.ValueBool)));
		Console.WriteLine("TIME = " + string.Join(",", fourth.Value.Points.Select(x => x.ValueBool)));
	}
}
