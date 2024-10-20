using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Builders;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Async;
using Willow.Model.Jobs;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;

/// <summary>
/// Twin Model Migration Job Implementation.
/// </summary>
/// <param name="logger">ILogger Instance.</param>
/// <param name="jobsService">IJobService implementation.</param>
/// <param name="modelParser">Model Parser.</param>
/// <param name="twinReader">IAzureDigitalTwinReader implementation.</param>
/// <param name="twinWriter">Twin Writer.</param>
/// <param name="telemetryCollector">Telemetry Collector.</param>
public class TwinModelMigrationJob(
    ILogger<TwinModelMigrationJob> logger,
    IJobsService jobsService,
    IAzureDigitalTwinModelParser modelParser,
    IAzureDigitalTwinReader twinReader,
IAzureDigitalTwinWriter twinWriter,
    ITelemetryCollector telemetryCollector) : BaseTwinJob<TwinModelMigrationJobOption>(logger, jobsService, telemetryCollector), IJobProcessor
{
    public override string JobType => "TwinsApi";
    public override string JobSubType => "ModelMigration";


    private readonly List<string> jobOutput = [];

    /// <summary>
    /// Method to execute the background jobs
    /// </summary>
    /// <param name="jobsEntry"> Jobs Entry.</param>
    /// <param name="jobExecutionContext"> Job Execution Context.</param>
    /// <param name="cancellationToken">Cancelation Token</param>
    /// <returns>Awaitable task</returns>
    public async Task ExecuteJobAsync(JobsEntry jobsEntry, JobExecutionContext jobExecutionContext, CancellationToken cancellationToken)
    {
        // Retrieve Job Configuration
        var option = jobsEntry.GetCustomData<TwinModelMigrationJobOption>();

        // Clear output and error console
        jobOutput.Clear();

        using (logger.BeginScope($"{nameof(TwinModelMigrationJob)} with Id :{jobsEntry.JobId}"))
        {
            var migrationRules = option.MigrationRules;

            foreach (var migrationRule in migrationRules)
            {

                string parentModel = migrationRule.Key;
                var relationshipRules = migrationRule.Value;
                LogOutput(jobsEntry, $"Parent Model:{parentModel}");

                int overallTwinProcessCount = 0;
                foreach (var relationRule in relationshipRules)
                {
                    var relationNamesWithDirection = relationRule.Key;
                    LogOutput(jobsEntry, $"Relationship:{relationNamesWithDirection}");

                    overallTwinProcessCount += await ProcessMigrationRule(jobsEntry, parentModel, relationNamesWithDirection, relationRule.Value);
                }

                LogOutput(jobsEntry, $"Overall Twin Processed Count: {overallTwinProcessCount}");
            }
        }
    }

    private async Task<int> ProcessMigrationRule(JobsEntry job, string parentModel, string relationshipNamesWithDirection, ModelRule[] modelRule)
    {
        // Loop through all the twins of the parent model
        var twinQuery = QueryBuilder.Create().SelectAll().FromDigitalTwins().Where().WithAnyModel([parentModel], "", exact: false);

        var asyncPagedTwins = twinReader.QueryAsync<BasicDigitalTwin>(twinQuery.GetQuery());
        int processedTwinCount = 0;
        await foreach (var parentTwin in asyncPagedTwins)
        {
            LogOutput(job, $"Processing Twin Id: {parentTwin.Id}");

            try
            {
                var relatedTwins = await GetSpecificRelatedTwins(parentTwin.Id, relationshipNamesWithDirection);

                foreach (var childModelRule in modelRule)
                {
                    await ApplyModelRule(job, parentTwin, relatedTwins, childModelRule);
                }

                if (processedTwinCount % 10 == 0)
                {
                    await UpdateJobStatus(job, AsyncJobStatus.Processing);
                }
            }
            catch (Exception ex)
            {
                UpdateErrorLog(job, $"Failed to process twin {parentTwin.Id}. Exception:{ex.Message}", ex);
            }
            ++processedTwinCount;
            job.ProgressCurrentCount = processedTwinCount;
            job.ProgressTotalCount = processedTwinCount;
        }
        LogOutput(job, $"Processed twin count: {processedTwinCount}.");
        await UpdateJobStatus(job, AsyncJobStatus.Processing);
        return processedTwinCount;
    }

    private async Task ApplyModelRule(JobsEntry job, BasicDigitalTwin parentTwin, IEnumerable<BasicDigitalTwin> relatedTwins, ModelRule rule)
    {
        var currentRule = $"{rule.OldModelId} - {rule.NewModelId}";

        try
        {
            // Check for the Child Twin
            var matchingChildTwins = relatedTwins.Where(w => w.Metadata.ModelId == rule.OldModelId);

            // If no twin or more than one matching child twins for a model found, skip current rule execution for the twin
            if (!matchingChildTwins.Any())
            {
                return;
            }
            else if (matchingChildTwins.Count() > 1)
            {
                LogOutput(job, $"Failed. Multiple twins found for Model {rule.OldModelId} with Ids({string.Join(',', matchingChildTwins.Select(s => s.Id))}). Skipping rule.");
                return;
            }

            LogOutput(job, $"Applying Rule: {currentRule}.");
            var childTwinToMigrate = matchingChildTwins.First();

            // Migrate existing Twin to New Model
            childTwinToMigrate.Metadata.ModelId = rule.NewModelId;

            var newModelInterfaceInfo = modelParser.GetInterfaceInfo(rule.NewModelId);

            if (newModelInterfaceInfo is null)
            {
                LogOutput(job, $"Failed. Model {rule.NewModelId} does not exist in the Ontology. Skipping rule {currentRule}.");
                return;
            }

            // Remove the properties from the existing twin that does not exist in the new model Ontology
            if (newModelInterfaceInfo.Properties != null)
            {
                // only if it is defined in the new model ontology
                foreach (var property in childTwinToMigrate.Contents)
                {
                    if (!newModelInterfaceInfo.Properties.ContainsKey(property.Key))
                    {
                        childTwinToMigrate.Contents.Remove(property);
                    }
                }
            }

            // validate create relationship rule before saving the twin changes
            var missingRelatedModels = rule.CreateRels.Where(r => !relatedTwins.Any(t => t.Metadata.ModelId == r.TargetModelId));
            if (missingRelatedModels.Any())
            {
                LogOutput(job, $" Failed Rule {currentRule} Create Relationship Validation. No matching twin found for Model(s):{string.Join(',', missingRelatedModels.Select(s => s.TargetModelId))}");
                return;
            }

            // save the twin changes
            childTwinToMigrate = await twinWriter.CreateOrReplaceDigitalTwinAsync(childTwinToMigrate);

            // Process Additional Relationships from the payload
            foreach (var relRule in rule.CreateRels)
            {
                var targetModelIdMatchingTwins = relatedTwins.Union([parentTwin]).Where(m => m.Metadata.ModelId == relRule.TargetModelId);
                if (!targetModelIdMatchingTwins.Any())
                {
                    LogOutput(job, $" Failed to create relationship {relRule.RelationshipName}. No matching twin found for Model:{relRule.TargetModelId}");
                    return;
                }
                if (targetModelIdMatchingTwins.Count() != 1)
                {
                    LogOutput(job, $" Failed to create relationship {relRule.RelationshipName}. Multiple twins found for Model:{relRule.TargetModelId}");
                    return;
                }

                var newRelationship = new BasicRelationship()
                {
                    Name = relRule.RelationshipName,
                };
                if (relRule.RelationshipDirection == RelationshipDirection.Incoming)
                {
                    newRelationship.SourceId = targetModelIdMatchingTwins.First().Id;
                    newRelationship.TargetId = childTwinToMigrate.Id;
                }
                else if (relRule.RelationshipDirection == RelationshipDirection.Outgoing)
                {
                    newRelationship.TargetId = targetModelIdMatchingTwins.First().Id;
                    newRelationship.SourceId = childTwinToMigrate.Id;
                }
                // Create Relationship
                newRelationship = await twinWriter.CreateOrReplaceRelationshipAsync(newRelationship);
                LogOutput(job, $" Established additional relationship {relRule.RelationshipName} with Id:{newRelationship.Id}");
            }
        }
        catch (Exception ex)
        {
            UpdateErrorLog(job, $" Failed to process Rule: {currentRule} for Twin Id {parentTwin.Id}. Exception:{ex.Message} ", ex);
        }

    }

    private async Task<IEnumerable<BasicDigitalTwin>> GetSpecificRelatedTwins(string twinId, string relationshipCollection)
    {
        List<BasicDigitalTwin> relatedTwins = [];

        foreach (var relationshipWithDirection in relationshipCollection.Split(','))
        {
            // Check if relationship name with Outgoing keyword, ex. Outgoing.IsCapabilityOf
            var isOutgoing = relationshipWithDirection.StartsWith("Outgoing.");
            var relationshipParts = relationshipWithDirection.Split('.');
            var relationshipName = (relationshipParts.Length > 2 ? relationshipParts[1] : relationshipParts[0]).Trim();

            if (isOutgoing)
            {
                // Get all the outgoing relationships for the twin
                var outgoingRelationships = await twinReader.GetTwinRelationshipsAsync(twinId, relationshipName);
                var outgoingRelationshipIds = outgoingRelationships.Where(w => w.Name == relationshipName).Select(s => s.TargetId).ToList();
                //Continue if no incoming relationships
                if (outgoingRelationshipIds.Count == 0)
                {
                    continue;
                }

                // Get all the twins based on the outgoing relationships
                var getRelatedTwinsTask = await twinReader.GetTwinsByIdsAsync(outgoingRelationshipIds);
                relatedTwins.AddRange(getRelatedTwinsTask.Content);
            }
            else
            {
                // Get all the incoming relationships for the twin
                var incomingRelationships = await twinReader.GetIncomingRelationshipsAsync(twinId);
                var incomingRelationshipIds = incomingRelationships.Where(w => string.Equals(w.Name, relationshipName, StringComparison.InvariantCultureIgnoreCase)).Select(s => s.SourceId).ToList();
                //Continue if no incoming relationships
                if (incomingRelationshipIds.Count == 0)
                {
                    continue;
                }

                // Get all the twins based on the incoming  relationships
                var getRelatedTwinsTask = await twinReader.GetTwinsByIdsAsync(incomingRelationshipIds);
                relatedTwins.AddRange(getRelatedTwinsTask.Content);
            }

        }
        return relatedTwins;
    }

    private void LogOutput(JobsEntry job, string message)
    {
        jobOutput.Add(message);
        job.JobsEntryDetail.OutputsJson = JsonSerializer.Serialize(jobOutput);
    }

    private void UpdateErrorLog(JobsEntry job, string message, Exception exception = null)
    {
        job.JobsEntryDetail.ErrorsJson += "\n" + message;
        if (exception is null)
        {
            logger.LogInformation(message);
        }
        else
        {
            logger.LogError(exception, message);
        }
    }
}
