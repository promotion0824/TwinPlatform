using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using Moq;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Services;
using WillowRules.Services;

namespace RulesEngine.Benchmarks;

[JsonExporterAttribute.Full]
public class TemplateBenchmarks
{
	private static TimeSeries CreateTimeSeries(params TimedValue[] points)
	{
		var timeSeries = new TimeSeries("test", "");

		foreach (var point in points)
		{
			timeSeries.AddPoint(point, false);
		}

		return timeSeries;
	}

	[Benchmark]
	public async Task Template_Trigger_RuleTemplateAnyHysteresis()
	{
		var ruleInstance = HysteresisRuleInstance;
		var now = DateTimeOffset.Now;
		var actorState = new ActorState(ruleInstance.RuleId, ruleInstance.Id, now, 1);
		var logger = new Mock<ILogger>().Object;

		actorState.TimedValues[Fields.Result.Id] = CreateTimeSeries(new TimedValue(DateTimeOffset.Now, true));

		var trendTime = DateTimeOffset.Now.AddMinutes(-1);
		var trendValue = 1.0d;
		var changedValue = new TimedValue(trendTime, trendValue);
		var hoursField = Fields.OverHowManyHours.With(1);
		var resultField = Fields.Result.With("sensor > setpoint + 5 | sensor < setpoint - 5");
		var template = new RuleTemplateAnyHysteresis("F", new RuleUIElementCollection(hoursField, resultField));
		var timeSeries = new Dictionary<string, TimeSeries>()
		{
			[ruleInstance.OutputTrendId] = CreateTimeSeries(new TimedValue(DateTimeOffset.Now.AddHours(-2), true), changedValue)
		};
		var timeSeriesManager = Mock.Of<ITimeSeriesManager>();

		var reader = CreateTimeSeriesReader(ruleInstance, timeSeriesManager);
		timeSeries[ruleInstance.OutputTrendId].AddPoint(changedValue, true);

		await template.Trigger(now, Env.Empty.Push(), ruleInstance, actorState, reader, logger);
	}

	private static RuleInstance HysteresisRuleInstance = new RuleInstance()
	{
		Id = "1",
		RuleId = "2",
		OutputTrendId = "3",
		PointEntityIds = new List<NamedPoint>(),
		RuleParametersBound = new List<RuleParameterBound>()
		{
			new RuleParameterBound(Fields.Result.Id, Parser.Deserialize("([air_flow_sp_ratio] > 1.1) & [damper_cmd] < 0.05"), "3", "none")
		},
		Status = RuleInstanceStatus.Valid
	};

	[Benchmark]
	public async Task Template_Trigger_RuleTemplateAnyFault()
	{
		var ruleInstance = AnyFaultRuleInstance;
		var now = DateTimeOffset.Now;
		var actorState = new ActorState(ruleInstance.RuleId, ruleInstance.Id, now, 1);
		var logger = new Mock<ILogger>().Object;

		actorState.TimedValues[Fields.Result.Id] = CreateTimeSeries(new TimedValue(DateTimeOffset.Now.AddMinutes(-5), false), new TimedValue(DateTimeOffset.Now, false));

		actorState.TimedValues["RESULT"] = CreateTimeSeries(new TimedValue(DateTimeOffset.Now.AddHours(-2), true));

		var trendTime = DateTimeOffset.Now.AddMinutes(-1);
		var trendValue = 1.0d;
		var changedValue = new TimedValue(trendTime, trendValue);
		var hoursField = Fields.OverHowManyHours.With(1);
		var template = new RuleTemplateAnyFault(hoursField);
		var timeSeries = new Dictionary<string, TimeSeries>()
		{
			[ruleInstance.OutputTrendId] = CreateTimeSeries(new TimedValue(DateTimeOffset.Now.AddHours(-2), true), changedValue)
		};
		var timeSeriesManager = Mock.Of<ITimeSeriesManager>();

		var reader = CreateTimeSeriesReader(ruleInstance, timeSeriesManager);
		timeSeries[ruleInstance.OutputTrendId].AddPoint(changedValue, true);

		await template.Trigger(now, Env.Empty.Push(), ruleInstance, actorState, reader, logger);
	}

	private IRuleTemplateDependencies CreateTimeSeriesReader(RuleInstance ruleInstance, ITimeSeriesManager manager)
	{
		return new RuleTemplateDependencies(ruleInstance, manager, Mock.Of<IEventHubService>(), new Dictionary<string, IMLRuntime>());
	}

	private static RuleInstance AnyFaultRuleInstance = new RuleInstance()
	{
		Id = "1",
		RuleId = "2",
		OutputTrendId = "3",
		PointEntityIds = new List<NamedPoint>()
		{
			new("id", "somevalue", "", "", new List < TwinLocation >())
		},
		RuleParametersBound = new List<RuleParameterBound>()
		{
			new RuleParameterBound(Fields.Result.Id, Parser.Deserialize("([air_flow_sp_ratio] > 1.1) & [damper_cmd] < 0.05"), "3", "none")
		},
		Status = RuleInstanceStatus.Valid
	};

	[Benchmark]
	public async Task Template_Trigger_RuleTemplateCalculatedPoint()
	{
		var logger = new Mock<ILogger>().Object;

		var ruleInstance = CalculatedPointRuleInstance;
		var now = DateTimeOffset.Now;
		var actorState = new ActorState(ruleInstance.RuleId, ruleInstance.Id, now, 1);
		var trendTime = DateTimeOffset.Now.AddMinutes(-1);
		var trendValue = 5.0d;
		var changedValue = new TimedValue(trendTime, trendValue);
		var template = new RuleTemplateCalculatedPoint();
		var timeSeries = new Dictionary<string, TimeSeries>()
		{
			[ruleInstance.OutputTrendId] = CreateTimeSeries(new TimedValue(DateTimeOffset.Now.AddHours(-2), 4), changedValue)
		};
		var timeSeriesManager = Mock.Of<ITimeSeriesManager>();

		var reader = CreateTimeSeriesReader(ruleInstance, timeSeriesManager);
		timeSeries[ruleInstance.OutputTrendId].AddPoint(changedValue, true);

		await template.Trigger(now, Env.Empty.Push(), ruleInstance, actorState, reader, logger);
	}

	private static RuleInstance CalculatedPointRuleInstance = new RuleInstance()
	{
		Id = "1",
		RuleId = "2",
		OutputTrendId = "3",
		PointEntityIds = new List<NamedPoint>()
		{
			new("id", "somevalue", "", "", new List < TwinLocation >())
		},
		RuleParametersBound = new List<RuleParameterBound>()
		{
			new RuleParameterBound(Fields.Result.Id, Parser.Deserialize("10"), "3", "none")
		},
		Status = RuleInstanceStatus.Valid
	};

	[Benchmark]
	public async Task Template_Trigger_RuleTemplateFrequency()
	{
		var logger = new Mock<ILogger>().Object;

		var ruleInstance = FrequencyRuleInstance;
		var now = DateTimeOffset.Now;
		var actorState = new ActorState(ruleInstance.RuleId, ruleInstance.Id, now, 1);

		actorState.TimedValues[Fields.Result.Id] = CreateTimeSeries(
			new TimedValue(DateTimeOffset.Now.AddMinutes(-5), true),
			new TimedValue(DateTimeOffset.Now.AddMinutes(-2), false),
			new TimedValue(DateTimeOffset.Now, true));

		var trendTime = DateTimeOffset.Now.AddMinutes(-1);
		var trendValue = 5.0d;
		var hoursField = Fields.OverHowManyHours.With(1);
		var changedValue = new TimedValue(trendTime, trendValue);
		var template = new RuleTemplateFrequency(hoursField);
		var timeSeries = new Dictionary<string, TimeSeries>()
		{
			[ruleInstance.OutputTrendId] = CreateTimeSeries(
				new TimedValue(DateTimeOffset.Now.AddHours(-2), true),
				changedValue)
		};
		var timeSeriesManager = Mock.Of<ITimeSeriesManager>();

		var reader = CreateTimeSeriesReader(ruleInstance, timeSeriesManager);
		timeSeries[ruleInstance.OutputTrendId].AddPoint(changedValue, true);

		await template.Trigger(now, Env.Empty.Push(), ruleInstance, actorState, reader, logger);
	}

	private static RuleInstance FrequencyRuleInstance = new RuleInstance()
	{
		Id = "1",
		RuleId = "2",
		OutputTrendId = "3",
		PointEntityIds = new List<NamedPoint>()
		{
			new("id", "somevalue", "", "", new List < TwinLocation >())
		},
		RuleImpactScoresBound = new List<RuleParameterBound>()
		{
			new RuleParameterBound(Fields.CostImpact.Name, Parser.Deserialize("10"), Fields.CostImpact.Id, "none"),
			new RuleParameterBound(Fields.ReliabilityImpact.Name, Parser.Deserialize("10"), Fields.ReliabilityImpact.Id, "none"),
			new RuleParameterBound(Fields.ComfortImpact.Name, Parser.Deserialize("10"), Fields.ComfortImpact.Id, "none")
		},
		Status = RuleInstanceStatus.Valid
	};

	[Benchmark]
	public async Task Template_Trigger_RuleTemplateUnchanging()
	{
		var logger = new Mock<ILogger>().Object;

		var ruleInstance = UnchangingRuleInstance;
		var now = DateTimeOffset.Now;
		var actorState = new ActorState(ruleInstance.RuleId, ruleInstance.Id, now, 1);

		var trendTime = DateTimeOffset.Now.AddMinutes(-1);
		var trendValue = 5.0d;
		var hoursField = Fields.OverHowManyHours.With(0);
		var changedValue = new TimedValue(trendTime, trendValue);
		var template = new RuleTemplateUnchanging(hoursField);
		var timeSeries = new Dictionary<string, TimeSeries>()
		{
			[ruleInstance.OutputTrendId] = CreateTimeSeries(
				new TimedValue(DateTimeOffset.Now.AddHours(-2.1d), true),
				new TimedValue(DateTimeOffset.Now.AddMinutes(-30), true),
				changedValue)
		};
		var timeSeriesManager = Mock.Of<ITimeSeriesManager>();

		var reader = CreateTimeSeriesReader(ruleInstance, timeSeriesManager);
		timeSeries[ruleInstance.OutputTrendId].AddPoint(changedValue, true);

		await template.Trigger(now, Env.Empty.Push(), ruleInstance, actorState, reader, logger);
	}

	private static RuleInstance UnchangingRuleInstance = new RuleInstance()
	{
		Id = "1",
		RuleId = "2",
		OutputTrendId = "3",
		PointEntityIds = new List<NamedPoint>()
		{
			new("id", "somevalue", "", "", new List < TwinLocation >())
		},
		RuleImpactScoresBound = new List<RuleParameterBound>()
		{
			new RuleParameterBound(Fields.CostImpact.Name, Parser.Deserialize("10"), "3", "none"),
			new RuleParameterBound(Fields.ReliabilityImpact.Name, Parser.Deserialize("10"), "3", "none"),
			new RuleParameterBound(Fields.ComfortImpact.Name, Parser.Deserialize("10"), "3", "none")
		},
		Status = RuleInstanceStatus.Valid
	};
}
