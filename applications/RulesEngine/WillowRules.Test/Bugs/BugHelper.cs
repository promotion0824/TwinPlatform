using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using TimeZoneConverter;
using Willow.Rules;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Services;

namespace WillowRules.Test.Bugs;

/// <summary>
/// A class to re-use methods around insight, rule and timeseries assertions and investigations
/// </summary>
internal class BugHelper
{
	private readonly string bugName;
	private readonly string sourceFile;
	public List<ActorState> Actors;
	public ActorState? Actor;
	public RuleInstance? RuleInstance;
	public ProcessorTestHarness harness;
	private bool assertOutputFiles;

	public BugHelper(
		string bugName,
		string sourceFile,
		bool assertOutputFiles = false)
	{
		this.bugName = bugName;
		this.sourceFile = sourceFile;
		this.assertOutputFiles = assertOutputFiles;
		harness = new ProcessorTestHarness();
		Actors = new List<ActorState>();
	}

	/// <summary>
	/// Runs the provided csv exported time series data from disk and executes the data against the provided rule template and returns an Insight as the result
	/// </summary>
	/// <returns></returns>
	public Insight GenerateInsightForPoint(
		Rule rule,
		string sensorModelId,
		string sensorTrendId,
		DateTime? startDate = null,
        DateTime? endDate = null,
        string? testcaseName = null,
		int maxDaysToKeep = 1000,
		int maxOutputDaysToKeep = 1000,
		int maxOutputvaluesToKeep = 250,
		bool assertSimulation = true,
		bool enableCompression = true,
		bool outputImagesOnly = true)
	{
		return GenerateInsightForPoint(
			rule,
			new TwinOverride(rule.PrimaryModelId, "equipment"),
			new List<TwinOverride>() { new TwinOverride(sensorModelId, sensorTrendId, sensorTrendId) },
			startDate: startDate,
            endDate: endDate,
            testcaseName: testcaseName,
			maxDaysToKeep: maxDaysToKeep,
			maxOutputvaluesToKeep: maxOutputvaluesToKeep,
			maxOutputDaysToKeep: maxOutputDaysToKeep,
			assertSimulation: assertSimulation,
			enableCompression: enableCompression,
			outputImagesOnly: outputImagesOnly);
	}

	/// <summary>
	/// Runs the provided csv exported time series data from disk and executes the data against the provided rule template and returns an Insight as the result
	/// </summary>
	/// <returns></returns>
	public Insight GenerateInsightForPoint(
		Rule rule,
		TwinOverride equipment,
		List<TwinOverride> cacheOverrides,
		DateTime? startDate = null,
		DateTime? endDate = null,
		string? testcaseName = null,
		int maxDaysToKeep = 1000,
		int maxOutputDaysToKeep = 1000,
		int maxOutputvaluesToKeep = 250,
		bool assertSimulation = true,
		bool outputImagesOnly = true,
		bool limitUntracked = true,
		bool enableCompression = true,
		bool optimizeExpressions = true,
		bool applyLimits = true)
	{
		cacheOverrides = cacheOverrides ?? new List<TwinOverride>();

		(var data, var filePath) = CreateData(bugName, sourceFile);

		rule = GetOrCreateRule(rule);

		if (RuleInstance is null)
		{
			harness.OverrideCaches(rule, equipment, cacheOverrides);

			RuleInstance = harness.GenerateRuleInstances(optimizeExpressions: optimizeExpressions).Result[0];
		}

        if (startDate is null)
        {
            startDate = data.First().SourceTimestamp.AddDays(-1);
        }

        (var insightsList, Actors, var timeSeriesList) = harness.ExecuteRules(
			filePath,
			startDate,
			endDate,
			assertSimulation: assertSimulation,
			maxDaysToKeep: maxDaysToKeep,
			maxOutputDaysToKeep: maxOutputDaysToKeep,
			maxOutputvaluesToKeep: maxOutputvaluesToKeep,
			enableCompression: enableCompression,
			limitUntracked: limitUntracked,
			applyLimits: applyLimits)
			.Result;

		Actor = Actors.First();

		AssertActor(Actor);

		var insight = insightsList.FirstOrDefault();

		return insight!;
	}


	private static void AssertActor(ActorState actor)
	{
		// Check values are in order
		foreach (var list in actor.TimedValues)
		{
			list.Value.CheckTimeSeriesIsInOrder().Should().BeTrue();
		}

		//if (actor.OutputValues.Points.Count > 1)
		//{
		//	actor.OutputValues.Points.HasConsecutiveCondition((previous, current) =>
		//		(previous.Faulted == false && previous.IsValid == true) == (current.Faulted == false && current.IsValid == true)).Should().BeFalse();
		//}

		actor.OutputValues.IsInOrder().Should().BeTrue();

		actor.HasOverlappingOutputValues().Should().BeFalse();
	}

	private Rule GetOrCreateRule(Rule? rule)
	{
		rule = rule ?? new Rule()
		{
			Id = $"Rule_{Guid.NewGuid()}",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit; 1",
			Parameters = new List<RuleParameter>()
			{
				new RuleParameter("Sensor", "result", "OPTION([dtmi:com:willowinc:Sensor;1])")
			}
		};

		return rule;
	}

	/// <summary>
	/// TODO: Use the ADXFileBasedService when PR completed
	/// </summary>
	/// <returns></returns>
	public static (List<RawData> data, string filePath) CreateData(string bugName, string fileName)
	{
		var filePath = GetFullDataPath(bugName, fileName);

		var adxService = new FileBasedADXService(filePath, Mock.Of<ILogger<FileBasedADXService>>());

		return (adxService.RunRawQuery(DateTime.MinValue, DateTime.MaxValue).ToListAsync().Result, filePath);
	}

	public static string GetFullDataPath(string subfolder, string fileName)
	{
		return Path.GetFullPath(Path.Combine("..", "..", "..", "Bugs", subfolder, fileName));
	}
}
