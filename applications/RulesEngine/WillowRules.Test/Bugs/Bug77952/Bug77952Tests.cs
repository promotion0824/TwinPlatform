using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Services;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug77952Tests
{
	[TestMethod]
	public void Bug_77952_Can_Doownload_RuleInstance_DebugInfo()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(4),
			Fields.PercentageOfTime.With(0.2),
			Fields.OverHowManyHours.With(2)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Cooling Setpoing", "CLG_SP", "OPTION([dtmi:com:willowinc:EffectiveCoolingZoneAirTemperatureSetpoint;1],[dtmi:com:willowinc:OccupiedCoolingZoneAirTemperatureSetpoint;1])"),
			new RuleParameter("Heating Setpoint", "HTG_SP", "OPTION([dtmi:com:willowinc:EffectiveHeatingZoneAirTemperatureSetpoint;1],[dtmi:com:willowinc:OccupiedHeatingZoneAirTemperatureSetpoint;1])"),
			new RuleParameter("Expression", "result", "CLG_SP - HTG_SP > 0")
		};

		var rule = new Rule()
		{
			Id = "heating-and-cooling-zone-air-temperature-setpoints-too-close",
			PrimaryModelId = "dtmi:com:willowinc:FanCoilUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements,
			Description = "FAULTYTEXT(Faulted constant {HTG_SP})NONFAULTYTEXT(It's VALID)"
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:FanCoilUnit;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:OccupiedCoolingZoneAirTemperatureSetpoint;1", "sensor1", "70da35f9-33c4-49a0-9e1d-f81816b1b8a9");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:OccupiedHeatingZoneAirTemperatureSetpoint;1", "sensor2", "c7c0437f-b4ea-4a93-9589-a8b406749366");
		var sensors = new List<TwinOverride>()
		{
			sensor1,
			sensor2
		};

		var bugHelper = new BugHelper("Bug77952", "Timeseries.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, equipment, sensors);

		insight.IsFaulty.Should().BeTrue();

		insight.Occurrences.Last().Text.Should().Be("Faulted constant 70.00");

		var harness = bugHelper.harness!;


		var adxService = new FileBasedADXService(BugHelper.GetFullDataPath("Bug77952", "Timeseries.csv"), Mock.Of<ILogger<FileBasedADXService>>());

		var fileService = new FileService(
			Mock.Of<IMemoryCache>(),
			harness.repositoryRules,
			harness.repositoryInsight,
			harness.repositoryRuleMetadata,
			harness.repositoryRuleInstances,
			harness.repositoryActorState,
			harness.repositoryTimeSeriesMapping,
			harness.repositoryTimeSeriesBuffer,
			harness.repositoryCalculatedPoint,
			harness.repositoryGlobalVariable,
			harness.repositoryCommand,
			harness.repositoryMLModel,
			harness.mlService,
			harness.dataCacheFactory,
			harness.twinSystemService,
			harness.twinService,
			adxService,
			MockObjects.WillowEnvironment,
			Mock.Of<ILogger<FileService>>());

		var filepath = fileService.ZipRuleInstanceDebugInfo(bugHelper.RuleInstance!.Id, true, DateTime.Now, DateTime.Now).Result;

		harness = new ProcessorTestHarness();

		fileService = new FileService(
			Mock.Of<IMemoryCache>(),
			harness.repositoryRules,
			harness.repositoryInsight,
			harness.repositoryRuleMetadata,
			harness.repositoryRuleInstances,
			harness.repositoryActorState,
			harness.repositoryTimeSeriesMapping,
			harness.repositoryTimeSeriesBuffer,
			harness.repositoryCalculatedPoint,
			harness.repositoryGlobalVariable,
			harness.repositoryCommand,
			harness.repositoryMLModel,
			harness.mlService,
			harness.dataCacheFactory,
			harness.twinSystemService,
			harness.twinService,
			adxService,
			MockObjects.WillowEnvironment,
			Mock.Of<ILogger<FileService>>());

		fileService.UploadRuleInstanceDebugInfo(filepath).Wait();

		harness.repositoryRules.Data.Count.Should().Be(1);
		harness.repositoryInsight.Data.Count.Should().Be(1);
		harness.repositoryRuleInstances.Data.Count.Should().Be(1);
		harness.repositoryActorState.Data.Count.Should().Be(1);
		harness.repositoryTimeSeriesBuffer.Data.Count.Should().Be(2);
		harness.repositoryTimeSeriesMapping.Data.Count.Should().Be(2);
	}
}
