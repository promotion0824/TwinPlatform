using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.Model.Async;
using Willow.Model.Mapping;
using Willow.Model.TimeSeries;

namespace Willow.AzureDigitalTwins.Api.Extensions;

public static class UnifiedJobExtensions
{
    // Job Types
    public static readonly string MTIJobType = "MTI";
    public static readonly string TLMJobType = "TLM";
    public static readonly string TLMImportJobType = "TLM Import";
    public static readonly string TLMDeleteJobType = "TLM Delete";

    // Job SubTypes
    public static readonly string TimeSeriesJobSubType = "TimeSeries";
    public static readonly string ImportModelJobSubType = "Models";
    public static readonly string TwinsValidationJobSubType = "TwinsValidation";


    private static readonly JsonSerializerOptions jobSerializationOption = new()
    {
        Converters = { new StringToIntConverter() },
        ReferenceHandler = ReferenceHandler.Preserve
    };

    public static MtiAsyncJob ToMtiAsyncJob(this JobsEntry jobsEntry)
    {
        var customData = jobsEntry.JobsEntryDetail?.CustomData is not null
            ? SerializationExtensions.DeserializeAnonymousType(jobsEntry.JobsEntryDetail?.CustomData, new { BuildingId = "", ConnectorId = "" })
            : null;
        var _ = Enum.TryParse(jobsEntry.JobSubtype, out MtiAsyncJobType jobType);

        var job = new MtiAsyncJob(jobsEntry.JobId)
        {
            JobType = jobType,
            UserId = jobsEntry.UserId,
            Details = new AsyncJobDetails
            {
                StartTime = jobsEntry.ProcessingStartTime?.DateTime,
                EndTime = jobsEntry.ProcessingEndTime?.DateTime,
                Status = jobsEntry.Status,
                StatusMessage = jobsEntry.ProgressStatusMessage
            },
            CreateTime = jobsEntry.TimeCreated.DateTime,
            LastUpdateTime = jobsEntry.TimeLastUpdated.DateTime,
            UserData = jobsEntry.UserMessage,
            BuildingId = customData?.BuildingId,
            ConnectorId = customData?.ConnectorId
        };

        return job;
    }

    public static JobsEntry ToUnifiedJob(this MtiAsyncJob job)
    {
        var unifiedJob = new JobsEntry
        {
            JobId = job.JobId ?? null,
            JobType = MTIJobType,
            JobSubtype = job.JobType.ToString(),
            UserId = job.UserId ?? null,
            Status = job.Details.Status,
            TimeCreated = job.CreateTime,
            TimeLastUpdated = job.LastUpdateTime is not null ? new DateTimeOffset(job.LastUpdateTime.Value) : DateTimeOffset.UtcNow,
            ProcessingStartTime = job.Details.StartTime,
            ProcessingEndTime = job.Details.EndTime,
            UserMessage = job.UserData,
            ProgressStatusMessage = job.Details.StatusMessage,
            JobsEntryDetail = job.BuildingId is not null || job.ConnectorId is not null
                ? new JobsEntryDetail()
                {
                    CustomData = JsonSerializer.Serialize(new { job.BuildingId, job.ConnectorId })
                }
                : null
        };

        return unifiedJob;

    }

    public static TwinsValidationJob ToTwinsValidationJob(this JobsEntry jobsEntry, bool includeDetail = false)
    {
        var input = includeDetail && jobsEntry.JobsEntryDetail?.InputsJson is not null
            ? SerializationExtensions.DeserializeAnonymousType(
                jobsEntry.JobsEntryDetail?.InputsJson,
                new { ModelIds = new List<string>(), ExactModelMatch = false, LocationId = "" })
            : null;

        var customData = includeDetail && jobsEntry.JobsEntryDetail?.CustomData is not null
            ? JsonSerializer.Deserialize<TwinValidationJobSummaryDetails>(jobsEntry.JobsEntryDetail?.CustomData)
            : null;

        return new TwinsValidationJob()
        {
            JobId = jobsEntry.JobId ?? null,
            UserId = jobsEntry.UserId,
            CreateTime = jobsEntry.TimeCreated.DateTime,
            LastUpdateTime = jobsEntry.TimeLastUpdated.DateTime,
            StartTime = jobsEntry.ProcessingStartTime?.DateTime,
            EndTime = jobsEntry.ProcessingEndTime?.DateTime,
            Details = new AsyncJobDetails
            {
                Status = jobsEntry.Status
            },
            ModelIds = input?.ModelIds ?? [],
            ExactModelMatch = input?.ExactModelMatch,
            LocationId = input?.LocationId,
            SummaryDetails = customData ?? new TwinValidationJobSummaryDetails()
        };
    }

    public static JobsEntry ToUnifiedJob(this TwinsValidationJob job, bool includeDetail = false)
    {
        return new JobsEntry
        {
            JobId = job.JobId ?? null,
            JobType = TLMJobType,
            JobSubtype = TwinsValidationJobSubType,
            UserId = job.UserId,
            TimeCreated = job.CreateTime,
            TimeLastUpdated = job.LastUpdateTime is not null ? job.LastUpdateTime.Value : DateTime.UtcNow,
            ProcessingStartTime = job.StartTime ?? null,
            ProcessingEndTime = job.EndTime ?? null,
            Status = job.Details.Status,
            JobsEntryDetail = includeDetail
                ? new JobsEntryDetail()
                {
                    InputsJson = job.ModelIds is not null || job.ExactModelMatch is not null || job.LocationId is not null
                        ? JsonSerializer.Serialize(new { job.ModelIds, job.ExactModelMatch, job.LocationId })
                        : null,
                    CustomData = job.SummaryDetails is not null ? JsonSerializer.Serialize(job.SummaryDetails) : null
                }
                : null
        };
    }

    public static TimeSeriesImportJob ToTimeSeriesJob(this JobsEntry jobsEntry)
    {
        var importJob = new TimeSeriesImportJob(jobsEntry.JobId)
        {
            RequestPath = !string.IsNullOrEmpty(jobsEntry.SourceResourceUri) ? jobsEntry.SourceResourceUri : null,
            EntitiesError = DeserializeToDictionary<string>(jobsEntry.JobsEntryDetail?.ErrorsJson),
            isSasUrlImport = !string.IsNullOrEmpty(jobsEntry.SourceResourceUri),
            ProcessedEntities = jobsEntry.ProgressCurrentCount != null ? jobsEntry.ProgressCurrentCount.Value : 0,
            TotalEntities = jobsEntry.ProgressTotalCount != null ? jobsEntry.ProgressTotalCount.Value : 0,
            Details = new AsyncJobDetails
            {
                StartTime = jobsEntry.ProcessingStartTime != null ? jobsEntry.ProcessingStartTime.Value.DateTime : null,
                EndTime = jobsEntry.ProcessingEndTime != null ? jobsEntry.ProcessingEndTime.Value.DateTime : null,
                Status = jobsEntry.Status,
                StatusMessage = jobsEntry.ProgressStatusMessage
            },
            CreateTime = jobsEntry.TimeCreated.DateTime,
            LastUpdateTime = jobsEntry.TimeLastUpdated.DateTime,
            UserId = jobsEntry.UserId,
            UserData = jobsEntry.UserMessage
        };
        return importJob;
    }

    public static JobsEntry ToUnifiedJob(this TimeSeriesImportJob importJob, JobsEntryDetail jed = null, bool includeDetail = false)
    {
        var unifiedJob = new JobsEntry()
        {
            JobId = importJob.JobId,
            JobType = TLMJobType,
            JobSubtype = TimeSeriesJobSubType,
            UserId = importJob.UserId,
            UserMessage = importJob.UserData,
            SourceResourceUri = importJob.RequestPath,
            Status = importJob.Details.Status,
            TimeCreated = importJob.CreateTime,
            TimeLastUpdated = new DateTimeOffset(importJob.LastUpdateTime.Value),
            JobsEntryDetail = includeDetail ? new JobsEntryDetail { JobId = importJob.JobId, InputsJson = jed != null && jed.InputsJson != null ? jed.InputsJson : null, ErrorsJson = System.Text.Json.JsonSerializer.Serialize(importJob.EntitiesError) } : null,
            ProcessingStartTime = importJob.Details.StartTime,
            ProcessingEndTime = importJob.Details.EndTime,
            ProgressStatusMessage = importJob.Details.StatusMessage,
            ProgressCurrentCount = importJob.ProcessedEntities,
            ProgressTotalCount = importJob.TotalEntities
        };
        return unifiedJob;
    }

    public static T GetCustomData<T>(this JobsEntry job)
    {
        var serializedData = JsonSerializer.Deserialize<T>(job.JobsEntryDetail.CustomData);
        return serializedData;
    }

    public static Dictionary<string, T> DeserializeToDictionary<T>(string errorJson)
    {
        if (string.IsNullOrWhiteSpace(errorJson)) return [];

        return JsonSerializer.Deserialize<Dictionary<string, T>>(errorJson, jobSerializationOption);
    }

    public static string ToJson<T>(T data)
    {
        if (data is null) return string.Empty;

        return JsonSerializer.Serialize(data, jobSerializationOption);
    }
}
public class StringToIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Parse the string value to an integer
        if (reader.TokenType == JsonTokenType.String && int.TryParse(reader.GetString(), out int result))
        {
            return result;
        }

        // Return default value if parsing fails
        return default;
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        // Serialize the integer value as a string
        writer.WriteStringValue(value.ToString());
    }
}
