using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Sources;
using WillowRules.DTO;
using EFCore.BulkExtensions;

namespace Willow.Rules.Services;

/// <summary>
/// Validates a Twin
/// </summary>
/// <remarks>
/// Maybe doesn't belong in rules service
/// </remarks>
public interface IValidationService
{
	Task ValidateOneTwin(BasicDigitalTwinPoco basicPoco);
}

/// <summary>
/// Validates twins and creates insights when they aren't right
/// </summary>
public class ValidationService : IValidationService
{
	private readonly WillowEnvironment willowEnvironment;
	private readonly ITwinService twinService;
	private readonly ITwinSystemService twinSystemService;
	private readonly IDbContextFactory<RulesContext> contextFactory;
	private readonly ILogger<ValidationService> logger;

	/// <summary>
	/// Creates a new <see cref="ValidationService"/>
	/// </summary>
	public ValidationService(
		WillowEnvironment willowEnvironment,
		ITwinService twinService,
		ITwinSystemService twinSystemService,
		IDbContextFactory<RulesContext> contextFactory,
		ILogger<ValidationService> logger)
	{
		this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
		this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
		this.twinSystemService = twinSystemService ?? throw new ArgumentNullException(nameof(twinSystemService));
		this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Validating a twin
	/// </summary>
	/// <remarks>
	/// Doesn't really belong in rules engine, belongs in a validator utility
	/// </remarks>
	public async Task ValidateOneTwin(BasicDigitalTwinPoco basicPoco)
	{
		using (var context = await contextFactory.CreateDbContextAsync())
		{
			logger.LogTrace("Validating twin {twinId}", basicPoco.Id);

			var twin = new TwinDto(basicPoco).ApplyUnitHackFix();

			var backwardEdges = await this.twinService.GetCachedBackwardRelatedTwins(twin.Id);

			var capabilities = backwardEdges.Where(e => e.RelationshipType == "isCapabilityOf")
				.Select(e => new TwinDto(e.Destination));

			foreach (var capability in capabilities)
			{
				capability.ApplyUnitHackFix();

				if (await ReportConfigurationErrors(twin, capability))
				{
					continue;
				}
			}
		}
	}

	private async Task ReportConfigurationError(TwinDto capability)
	{
		//logger.LogWarning("Configuration error {capabilityId} cannot be a {modelId}", capability.Id, capability.modelId);
		string ruleInstanceId = capability.Id + "_Configuration";

		var insight = new Insight
		{
			Id = ruleInstanceId,
			EquipmentId = capability.Id,
			SiteId = capability.SiteId,
			LastUpdated = DateTimeOffset.Now,
			Occurrences = new List<InsightOccurrence>
			{
				//new InsightOccurrence(true, true, DateTimeOffset.Now, DateTimeOffset.Now, "Mismatch between BACNET ID and type")
			},
			ImpactScores = new List<ImpactScore>(),
			Invocations = 0,
			RuleId = "Configuration",
			RuleInstanceId = ruleInstanceId,
			PrimaryModelId = capability.ModelId,
			RuleName = "Configuration error",
			RuleTemplateName = "Configuration",
			Text = $"Mismatch between BACNET name `{capability.Id}` and type {capability.ModelId}"
		};

		using (var context = contextFactory.CreateDbContext())
		{
			await context.BulkInsertOrUpdateAsync(new List<Insight> { insight });
			await context.SaveChangesAsync();
		}
	}

	/// <summary>
	/// Reports any configuration errors found in a TwinDto
	/// </summary>
	private async Task<bool> ReportConfigurationErrors(TwinDto twin, TwinDto capability)
	{
		string capabilityModelId = capability.ModelId;

		// Detect invalid capabilities and create immediate Insights
		if (
			capability.Id.Contains("FILTRATIONLOWTEMPERATURE") && capabilityModelId.Contains(":AlarmSensor;") ||
			capability.Id.Contains("_FAULT_COUNTER") && capabilityModelId.Contains(":AlarmSensor;") ||
			(capability.Id.Contains("ALARM_TOTAL") && capabilityModelId.Contains(":AlarmSensor;")) ||
			(capability.Id.Contains("ALARM_DATE") && capabilityModelId.Contains(":AlarmSensor;")) ||
			(capability.Id.Contains("ALARM_MONTH") && capabilityModelId.Contains(":AlarmSensor;")) ||
			(capability.Id.Contains("ALARM_MINUTE") && capabilityModelId.Contains(":AlarmSensor;")) ||
			(capability.Id.Contains("ALARM_HOUR") && capabilityModelId.Contains(":AlarmSensor;")) ||
			(capability.Id.Contains("WARNING_DISPLAY") && capabilityModelId.Contains(":AlarmSensor;")) ||
			(capability.Id.Contains("ALARM_DISPLAY") && capabilityModelId.Contains(":AlarmSensor;")) ||
			(capability.Id.Contains("ALARM_LOG_DSPLY") && capabilityModelId.Contains(":AlarmSensor;")) ||
			(capability.Id.Contains("ALARM_TOTAL") && capabilityModelId.Contains(":AlarmSensor;")) ||
			(capability.Id.Contains("CO2_TRIPPOINT") && capabilityModelId.Contains(":AlarmSensor;")) ||
			(capability.Id.Contains("VOC_TRIPPOINT") && capabilityModelId.Contains(":AlarmSensor;")) ||
			(capability.Id.Contains("ALARM_CLEAR") && capabilityModelId.Contains(":AlarmSensor;")) ||
			false)
		{
			await ReportConfigurationError(capability);
			return true; // skip reporting other issues with this?
		}

		// For now, let's not do all these
		// if (capability.Id.Contains("-CFM-") && !capability.Unit.Equals("cfm", StringComparison.OrdinalIgnoreCase))
		// {
		// 	await ReportConfigurationError("CFM", "Configuration: Missing Unit: CFM", twin, $"{capability.Id} should have a unit of CFM maybe?");
		// }
		// else if (capability.ModelId.Contains("AirFlow", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(capability.Unit))
		// {
		// 	await ReportConfigurationError("CFM", "Configuration: Missing Unit: CFM", twin, $"{capability.Id} should have a unit of cfm maybe?");
		// }
		//else
		if (capability.ModelId.Contains("Temperature", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(capability.Unit))
		{
			await ReportConfigurationError("Temp", "Configuration: Missing Unit: Temperature", twin, $"{capability.Id} should have a unit of degF or degC maybe?");
		}
		else if (capability.Id.EndsWith("-SSP") && capability.ModelId.Equals(":Sensor;1"))
		{
			await ReportConfigurationError("Sensor", "Configuration: Use a specific sensor type", twin, $"{capability.Id} should not be a generic Sensor;1");
		}
		else if (capability.Id.EndsWith("-SSP") && string.IsNullOrEmpty(capability.Unit))
		{
			await ReportConfigurationError("Pressure", "Configuration: Missing Unit: Pressure", twin, $"{capability.Id} should have a unit of pressure maybe?");
		}

		// Another sample configuration error

		else if (twin.ModelId.Contains("SupplyFan") && twin.tags != null && twin.tags.Keys.Any(t => t.Equals("exhaust")))
		{
			await ReportConfigurationError("TagName",
				"Configuration: tags vs name",
				twin,
				$"{twin.Id} claims to be a SupplyFan but has tags {string.Join(",", twin.tags.Keys)}");
		}
		else if (twin.ModelId.Contains("ExhaustFan") && twin.tags != null && twin.tags.Keys.Any(t => t.Equals("supply")))
		{
			await ReportConfigurationError("TagName",
				"Configuration: tags vs name",
				twin,
				$"{twin.Id} claims to be an ExhaustFan but has tags {string.Join(",", twin.tags)}");
		}

		return false;
	}

	private async Task ReportConfigurationError(string idSuffix, string ruleName, TwinDto twin, string error)
	{
		//logger.LogWarning($"Configuration error: {error}");
		string ruleInstanceId = twin.Id + "_" + idSuffix;
		var insight = new Insight
		{
			Id = ruleInstanceId ?? throw new ArgumentNullException("ruleInstanceId"),
			EquipmentId = twin.Id,
			LastUpdated = DateTimeOffset.Now,
			Occurrences = new List<InsightOccurrence>
			{
				//new InsightOccurrence(true, true, DateTimeOffset.Now, DateTimeOffset.Now, "Mismatch between BACNET ID and type")
			},
			ImpactScores = new List<ImpactScore>(),
			Invocations = 0,
			RuleId = "Configuration",
			RuleInstanceId = ruleInstanceId,
			PrimaryModelId = twin.ModelId,
			RuleName = ruleName,
			RuleTemplateName = "Configuration",
			Text = error
		};

		using (var context = contextFactory.CreateDbContext())
		{
			await context.BulkInsertOrUpdateAsync(new List<Insight> { insight });
			await context.SaveChangesAsync();
		}
	}
}
