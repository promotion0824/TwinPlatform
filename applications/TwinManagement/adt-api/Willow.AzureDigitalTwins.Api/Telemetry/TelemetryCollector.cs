using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;
using Willow.AppContext;
using Willow.Telemetry;

namespace Willow.AzureDigitalTwins.Api.Telemetry;

/// <summary>
/// Publishes metrics to Application Insights
/// </summary>
public interface ITelemetryCollector
{
    public Meter Meter { get; }

    /// <summary>
    /// Track activity
    /// </summary>
    Activity? StartActivity(string name, ActivityKind kind, string parentId = null);
    void TrackAdtImportJobExecutionTime(long time);
    void TrackAdtTwinImportTwinCountRequested(long count);
    void TrackAdtTwinImportTwinCountSucceeded(long count);

    void TrackAdtTwinImportRelationshipCountRequested(long count);
    void TrackAdtTwinImportRelationshipCountSucceeded(long count);

    void TrackTwinDelete(long count);
    void TrackRelationshipDelete(long count);

    void TrackAdtModelCreationCount(long count);
    void TrackAdtModelDeletionCount(long count);

    void TrackTwinWithRelationshipsDeleteSuccess(long count);
    void TrackTwinWithRelationshipsDeleteFailure(long count);

    void TrackADTADXSync(long count);
    void TrackCreateMappedEntity(long count);
    void TrackCreateJobEntry(long count, KeyValuePair<string, object>[] dimensions);
    void TrackUpdateJobEntry(long count, KeyValuePair<string, object>[] dimensions);
    void TrackCreateUpdateTwinRequest(long count);
    // Track the number of update twin request that has been updated
    void TrackUpdateTwinUpdateRequestCount(long count);

    void TrackGetModelExecutionTime(long time);
    void TrackGetModelResponseExecutionTime(long time);

    void TrackGetTwinCountExecutionTime(long time);
    void TrackGetTwinCountByModelException(long count);
    void TrackDocumentUploadExecutionTime(long time);

    void TrackTimeSeriesImportCountRequested(long count);
    void TrackTimeSeriesImportCountSucceeded(long count);
    void TrackTimeSeriesImportExecutionTime(long time);

    // Track search index twin document added to the Search Index
    void TrackACSIndexAdditionsCount(long count);
    // Track search index twin document removed from the Search Index
    void TrackACSIndexDeletionsCount(long count);

    //AdxSync Message handler ProcessReceivedMessage count
    void TrackAdxSyncMessageCount(long count);

    //SyncCache MessagehHandler ProcessReceivedMessage count
    void TrackCacheSyncMessageCount(long count);

    //TwinsExport MessageHandler ProcessReceivedMessage count
    void TrackAdtToAdxExportMessageCount(long count);

    //TwinsImport MessageHandler ProcessReceivedMessage count
    void TrackImportMessageReceivedCount(long count);

    //TwinsChangeEventMessageHandlerBase picked up Service bus message for processing
    void TrackServiceBusTwinCreationEventCount(long count);
    void TrackServiceBusTwinDeletionEventCount(long count);
    void TrackServiceBusTwinUpdateEventCount(long count);
    void TrackServiceBusRelationshipCreationEventCount(long count);
    void TrackServiceBusRelationshipDeletionEventCount(long count);
    void TrackServiceBusRelationshipUpdateEventCount(long count);
    void TrackServiceBusModelCreationEventCount(long count);
    void TrackServiceBusModelDeletionEventCount(long count);

    void TrackADXModelCreationEventCount(long count);
    void TrackADXModelDeletionEventCount(long count);
    void TrackADXTwinCreationCount(long count);
    void TrackADXTwinDeletionCount(long count);
    void TrackADXRelationshipCreationCount(long count);
    void TrackADXRelationshipDeletionCount(long count);
    void TrackADXModelCreationCount(long count);
    void TrackADXModelDeletionCount(long count);
    void TrackACSTwinUpsertCount(long count);
    void TrackACSTwinDeletionCount(long count);
    void TrackDocumentBlobsCount(int count);
    void TrackDocumentGetBlobsCountExecutionTime(long time);
    void TrackDocumentBlobsSetHashTag(int count);
    void TrackDocumentBlobsNoRefDeleted(int count);
    void TrackDocumentTwinsWithUpdatedHash(int count);
    void TrackDupDocumentBlobsDeleted(int count);
    void TrackDocumentBlobsSetHashTagExecutionTime(long time);
    void TrackDocumentBlobsSetAllHashTagExecutionTime(long time);
    void TrackDocumentGroupUpdateTwinSameBlobHashExecutionTime(long time);
    void TrackDocumentDeleteBlobsNoTwinRefExecutionTime(long time);
    void TrackDocumentTwinWithNonExistingBlobRefExecutionTime(long time);
    void TrackDocumentTwinWithNonExistingBlobRefCount(int count);
    void TrackACSFlushJobExecutionTime(long ms);
    void TrackADXFlushJobExecutionTime(long ms);
    void TrackADXFlushedRowCount(long count);

    //Search docs inserted
    void TrackACSFlushInsertedRowCount(long count);

    //Search docs deleted
    void TrackACSFlushDeletedRowCount(long count);

    //Search docs yet to be processed(inserted or deleted)
    void TrackACSFlushPendingRowCount(long count);

    //Search docs failed to upload/delete, authorization error or 404 error or any other cause of failure
    void TrackACSFlushFailedRowCount(long count);
    void TrackDocumentUpdateBlobsWithoutTwinMetaExecutionTime(long ms);
    void TrackDocumentUpdateBlobsWithoutTwinMetaCount(int count);
    void TrackDocumentPointUrisToCurrentSettingExecutionTime(long ms);
    void TrackDocumentPointUrisToCurrentSettingCount(int count);
    void TrackDocumentRebuildIndexExecutionTime(long ms);
    void TrackJobUpdateExceptionCount(long count);
    void TrackJobUpdateSuccessCount(long count);
    void TrackMarkSweepDocTwinsJobExecutionTime(long ms);
    void TrackDocumentTotalTwinsUpdated(int totalTwins);
    void TrackDocumentUpdateTwinsWithSha256HashExecutionTime(long ms);
    void TrackDocumentUpdateTwinsWithSha256HashCount(int count);
    void TrackDocumentUpdateSummaryToTwinsExecutionTime(long ms);
    void TrackDocumentUpdateSummaryToTwinsCount(int count);
    void TrackDocumentWriteUpdatedTwinsToAdtExecutionTime(long ms);
    void TrackDocumentBlobNotFoundCount(int count);
    void TrackTotalAdtDocTwinsCount(int count);
    void TrackInvalidAdtDocTwinsCount(int count);
}
public class TelemetryCollector : ITelemetryCollector
{

    //app name must align with the open telemetry setup from the Willow.Telemetry packages
    //see https://github.com/WillowInc/TwinPlatform/blob/main/libraries/Willow.Telemetry.Web/OpenTelemetryConfiguration.cs

    // Activity Source must be created once per source and be reused. Different source can create their own activity source with a unique name.
    private readonly ActivitySource activitySource;
    private readonly MetricsAttributesHelper _metricsAttributesHelper;
    private Meter _meter { get; set; }
    public Meter Meter { get { return _meter; } }

    private readonly Lazy<Histogram<long>> AdtImportJobExecutionTime;
    private readonly Lazy<Counter<long>> AdtTwinImportTwinCountRequested;
    private readonly Lazy<Counter<long>> AdtTwinImportTwinCountSucceeded;

    private readonly Lazy<Counter<long>> AdtTwinImportRelationshipCountRequested;
    private readonly Lazy<Counter<long>> AdtTwinImportRelationshipCountSucceeded;

    private readonly Lazy<Counter<long>> AdtTwinDelete;
    private readonly Lazy<Counter<long>> AdtRelationshipDelete;
    private readonly Lazy<Counter<long>> AdtModelCreationCount;
    private readonly Lazy<Counter<long>> AdtModelDeletionCount;

    private readonly Lazy<Counter<long>> AdtTwinWithRelationshipsDeleteSuccess;
    private readonly Lazy<Counter<long>> AdtTwinWithRelationshipsDeleteFailure;
    private readonly Lazy<Counter<long>> AdtAdxSyncCount;
    private readonly Lazy<Counter<long>> MappedEntityCount;
    private readonly Lazy<Counter<long>> CreateJobEntityCount;
    private readonly Lazy<Counter<long>> UpdateJobEntityCount;
    private readonly Lazy<Counter<long>> UpdateTwinRequestCount;
    private readonly Lazy<Counter<long>> UpdateTwinUpdateRequestCount;
    private readonly Lazy<Histogram<long>> GetModelExecutionTime;
    private readonly Lazy<Histogram<long>> GetModelResponseExecutionTime;
    private readonly Lazy<Histogram<long>> GetTwinCountExecutionTime;
    private readonly Lazy<Counter<long>> GetTwinCountByModelExceptionCount;
    private readonly Lazy<Histogram<long>> DocumentUploadExecutionTime;
    private readonly Lazy<Histogram<long>> DocumentBlobsCountExecutionTime;
    private readonly Lazy<Histogram<long>> DocumentBlobsSetHashTagExecutionTime;
    private readonly Lazy<Histogram<long>> DocumentBlobsSetAllHashTagExecutionTime;
    private readonly Lazy<Histogram<long>> DocumentGroupUpdateTwinSameBlobHashExecutionTime;
    private readonly Lazy<Histogram<long>> DocumentDeleteBlobsNoTwinRefExecutionTime;
    private readonly Lazy<Histogram<long>> DocumentTwinWithNonExistingBlobRefExecutionTime;
    private readonly Lazy<Counter<int>> DocumentTwinWithNonExistingBlobRefCount;
    private readonly Lazy<Histogram<long>> DocumentUpdateBlobsWithoutTwinMetaExecutionTime;
    private readonly Lazy<Counter<int>> DocumentUpdateBlobsWithoutTwinMetaCount;
    private readonly Lazy<Histogram<long>> DocumentPointUrisToCurrentSettingExecutionTime;
    private readonly Lazy<Counter<int>> DocumentPointUrisToCurrentSettingCount;
    private readonly Lazy<Histogram<long>> DocumentRebuildIndexExecutionTime;
    private readonly Lazy<Histogram<long>> MarkSweepDocTwinsJobExecutionTime;
    private readonly Lazy<Counter<int>> DocumentTotalTwinsUpdatedCount;
    private readonly Lazy<Counter<int>> DocumentUpdateTwinsWithSha256HashCount;
    private readonly Lazy<Histogram<long>> DocumentUpdateTwinsWithSha256HashExecutionTime;
    private readonly Lazy<Counter<int>> DocumentUpdateSummaryToTwinsCount;
    private readonly Lazy<Histogram<long>> DocumentUpdateSummaryToTwinsExecutionTime;
    private readonly Lazy<Histogram<long>> DocumentWriteUpdatedTwinsToAdtExecutionTime;
    private readonly Lazy<Counter<int>> DocumentBlobNotFoundCount;
    private readonly Lazy<Counter<int>> TotalAdtDocTwinsCount;
    private readonly Lazy<Counter<int>> InvalidAdtDocTwinsCount;

    private readonly Lazy<Histogram<long>> ACSFlushJobExecutionTime;
    private readonly Lazy<Histogram<long>> ADXFlushJobExecutionTime;

    private readonly Lazy<Counter<long>> TimeSeriesImportCountRequested;
    private readonly Lazy<Counter<long>> TimeSeriesImportCountSucceeded;
    private readonly Lazy<Histogram<long>> TimeSeriesImportExecutionTime;

    private readonly Lazy<Counter<long>> ACSIndexAdditionsCount;
    private readonly Lazy<Counter<long>> ACSIndexDeletionsCount;

    private readonly Lazy<Counter<long>> AdxSyncMessageCount;

    private readonly Lazy<Counter<long>> CacheSyncMessageCount;
    private readonly Lazy<Counter<long>> AdtToAdxExportMessageCount;
    private readonly Lazy<Counter<long>> ImportMessageReceivedCount;

    private readonly Lazy<Counter<long>> ServiceBusTwinCreationEventCount;
    private readonly Lazy<Counter<long>> ServiceBusTwinDeletionEventCount;
    private readonly Lazy<Counter<long>> ServiceBusTwinUpdateEventCount;
    private readonly Lazy<Counter<long>> ServiceBusRelationshipCreationEventCount;
    private readonly Lazy<Counter<long>> ServiceBusRelationshipDeletionEventCount;
    private readonly Lazy<Counter<long>> ServiceBusRelationshipUpdateEventCount;
    private readonly Lazy<Counter<long>> ServiceBusModelCreationEventCount;
    private readonly Lazy<Counter<long>> ServiceBusModelDeletionEventCount;

    private readonly Lazy<Counter<long>> ADXModelCreationEventCount;
    private readonly Lazy<Counter<long>> ADXModelDeletionEventCount;

    private readonly Lazy<Counter<long>> ADXTwinCreationCount;
    private readonly Lazy<Counter<long>> ADXTwinDeletionCount;

    private readonly Lazy<Counter<long>> ADXRelationshipCreationCount;
    private readonly Lazy<Counter<long>> ADXRelationshipDeletionCount;

    private readonly Lazy<Counter<long>> ADXModelCreationCount;
    private readonly Lazy<Counter<long>> ADXModelDeletionCount;

    private readonly Lazy<Counter<long>> ACSTwinUpsertCount;
    private readonly Lazy<Counter<long>> ACSTwinDeletionCount;

    private readonly Lazy<Counter<int>> DocumentBlobsCount;
    private readonly Lazy<Counter<int>> DocumentBlobsSetHashTag;
    private readonly Lazy<Counter<int>> DocumentBlobsNoRefDeleted;
    private readonly Lazy<Counter<int>> DocumentTwinsWithUpdatedHash;
    private readonly Lazy<Counter<int>> DupDocumentBlobsDeleted;

    private readonly Lazy<Counter<long>> ADXFlushedRowCount;

    private readonly Lazy<Counter<long>> ACSFlushInsertedRowCount;
    private readonly Lazy<Counter<long>> ACSFlushDeletedRowCount;
    private readonly Lazy<Counter<long>> ACSFlushPendingRowCount;
    private readonly Lazy<Counter<long>> ACSFlushFailedRowCount;

    private readonly Lazy<Counter<long>> JobUpdateExceptionCount;
    private readonly Lazy<Counter<long>> JobUpdateSuccessCount;


    public TelemetryCollector(IMeterFactory meterFactory, IConfiguration configuration, MetricsAttributesHelper metricsAttributesHelper)
    {
        _metricsAttributesHelper = metricsAttributesHelper;

        // Create Activity Source
        if (activitySource is null)
        {
            string applicationName = configuration.GetValue<string>("ApplicationInsights:CloudRoleName") ?? Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Unknown";
            string applicationVersion = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "Unknown";
            activitySource = new ActivitySource(applicationName, applicationVersion);
        }

        // Create the meter
        if (_meter is null)
        {
            var willowContext = configuration.GetSection("WillowContext").Get<WillowContextOptions>();
            _meter = meterFactory.Create(willowContext?.MeterOptions.Name ?? "Unknown", willowContext?.MeterOptions.Version ?? "Unknown");
        }

        // Create Histogram
        this.AdtImportJobExecutionTime = CreateLazyHistogram<long>("AdtApi-AdtImportJobExecutionTime");
        this.AdtTwinImportTwinCountRequested = CreateLazyCounter<long>("AdtApi-AdtTwinImportTwinCountRequested");
        this.AdtTwinImportTwinCountSucceeded = CreateLazyCounter<long>("AdtApi-AdtTwinImportTwinCountSucceeded");
        this.AdtTwinImportRelationshipCountRequested = CreateLazyCounter<long>("AdtApi-AdtTwinImportRelationshipCountRequested");
        this.AdtTwinImportRelationshipCountSucceeded = CreateLazyCounter<long>("AdtApi-AdtTwinImportRelationshipCountSucceeded");
        this.AdtTwinDelete = CreateLazyCounter<long>("AdtApi-AdtTwinDelete");
        this.AdtRelationshipDelete = CreateLazyCounter<long>("AdtApi-AdtRelationshipDelete");
        this.AdtModelCreationCount = CreateLazyCounter<long>("AdtApi-AdtModelCreationCount");
        this.AdtModelDeletionCount = CreateLazyCounter<long>("AdtApi-AdtModelDeletionCount");

        this.AdtTwinWithRelationshipsDeleteSuccess = CreateLazyCounter<long>("AdtApi-AdtTWRDeleteSuccess");
        this.AdtTwinWithRelationshipsDeleteFailure = CreateLazyCounter<long>("AdtApi-AdtTWRDeleteFailure");
        this.AdtAdxSyncCount = CreateLazyCounter<long>("AdtApi-AdtAdxSyncCount");
        this.MappedEntityCount = CreateLazyCounter<long>("AdtApi-MappedEntityCount");
        this.UpdateTwinRequestCount = CreateLazyCounter<long>("AdtApi-UpdateTwinRequestCount");
        this.UpdateTwinUpdateRequestCount = CreateLazyCounter<long>("AdtApi-UpdateTwinUpdateRequestsCount");
        this.GetModelExecutionTime = CreateLazyHistogram<long>("AdtApi-GetModelExecutionTime");
        this.GetModelResponseExecutionTime = CreateLazyHistogram<long>("AdtApi-GetModelResponseExecutionTime");

        this.GetTwinCountExecutionTime = CreateLazyHistogram<long>("AdtApi-GetTwinCountExecutionTime");
        this.GetTwinCountByModelExceptionCount = CreateLazyCounter<long>("AdtApi-GetTwinCountByModelExceptionCount");
        this.DocumentUploadExecutionTime = CreateLazyHistogram<long>("AdtApi-GetDocumentUploadExecutionTime");
        this.DocumentBlobsCountExecutionTime = CreateLazyHistogram<long>("AdtApi-GetDocumentBlobsCountExecutionTime");
        this.DocumentBlobsSetHashTagExecutionTime = CreateLazyHistogram<long>("AdtApi-DocumentBlobsSetHashTagExecutionTime");
        this.DocumentBlobsSetAllHashTagExecutionTime = CreateLazyHistogram<long>("AdtApi-DocumentBlobsSetAllHashTagExecutionTime");
        this.DocumentGroupUpdateTwinSameBlobHashExecutionTime = CreateLazyHistogram<long>("AdtApi-DocumentGroupUpdateTwinSameBlobHashExecutionTime");
        this.DocumentDeleteBlobsNoTwinRefExecutionTime = CreateLazyHistogram<long>("AdtApi-DocumentDeleteBlobsNoTwinRefExecutionTime");
        this.DocumentTwinWithNonExistingBlobRefExecutionTime = CreateLazyHistogram<long>("AdtApi-DocumentDeleteTwinWithNonExistingBlobRefExecutionTime");
        this.DocumentTwinWithNonExistingBlobRefCount = CreateLazyCounter<int>("AdtApi-DocumentDeleteTwinWithNonExistingBlobRefCount");
        this.DocumentUpdateBlobsWithoutTwinMetaExecutionTime = CreateLazyHistogram<long>("AdtApi-DocumentUpdateBlobsWithoutTwinMetaExecutionTime");
        this.DocumentUpdateBlobsWithoutTwinMetaCount = CreateLazyCounter<int>("AdtApi-DocumentUpdateBlobsWithoutTwinMetaCount");
        this.DocumentPointUrisToCurrentSettingExecutionTime = CreateLazyHistogram<long>("AdtApi-DocumentPointUrisToCurrentSettingExecutionTime");
        this.DocumentPointUrisToCurrentSettingCount = CreateLazyCounter<int>("AdtApi-DocumentPointUrisToCurrentSettingCount");
        this.DocumentRebuildIndexExecutionTime = CreateLazyHistogram<long>("AdtApi-DocumentRebuildIndexExecutionTime");
        this.DocumentTotalTwinsUpdatedCount = CreateLazyCounter<int>("AdtApi-DocumentTotalTwinsUpdatedCount");
        this.DocumentUpdateTwinsWithSha256HashExecutionTime = CreateLazyHistogram<long>("AdtApi-DocumentUpdateTwinsWithSha256HashExecutionTime");
        this.DocumentUpdateTwinsWithSha256HashCount = CreateLazyCounter<int>("AdtApi-DocumentUpdateTwinsWithSha256HashCount");
        this.MarkSweepDocTwinsJobExecutionTime = CreateLazyHistogram<long>("AdtApi-MarkSweepDocTwinsJobExecutionTime");
        this.DocumentUpdateSummaryToTwinsExecutionTime = CreateLazyHistogram<long>("AdtApi-DocumentUpdateSummaryToTwinsExecutionTime");
        this.DocumentUpdateSummaryToTwinsCount = CreateLazyCounter<int>("AdtApi-DocumentUpdateSummaryToTwinsCount");
        this.DocumentWriteUpdatedTwinsToAdtExecutionTime = CreateLazyHistogram<long>("AdtApi-DocumentWriteUpdatedTwinsToAdtExecutionTime");
        this.DocumentBlobNotFoundCount = CreateLazyCounter<int>("AdtApi-DocumentBlobNotFoundCount");
        this.TotalAdtDocTwinsCount = CreateLazyCounter<int>("AdtApi-TotalAdtDocTwinsCount");
        this.InvalidAdtDocTwinsCount = CreateLazyCounter<int>("AdtApi-InvalidAdtDocTwinsCount");

        this.TimeSeriesImportCountRequested = CreateLazyCounter<long>("AdtApi-TimeSeriesImportCountRequested");
        this.TimeSeriesImportCountSucceeded = CreateLazyCounter<long>("AdtApi-TimeSeriesImportCountSucceeded");
        this.TimeSeriesImportExecutionTime = CreateLazyHistogram<long>("AdtApi-TimeSeriesImportExecutionTime");

        this.ACSIndexAdditionsCount = CreateLazyCounter<long>("AdtApi-ACSIndexAdditionsCount");
        this.ACSIndexDeletionsCount = CreateLazyCounter<long>("AdtApi-ACSIndexDeletionsCount");

        this.AdxSyncMessageCount = CreateLazyCounter<long>("AdtApi-AdxSyncMessageCount");
        this.CacheSyncMessageCount = CreateLazyCounter<long>("AdtApi-CacheSyncMessageCount");
        this.AdtToAdxExportMessageCount = CreateLazyCounter<long>("AdtApi-AdtToAdxExportMessageCount");
        this.ImportMessageReceivedCount = CreateLazyCounter<long>("AdtApi-ImportMessageReceivedCount");


        this.ServiceBusTwinCreationEventCount = CreateLazyCounter<long>("AdtApi-ServiceBusTwinCreationEventCount");
        this.ServiceBusTwinDeletionEventCount = CreateLazyCounter<long>("AdtApi-ServiceBusTwinDeletionEventCount");
        this.ServiceBusTwinUpdateEventCount = CreateLazyCounter<long>("AdtApi-ServiceBusTwinUpdateEventCount");
        this.ServiceBusRelationshipCreationEventCount = CreateLazyCounter<long>("AdtApi-ServiceBusRelationshipCreationEventCount");
        this.ServiceBusRelationshipDeletionEventCount = CreateLazyCounter<long>("AdtApi-ServiceBusRelationshipDeletionEventCount");
        this.ServiceBusRelationshipUpdateEventCount = CreateLazyCounter<long>("AdtApi-ServiceBusRelationshipUpdateEventCount");
        this.ServiceBusModelCreationEventCount = CreateLazyCounter<long>("AdtApi-ServiceBusModelCreationEventCount");
        this.ServiceBusModelDeletionEventCount = CreateLazyCounter<long>("AdtApi-ServiceBusModelDeletionEventCount");

        this.ADXModelCreationEventCount = CreateLazyCounter<long>("AdtApi-ADXModelCreationEventCount");
        this.ADXModelDeletionEventCount = CreateLazyCounter<long>("AdtApi-ADXModelDeletionEventCount");


        this.ADXTwinCreationCount = CreateLazyCounter<long>("AdtApi-ADXTwinCreationCount");
        this.ADXTwinDeletionCount = CreateLazyCounter<long>("AdtApi-ADXTwinDeletionCount");

        this.ADXRelationshipCreationCount = CreateLazyCounter<long>("AdtApi-ADXRelationshipCreationCount");
        this.ADXRelationshipDeletionCount = CreateLazyCounter<long>("AdtApi-ADXRelationshipDeletionCount");

        this.ADXModelCreationCount = CreateLazyCounter<long>("AdtApi-ADXModelCreationCount");
        this.ADXModelDeletionCount = CreateLazyCounter<long>("AdtApi-ADXModelDeletionCount");

        this.ACSTwinUpsertCount = CreateLazyCounter<long>("AdtApi-ACSTwinUpsertCount");
        this.ACSTwinDeletionCount = CreateLazyCounter<long>("AdtApi-ACSTwinDeletionCount");

        this.DocumentBlobsCount = CreateLazyCounter<int>("AdtApi-DocumentBlobsCount");
        this.DocumentBlobsSetHashTag = CreateLazyCounter<int>("AdtApi-DocumentBlobsSetHashTag");
        this.DocumentBlobsNoRefDeleted = CreateLazyCounter<int>("AdtApi-DocumentBlobsNoRefDeleted");
        this.DocumentTwinsWithUpdatedHash = CreateLazyCounter<int>("AdtApi-DocumentTwinsWithUpdatedHash");
        this.DupDocumentBlobsDeleted = CreateLazyCounter<int>("AdtApi-DupDocumentBlobsDeleted");

        this.ACSFlushJobExecutionTime = CreateLazyHistogram<long>("AdtApi-ACSFlushJobExecutionTime");
        this.ADXFlushJobExecutionTime = CreateLazyHistogram<long>("AdtApi-ADXFlushJobExecutionTime");

        this.ACSFlushInsertedRowCount = CreateLazyCounter<long>("AdtApi-ACSFlushInsertedRowCount");
        this.ADXFlushedRowCount = CreateLazyCounter<long>("AdtApi-ADXFlushedRowCount");

        this.ACSFlushInsertedRowCount = CreateLazyCounter<long>("AdtApi-ACSFlushInsertedRowCount");
        this.ACSFlushDeletedRowCount = CreateLazyCounter<long>("AdtApi-ACSFlushDeletedRowCount");
        this.ACSFlushPendingRowCount = CreateLazyCounter<long>("AdtApi-ACSFlushPendingRowCount");
        this.ACSFlushFailedRowCount = CreateLazyCounter<long>("AdtApi-ACSFlushFailedRowCount");
        this.CreateJobEntityCount = CreateLazyCounter<long>("AdtApi-CreateJobEntityCount");
        this.UpdateJobEntityCount = CreateLazyCounter<long>("AdtApi-UpdateJobEntityCount");

        this.JobUpdateExceptionCount = CreateLazyCounter<long>("AdtApi-JobUpdateExceptionCount");
        this.JobUpdateSuccessCount = CreateLazyCounter<long>("AdtApi-JobUpdateSuccessCount");
    }
    public void TrackGetModelExecutionTime(long time) => GetModelExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());

    public void TrackGetModelResponseExecutionTime(long time) => GetModelResponseExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());

    public void TrackGetTwinCountExecutionTime(long time) => GetTwinCountExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());

    public void TrackGetTwinCountByModelException(long count) => GetTwinCountByModelExceptionCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackAdtImportJobExecutionTime(long time) => AdtImportJobExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());

    public void TrackAdtTwinImportTwinCountRequested(long count) => AdtTwinImportTwinCountRequested.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackAdtTwinImportTwinCountSucceeded(long count) => AdtTwinImportTwinCountSucceeded.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackAdtTwinImportRelationshipCountRequested(long count) => AdtTwinImportRelationshipCountRequested.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackAdtTwinImportRelationshipCountSucceeded(long count) => AdtTwinImportRelationshipCountSucceeded.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackTwinDelete(long count) => AdtTwinDelete.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackRelationshipDelete(long count) => AdtRelationshipDelete.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackAdtModelCreationCount(long count) => AdtModelCreationCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackAdtModelDeletionCount(long count) => AdtModelDeletionCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackTwinWithRelationshipsDeleteSuccess(long count) => AdtTwinWithRelationshipsDeleteSuccess.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackTwinWithRelationshipsDeleteFailure(long count) => AdtTwinWithRelationshipsDeleteFailure.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackADTADXSync(long count) => AdtAdxSyncCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackCreateMappedEntity(long count) => MappedEntityCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackCreateJobEntry(long count, KeyValuePair<string, object>[] dimensions)
    {
        CreateJobEntityCount.Value.Add(count, dimensions);
    }

    public void TrackUpdateJobEntry(long count, KeyValuePair<string, object>[] dimensions)
    {
        UpdateJobEntityCount.Value.Add(count, dimensions);
    }
    public void TrackDocumentUploadExecutionTime(long time) => DocumentUploadExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());
    public void TrackCreateUpdateTwinRequest(long count) => UpdateTwinRequestCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackUpdateTwinUpdateRequestCount(long count) => UpdateTwinUpdateRequestCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackDocumentGetBlobsCountExecutionTime(long time) => DocumentBlobsCountExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());

    public void TrackDocumentBlobsSetHashTagExecutionTime(long time) => DocumentBlobsSetHashTagExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());

    public void TrackDocumentBlobsSetAllHashTagExecutionTime(long time) => DocumentBlobsSetAllHashTagExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());

    public void TrackDocumentGroupUpdateTwinSameBlobHashExecutionTime(long time) => DocumentGroupUpdateTwinSameBlobHashExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());

    public void TrackDocumentDeleteBlobsNoTwinRefExecutionTime(long time) => DocumentDeleteBlobsNoTwinRefExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());

    public void TrackDocumentTwinWithNonExistingBlobRefExecutionTime(long time) => DocumentTwinWithNonExistingBlobRefExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());

    public void TrackDocumentTwinWithNonExistingBlobRefCount(int count) => DocumentTwinWithNonExistingBlobRefCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackDocumentUpdateBlobsWithoutTwinMetaExecutionTime(long ms) => DocumentUpdateBlobsWithoutTwinMetaExecutionTime.Value.Record(ms, _metricsAttributesHelper.GetValues());

    public void TrackDocumentUpdateBlobsWithoutTwinMetaCount(int count) => DocumentUpdateBlobsWithoutTwinMetaCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackDocumentPointUrisToCurrentSettingExecutionTime(long ms) => DocumentPointUrisToCurrentSettingExecutionTime.Value.Record(ms, _metricsAttributesHelper.GetValues());

    public void TrackDocumentPointUrisToCurrentSettingCount(int count) => DocumentPointUrisToCurrentSettingCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackDocumentRebuildIndexExecutionTime(long ms) => DocumentRebuildIndexExecutionTime.Value.Record(ms, _metricsAttributesHelper.GetValues());

    public void TrackDocumentUpdateTwinsWithSha256HashExecutionTime(long ms) => DocumentUpdateTwinsWithSha256HashExecutionTime.Value.Record(ms, _metricsAttributesHelper.GetValues());

    public void TrackDocumentUpdateTwinsWithSha256HashCount(int count) => DocumentUpdateTwinsWithSha256HashCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackDocumentUpdateSummaryToTwinsExecutionTime(long ms) => DocumentUpdateSummaryToTwinsExecutionTime.Value.Record(ms, _metricsAttributesHelper.GetValues());

    public void TrackDocumentUpdateSummaryToTwinsCount(int count) => DocumentUpdateSummaryToTwinsCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackMarkSweepDocTwinsJobExecutionTime(long ms) => MarkSweepDocTwinsJobExecutionTime.Value.Record(ms, _metricsAttributesHelper.GetValues());

    public void TrackDocumentTotalTwinsUpdated(int totalTwins) => DocumentTotalTwinsUpdatedCount.Value.Add(totalTwins, _metricsAttributesHelper.GetValues());

    public void TrackDocumentWriteUpdatedTwinsToAdtExecutionTime(long ms) => DocumentWriteUpdatedTwinsToAdtExecutionTime.Value.Record(ms, _metricsAttributesHelper.GetValues());

    public void TrackDocumentBlobNotFoundCount(int count) => DocumentBlobNotFoundCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackTotalAdtDocTwinsCount(int count) => TotalAdtDocTwinsCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackInvalidAdtDocTwinsCount(int count)=> InvalidAdtDocTwinsCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackTimeSeriesImportCountRequested(long count) => TimeSeriesImportCountRequested.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackTimeSeriesImportCountSucceeded(long count) => TimeSeriesImportCountSucceeded.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackTimeSeriesImportExecutionTime(long time) => TimeSeriesImportExecutionTime.Value.Record(time, _metricsAttributesHelper.GetValues());

    // Track search index twin document added to the Search Index
    public void TrackACSIndexAdditionsCount(long count) => ACSIndexAdditionsCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    // Track search index twin document removed from the Search Index
    public void TrackACSIndexDeletionsCount(long count) => ACSIndexDeletionsCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    //AdxSync Message handler ProcessReceivedMessage count
    public void TrackAdxSyncMessageCount(long count) => AdxSyncMessageCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    //SyncCache MessagehHandler ProcessReceivedMessage count
    public void TrackCacheSyncMessageCount(long count) => CacheSyncMessageCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    //TwinsExport MessageHandler ProcessReceivedMessage count
    public void TrackAdtToAdxExportMessageCount(long count) => AdtToAdxExportMessageCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    //TwinsImport MessageHandler ProcessReceivedMessage count
    public void TrackImportMessageReceivedCount(long count) => ImportMessageReceivedCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    //TwinsChangeEventMessageHandlerBase picked up Service bus message for processing
    public void TrackServiceBusTwinCreationEventCount(long count) => ServiceBusTwinCreationEventCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackServiceBusTwinDeletionEventCount(long count) => ServiceBusTwinDeletionEventCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackServiceBusTwinUpdateEventCount(long count) => ServiceBusTwinUpdateEventCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackServiceBusRelationshipCreationEventCount(long count) => ServiceBusRelationshipCreationEventCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackServiceBusRelationshipDeletionEventCount(long count) => ServiceBusRelationshipDeletionEventCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackServiceBusRelationshipUpdateEventCount(long count) => ServiceBusRelationshipUpdateEventCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackServiceBusModelCreationEventCount(long count) => ServiceBusModelCreationEventCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackServiceBusModelDeletionEventCount(long count) => ServiceBusModelDeletionEventCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackDocumentBlobsSetHashTag(int count) => DocumentBlobsSetHashTag.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackDocumentBlobsNoRefDeleted(int count) => DocumentBlobsNoRefDeleted.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackDocumentTwinsWithUpdatedHash(int count) => DocumentTwinsWithUpdatedHash.Value.Add(count, _metricsAttributesHelper.GetValues());

    #region ExportService sent Service bus message for updating ADX Models table

    public void TrackADXModelCreationEventCount(long count) => ADXModelCreationEventCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackADXModelDeletionEventCount(long count) => ADXModelDeletionEventCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    #endregion

    #region ADXSyncMessageHandler received and processing Service bus message for updating ADX tables
    public void TrackADXTwinCreationCount(long count) => ADXTwinCreationCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackADXTwinDeletionCount(long count) => ADXTwinDeletionCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackADXRelationshipCreationCount(long count) => ADXRelationshipCreationCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackADXRelationshipDeletionCount(long count) => ADXRelationshipDeletionCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackADXModelCreationCount(long count) => ADXModelCreationCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackADXModelDeletionCount(long count) => ADXModelDeletionCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    #endregion

    public void TrackACSTwinUpsertCount(long count) => ACSTwinUpsertCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackACSTwinDeletionCount(long count) => ACSTwinDeletionCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackDocumentBlobsCount(int count) => DocumentBlobsCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackDupDocumentBlobsDeleted(int count) => DupDocumentBlobsDeleted.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackACSFlushJobExecutionTime(long ms) => ACSFlushJobExecutionTime.Value.Record(ms, _metricsAttributesHelper.GetValues());
    public void TrackADXFlushJobExecutionTime(long ms) => ADXFlushJobExecutionTime.Value.Record(ms, _metricsAttributesHelper.GetValues());

    public void TrackACSFlushInsertedRowCount(long count) => ACSFlushInsertedRowCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackACSFlushDeletedRowCount(long count) => ACSFlushDeletedRowCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackACSFlushPendingRowCount(long count) => ACSFlushPendingRowCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackACSFlushFailedRowCount(long count) => ACSFlushFailedRowCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackADXFlushedRowCount(long count) => ADXFlushedRowCount.Value.Add(count, _metricsAttributesHelper.GetValues());

    public void TrackJobUpdateExceptionCount(long count) => JobUpdateExceptionCount.Value.Add(count, _metricsAttributesHelper.GetValues());
    public void TrackJobUpdateSuccessCount(long count) => JobUpdateSuccessCount.Value.Add(count, _metricsAttributesHelper.GetValues());


    public Activity? StartActivity(string name, ActivityKind kind, string parentId = null)
    {
        return activitySource.StartActivity(name, kind, Activity.Current?.Id ?? parentId);
    }

    private Lazy<Histogram<T>> CreateLazyHistogram<T>(string name) where T : struct
    {
        return new Lazy<Histogram<T>>(() => _meter.CreateHistogram<T>(name));
    }

    private Lazy<Counter<T>> CreateLazyCounter<T>(string name) where T : struct
    {
        return new Lazy<Counter<T>>(() => _meter.CreateCounter<T>(name));
    }
}
