using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Willow.Api.Common.Extensions;
using Willow.AzureDataExplorer.Builders;
using Willow.AzureDataExplorer.Helpers;
using Willow.AzureDataExplorer.Infra;
using Willow.AzureDataExplorer.Ingest;
using Willow.AzureDataExplorer.Model;
using Willow.AzureDataExplorer.Options;
using Willow.AzureDataExplorer.Query;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Services.Hosted;
using Willow.AzureDigitalTwins.DataQuality.Api.Services;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.DataQuality.Model.Capability;
using Willow.DataQuality.Model.Validation;
using Willow.DataQuality.Model.ValidationResults;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.DataQuality;

public interface IDataQualityAdxService
{
    Task InitDQAdxSettingsAsync();
    Task IngestDataToValidationTableAsync(IEnumerable<ValidationResults> data);
    Task ProcessTwinsValidationJobAsync(TwinsValidationJob twinsValidationJob);
    Task<TwinsValidationJob> QueueTwinsValidationProcessAsync(string userId,
        IEnumerable<string> modelIds = null,
        bool? exactModelMatch = null,
        string locationId = null,
        DateTimeOffset? startCheckTime = null,
        DateTimeOffset? endCheckTime = null);
    Task<IEnumerable<TwinsValidationJob>> FindValidationJobsAsync(string jobId = null,
        AsyncJobStatus? status = null,
        string userId = null,
        DateTime? from = null,
        DateTime? to = null,
        bool fullDetails = true);
    Task IngestCapabilityStatusToValidationTableAsync(IEnumerable<CapabilityStatusDto> data);
    Task<Page<ValidationResults>> GetTwinDataQualityResultsByIdAsync(string[] ids, bool idsAreModels = false,
                        string[] resultSources = null,
                        Result[] resultTypes = null,
                        CheckType[] checkTypes = null,
                        DateTimeOffset? startDate = null,
                        DateTimeOffset? endDate = null,
                        string searchString = null,
                        string locationId = null,
                        int pageSize = 100,
                        string continuationToken = null);
    Task<TwinsValidationJob> GetLatestValidationJobAsync(AsyncJobStatus? status = null);
    Task DeleteValidationJobAsync(string[] jobId);
    bool IsAnyJobRunning();
}

public class DataQualityAdxService : IDataQualityAdxService
{
    private readonly IJobsService _jobsService;
    private readonly IAzureDataExplorerInfra _azureDataExplorerInfra;
    private readonly IAzureDataExplorerIngest _azureDataExplorerIngest;
    private readonly IAzureDataExplorerQuery _azureDataExplorerQuery;
    private readonly IMapper _mapper;
    private readonly ILogger<DataQualityAdxService> _logger;
    private readonly IAdxService _adxService;
    private readonly string _adxDatabase;
    private readonly string _validationFolder;
    private readonly IDQRuleService _dQRuleService;
    private readonly IAzureDigitalTwinCacheProvider _azureDigitalTwinCacheProvider;
    private readonly string _runningValidationKey;
    private readonly IMemoryCache _memoryCache;
    private readonly IBackgroundTaskQueue<TwinsValidationJob> _twinsValidationTaskQueue;

    private const string ValidationTable = "ValidationResults";
    public const string ValidationFunctionName = "ActiveValidationResults";
    const string ResultSourceStaticDataQuality = "StaticDataQuality";
    const string OrphanedTwin = "OrphanedTwin";
    public const string ValidationMaterializedViewName = "SimplifyValidationResults";

    public DataQualityAdxService(IAzureDataExplorerInfra azureDataExplorerInfra,
        ILogger<DataQualityAdxService> logger,
        IOptions<AzureDataExplorerOptions> azureDataExplorerOptions,
        IAzureDataExplorerIngest azureDataExplorerIngest,
        IAzureDataExplorerQuery azureDataExplorerQuery,
        IMapper mapper,
        IMemoryCache memoryCache,
        AzureDigitalTwinsSettings azureDigitalTwinsSettings,
        IAdxService adxService,
        IDQRuleService dQRuleService,
        IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider,
        IJobsService jobsService,
        IBackgroundTaskQueue<TwinsValidationJob> twinsValidationTaskQueue
        )
    {
        _azureDataExplorerInfra = azureDataExplorerInfra;
        _azureDataExplorerIngest = azureDataExplorerIngest;
        _azureDataExplorerQuery = azureDataExplorerQuery;
        _logger = logger;
        _mapper = mapper;
        _adxDatabase = azureDataExplorerOptions != null ? azureDataExplorerOptions.Value.DatabaseName : null;
        _validationFolder = $"{azureDigitalTwinsSettings.Instance.InstanceUri.Host}/validation";
        _adxService = adxService;
        _dQRuleService = dQRuleService;
        _azureDigitalTwinCacheProvider = azureDigitalTwinCacheProvider;
        _memoryCache = memoryCache;
        _runningValidationKey = $"{azureDigitalTwinsSettings.Instance.InstanceUri.Host}.runningvalidation";
        _jobsService = jobsService;
        _twinsValidationTaskQueue = twinsValidationTaskQueue;
    }

    public async Task IngestDataToValidationTableAsync(IEnumerable<ValidationResults> data)
    {
        var propertyNames = GetDefaultColumns().Select(n => n.Item1).ToList();
        List<ValidationResultsAdxDto> vrDto = new List<ValidationResultsAdxDto>();
        foreach (var d in data)
        {
            try
            {
                var vr = _mapper.Map<ValidationResultsAdxDto>(d);
                vrDto.Add(vr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception mapping ValidationResultsAdxDto IngestDataToValidationTableAsync(): TwinId: {Id}", d.TwinDtId);
            }
        }
        await _azureDataExplorerIngest.IngestFromDataReaderAsync<ValidationResultsAdxDto>(_adxDatabase, ValidationTable, propertyNames, vrDto);
    }

    public async Task IngestCapabilityStatusToValidationTableAsync(IEnumerable<CapabilityStatusDto> data)
    {
        var propertyNames = GetDefaultColumns().Select(n => n.Item1).ToList();
        var capabilityStatus = new List<ValidationResultsAdxDto>();
        foreach (var d in data)
        {
            try
            {
                var oneCapabilityStatus = _mapper.Map<ValidationResultsAdxDto>(d);
                capabilityStatus.Add(oneCapabilityStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception mapping Capability status IngestCapabilityStatusToValidationTableAsync() TwinId: {Id}", d.TwinId);
            }
        }

        await _azureDataExplorerIngest.IngestFromDataReaderAsync<ValidationResultsAdxDto>(_adxDatabase, ValidationTable, propertyNames, capabilityStatus);
    }

    //Note: ValidationResultsDto properties and columns list in GetDefaultColumns() with their order need to be in sync
    // for this method to function correctly
    public async Task<Page<ValidationResults>> GetTwinDataQualityResultsByIdAsync(string[] ids,
                    bool idsAreModels = false,
                    string[] resultSources = null,
                    Result[] resultTypes = null,
                    CheckType[] checkTypes = null,
                    DateTimeOffset? startDate = null,
                    DateTimeOffset? endDate = null,
                    string searchString = null,
                    string locationId = null,
                    int pageSize = 100,
                    string continuationToken = null)
    {
        PageQueryResult result = null;
        var twins = new List<ValidationResultsAdxDto>();
        var validationResults = new List<ValidationResults>();

        var dtoProperties = typeof(ValidationResultsAdxDto).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        // These values never change from call-to-call -- they are fixed at compile-time, so we really only need to check
        // this once at service startup. We could allow the DTO to have extra properties, as long as it has the properties we require.
        if (dtoProperties.Length != GetDefaultColumns().Count)
        {
            throw new Exception("Count mismatch between ValidationResults table schema and ValidationResultsDto properties");
        }

        if (!string.IsNullOrEmpty(continuationToken))
        {
            var pagedQuery = System.Text.Json.JsonSerializer.Deserialize<PageQueryResult>(continuationToken);

            result = await _azureDataExplorerQuery.GetPageQueryAsync(_adxDatabase, pagedQuery, pageSize);
        }
        else
        {
            IQueryWhere oneQuery = null;
            var orderbyParams = new List<OrderByParam>
                {
                    new OrderByParam("TwinDtId", Order.asc),
                    new OrderByParam("RuleId", Order.asc),
                    new OrderByParam("todatetime(RunInfo.CheckTime)", Order.asc)
                };
            IQueryWhere BuildQuery(string tableOrFunctionName)
            {
                var query = QueryBuilder.Create().Select(tableOrFunctionName);

                if (ids.Length > 0)
                {
                    query.Where();
                    if (!idsAreModels)
                        (query as IQueryFilterGroup).PropertyIn("TwinDtId", ids);
                    else
                        (query as IQueryFilterGroup).PropertyIn("ModelId", ids);

                }
                if ((resultSources?.Length ?? 0) > 0)
                {
                    if (ids.Length > 0)
                    {
                        (query as IQueryFilterGroup).And();
                    }
                    else query.Where();
                    (query as IQueryFilterGroup).PropertyIn("ResultSource", resultSources);
                }
                if ((resultTypes?.Length ?? 0) > 0)
                {
                    query.Where();
                    (query as IQueryFilterGroup).PropertyIn("ResultType", resultTypes.Select(e => e.ToString()).ToList());
                }

                if ((checkTypes?.Length ?? 0) > 0)
                {
                    query.Where();
                    (query as IQueryFilterGroup).PropertyIn("CheckType", checkTypes.Select(e => e.ToString()).ToList());
                }
                AppendLocationSearch(locationId, query);
                AppendIdNameResultInfoSearch(searchString, query, checkTypes);
                (query as IQueryFilterGroup).OrderBy(orderbyParams.ToArray());
                return query;
            }
            if (startDate == null && endDate == null)
            {
                oneQuery = BuildQuery(ValidationFunctionName);
            }
            else
            {
                oneQuery = BuildQuery(ValidationTable);
                oneQuery.Where();
                if (startDate != null && endDate != null)
                {
                    (oneQuery as IQueryFilterGroup).BetweenDates("todatetime(RunInfo.CheckTime)", (DateTimeOffset)startDate, (DateTimeOffset)endDate);
                }
                else
                {
                    var date = startDate == null ? endDate : startDate;
                    (oneQuery as IQueryFilterGroup).OnDate("RunInfo.CheckTime", (DateTimeOffset)date);
                }
            }
            result = await _azureDataExplorerQuery.CreatePagedQueryAsync(_adxDatabase, oneQuery as IQuerySelector, pageSize);
        }

        while (result != null && result.ResultsReader.Read())
        {
            int colIdx = 0;
            var vdto = new ValidationResultsAdxDto();
            try
            {
                //WARNING: The order of these statements must match the order of the columns in the GetDefaultColumns()
                vdto.TwinDtId = result.ResultsReader.GetString(colIdx++);
                vdto.TwinIdentifiers = result.ResultsReader.GetValue(colIdx++);
                vdto.ModelId = result.ResultsReader.GetString(colIdx++);
                vdto.ResultSource = result.ResultsReader.GetString(colIdx++);
                vdto.Description = result.ResultsReader.GetString(colIdx++);
                vdto.ResultType = result.ResultsReader.GetString(colIdx++);
                vdto.CheckType = result.ResultsReader.GetString(colIdx++);
                vdto.ResultInfo = result.ResultsReader.GetValue(colIdx++);
                vdto.RuleScope = result.ResultsReader.GetValue(colIdx++);
                vdto.RuleId = result.ResultsReader.GetString(colIdx++);
                vdto.RunInfo = result.ResultsReader.GetValue(colIdx++);
                vdto.TwinInfo = result.ResultsReader.GetValue(colIdx++);
                vdto.Score = result.ResultsReader.GetInt32(colIdx++);

                twins.Add(vdto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transforming data from ADX. Check if ValidationDto properties match schema in ValidationResults table : {0}", vdto.TwinDtId);
            }
        }
        if (twins != null)
        {
            foreach (var twin in twins)
            {
                try
                {
                    var oneTwin = _mapper.Map<ValidationResults>(twin);
                    validationResults.Add(oneTwin);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception mapping ValidationResults in GetTwinDataQualityResultsByIdAsync() TwinId: {Id}", twin.TwinDtId);
                }
            }
        }

        if (result != null) result.ResultsReader = null;
        return new Page<ValidationResults> { Content = validationResults, ContinuationToken = result != null && result.NextPage > 0 ? System.Text.Json.JsonSerializer.Serialize(result) : null };
    }

    private void AppendIdNameResultInfoSearch(string searchString, IQueryWhere query, CheckType[] checkTypes)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return;

        foreach (var checkType in checkTypes)
        {
            switch (checkType)
            {
                case CheckType.Properties:
                    BuildCheckTypeQuery(query, typeof(ResultInfo), "resultInfo", searchString);
                    break;
                case CheckType.Relationships:
                    BuildCheckTypeQuery(query, typeof(ResultInfoRelationship), "resultInfoRelationship", searchString);
                    break;
                case CheckType.Telemetry:
                    BuildCheckTypeQuery(query, typeof(CapabilityStatus), "resultInfoCapability", searchString);
                    break;
            }
        }
        if (checkTypes.Any())
            (query as IQueryFilterGroup).Or();
        else
            (query as IQueryWhere).Where();
        (query as IQueryFilterGroup).Contains("TwinDtId", searchString);
        (query as IQueryFilterGroup).Or();
        (query as IQueryFilterGroup).Contains("TwinInfo.name", searchString);
    }

    private void BuildCheckTypeQuery(IQueryWhere query, Type type, string typeName, string searchString)
    {
        var propertyInfo = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        (query as IQuerySelector).Extend(typeName, $"parse_json(ResultInfo)");
        (query as IQuerySelector).Expand(typeName);
        query.Where();

        for (int i = 0; i < propertyInfo.Length; i++)
        {
            if (i > 0) (query as IQueryFilterGroup).Or();
            (query as IQueryFilterGroup).Contains($"{typeName}.{propertyInfo[i].Name}", searchString);
        }
    }
    private void AppendLocationSearch(string locationId, IQueryWhere query)
    {
        if (string.IsNullOrWhiteSpace(locationId))
            return;
        var types = typeof(Locations);
        var propertyInfo = types.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        (query as IQuerySelector).Extend("twinInfo", "parse_json(tostring(TwinInfo))");
        query.Where();
        for (int i = 0; i < propertyInfo.Length; i++)
        {
            if (i > 0) (query as IQueryFilterGroup).Or();
            (query as IQueryFilterGroup).Property(string.Format($"twinInfo.locations.{propertyInfo[i].Name}"), locationId);
        }
    }

    public async Task InitDQAdxSettingsAsync()
    {
        var databaseSchema = await _azureDataExplorerInfra.GetDatabaseSchemaAsync(_adxDatabase);
        var adxInitialized = databaseSchema != null && databaseSchema.Tables.ContainsKey(ValidationTable);

        if (!adxInitialized)
        {
            await CreateAdxDefaultInfraAsync();
            await CreateMaterializedViewValidationResults(dropAndRecreate: true);
        }
        else
        {
            //Check table schema
            //If doesnt match, drop and recreate
            var match = CompareSchema(databaseSchema);
            if (!match)
            {
                await _azureDataExplorerInfra.DropTableAsync(_adxDatabase, ValidationTable);
                await CreateAdxDefaultInfraAsync();
                await CreateMaterializedViewValidationResults(dropAndRecreate: true);
            }
        }
        await CreateOrAlterFunctionValidationResults();
    }


    // Comparing schema of the Default Columns and the existing schema in the DB.
    // Returns false when the existing schema in the DB is missing any columns listed in the default columns
    // Otherwise returns true
    private bool CompareSchema(DatabaseSchema dbSchema)
    {
        TableSchema tSchema = dbSchema.Tables[ValidationTable];
        var defaultColumns = GetDefaultColumns();
        foreach (var (name, type) in defaultColumns)
        {
            var dbColumn = tSchema.OrderedColumns.FirstOrDefault(c => c.Name == name && c.Type.GetDescription() == type);
            if (dbColumn == null)
                return false;
        }
        return true;
    }

    public async Task CreateAdxDefaultInfraAsync()
    {
        _logger.LogInformation("Creating ADX default schema.");
        var columns = GetDefaultColumns();

        await _azureDataExplorerInfra.CreateTableAsync(_adxDatabase, ValidationTable, columns);

        _logger.LogInformation("Done creating ADX default schema.");
    }

    private List<Tuple<string, string>> GetDefaultColumns()
    {
        string colTypeString = ColumnType.String.GetDescription();
        string colTypeObject = ColumnType.Object.GetDescription();
        string colTypeInt = ColumnType.Int.GetDescription();

        // WARNING: This list must be kept in sync with ValidationResultsDto with the exact property names
        return new List<Tuple<string, string>>
        {
			//ValidationResults

			new("TwinDtId", colTypeString),
            new("TwinIdentifiers", colTypeObject),
            new("ModelId", colTypeString),

            new("ResultSource", colTypeString),
            new("Description", colTypeString),
            new("ResultType", colTypeString),
            new("CheckType", colTypeString),

            new("ResultInfo", colTypeObject),
            new("RuleScope", colTypeObject),
            new("RuleId", colTypeString),
            new("RunInfo", colTypeObject),

            new("TwinInfo", colTypeObject),
            new("Score", colTypeInt)

        };
    }

    private async Task CreateOrAlterFunctionValidationResults()
    {
        _logger.LogInformation("Creating ValidationResults function.");

        // WARNING: This list must be kept in sync with ValidationResultsDto with the exact property names
        var function = string.Format("{0} |order by RunInfo_CheckTime |  project TwinDtId, TwinIdentifiers, ModelId, ResultSource, Description, ResultType, CheckType, ResultInfo, RuleScope, RuleId, RunInfo, TwinInfo, Score", ValidationMaterializedViewName);

        await _azureDataExplorerInfra.CreateOrAlterFunction(_adxDatabase, ValidationFunctionName, function, ValidationTable);

        _logger.LogInformation("Completed ValidationResults function.");

    }

    private async Task CreateMaterializedViewValidationResults(bool dropAndRecreate = false)
    {
        _logger.LogInformation("Creating ValidationResults materialized view.");

        var function = string.Format("{0} | summarize arg_max(todatetime(RunInfo.CheckTime), *) by TwinDtId, ResultType, CheckType, ResultSource, RuleId", ValidationTable);

        // Don't drop and re-create the DQ materialized view each time so we keep our data between deployments/restarts.
        // Backfill is expensive and may not work on smaller cluster hot-cache configurations such as non-prod.
        // Note if we ever change the view query, we'll have to set dropAndRecreate to true.
        // If we continue to use the mat-view and we ever have a schema-change, we can introduce
        //  a setting to drop the view, then set it to true and then back to false.
        await _azureDataExplorerInfra.CreateMaterializedView(_adxDatabase,
                        ValidationMaterializedViewName, ValidationTable, function,
                        backFill: false, dropAndRecreate: dropAndRecreate);

        _logger.LogInformation("Completed ValidationResults materialized view.");
    }

    public async Task<IEnumerable<TwinsValidationJob>> FindValidationJobsAsync(string jobId = null, AsyncJobStatus? status = null, string userId = null, DateTime? from = null, DateTime? to = null, bool fullDetails = false)
    {
        if (jobId is not null)
            return [(await _jobsService.GetJob(jobId, includeDetail: false)).ToTwinsValidationJob(fullDetails)];

        var searchRequest = new JobSearchRequest
        {
            JobTypes = ["TwinsValidation"],
            JobStatuses = status is not null ? [status.Value] : null,
            UserId = userId,
            StartDate = from,
            EndDate = to
        };
        return (_jobsService.FindJobEntries(searchRequest)).Select(job => job.ToTwinsValidationJob(includeDetail: false)).ToEnumerable();
    }

    public async Task DeleteValidationJobAsync(string[] jobIds)
    {
        _ = await _jobsService.DeleteBulkJobs(jobIds);
    }

    public async Task<TwinsValidationJob> GetLatestValidationJobAsync(AsyncJobStatus? status = null)
    {
        var request = new JobSearchRequest
        {
            JobTypes = ["TwinsValidation"],
            JobStatuses = status is not null ? [status.Value] : null,
        };
        // Find job entries always filter time descending
        var jobs = _jobsService.FindJobEntries(request);
        var latest = await jobs.FirstOrDefaultAsync();
        return latest?.ToTwinsValidationJob(includeDetail: false);
    }

    public async Task<TwinsValidationJob> QueueTwinsValidationProcessAsync(string userId,
        IEnumerable<string> modelIds = null,
        bool? exactModelMatch = null,
        string locationId = null,
        DateTimeOffset? startCheckTime = null,
        DateTimeOffset? endCheckTime = null)
    {
        var validationJob = new TwinsValidationJob
        {
            UserId = userId,
            CreateTime = DateTime.UtcNow,
            ModelIds = modelIds?.ToList(),
            ExactModelMatch = exactModelMatch,
            LocationId = locationId,
            StartTime = startCheckTime,
            EndTime = endCheckTime
        };

        var jobsEntry = await _jobsService.CreateOrUpdateJobEntry(validationJob.ToUnifiedJob(includeDetail: true));
        // Note: this job will have a JobId now
        var job = jobsEntry.ToTwinsValidationJob(includeDetail: true);

        if (!_twinsValidationTaskQueue.TryQueueJob(job))
            throw new Exception("Queue twins validation job failed.");

        return job;
    }

    // Note we may want to cache the whole jobs list - in which case we should remove this
    //   in favor of the more general solution
    public bool IsAnyJobRunning()
    {
        return _memoryCache.TryGetValue(_runningValidationKey, out var _);
    }

    public async Task ProcessTwinsValidationJobAsync(TwinsValidationJob twinsValidationJob)
    {
        try
        {
            if (IsAnyJobRunning())
                _logger.LogWarning("Re-entering ProcessTwinsValidationJobAsync");

            _memoryCache.Set(_runningValidationKey, twinsValidationJob);
            await _ProcessTwinsValidationJobAsync(twinsValidationJob);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing validation scan {twinsValidationJob.JobId}");

            twinsValidationJob.Details.Status = AsyncJobStatus.Error;
            twinsValidationJob.Details.StatusMessage = ex.Message;
        }
        finally
        {
            _memoryCache.Remove(_runningValidationKey);

            // Note: this will update the job with the final status
            _ = await _jobsService.CreateOrUpdateJobEntry(twinsValidationJob.ToUnifiedJob(includeDetail: false));
        }
    }

    private async Task _ProcessTwinsValidationJobAsync(TwinsValidationJob twinsValidationJob)
    {
        string continuationToken = null;
        twinsValidationJob.SummaryDetails.ProcessedEntities = 0;

        twinsValidationJob.Details.Status = AsyncJobStatus.Processing;
        twinsValidationJob.Details.StartTime = twinsValidationJob.CreateTime;

        // Note: this will update the job with the Processing status
        _ = await _jobsService.CreateOrUpdateJobEntry(twinsValidationJob.ToUnifiedJob(includeDetail: false));

        var startTime = Stopwatch.StartNew();
        var nPage = 0;
        int nTwins = 0;

        // TODO: Support cancellation
        try
        {
            var cache = _azureDigitalTwinCacheProvider.GetOrCreateCache();
            var unitsInfo = cache.ModelCache.UnitInfos;

            // Get the list of all models that is the intersection of every model, including child models if appropriate,
            //   that are mentioned in the model filter and every template rule.
            // The usual case is to have no model filter (we may remove this completely - should be a template filter if anything)
            //   so we don't want to retrieve every twin in the instance - only those that apply to our rules
            var modelsToQuery = await _dQRuleService.GetRuleModels(twinsValidationJob.ModelIds, twinsValidationJob.ExactModelMatch ?? false);
            twinsValidationJob.SummaryDetails.ModelsQueried = modelsToQuery;

            do // while (continuationToken != null)
            {
                var request = new GetTwinsInfoRequest
                {
                    LocationId = twinsValidationJob.LocationId,
                    // Note we have already pre-expanded all child models in GetRuleModels above
                    ExactModelMatch = true,
                    IncludeRelationships = false,
                    IncludeIncomingRelationships = false,
                    ModelId = modelsToQuery.ToArray(),
                    StartTime = twinsValidationJob.StartTime,
                    EndTime = twinsValidationJob.EndTime
                };
                var twinsPage = await _adxService.GetTwins(
                                request,
                                pageSize: 500,
                                continuationToken: continuationToken);

                ++nPage;
                var twins = twinsPage.Content.ToList();
                nTwins += twins.Count;

                _logger.LogInformation("Page# {page} of {nTwins} twins ({nTotalTwins} total)", nPage, twins.Count, nTwins);

                continuationToken = twinsPage.ContinuationToken;

                var results = await _dQRuleService.GetValidationResults(twins, unitsInfo);
                var allResults = new List<ValidationResults>();

                foreach (var x in results)
                {
                    var locationInfo = x.TwinWithRelationship.TwinData.GetValueOrDefault("Location");

                    var name = x.TwinWithRelationship.Twin.Contents.GetValueOrDefault("name")?.ToString();
                    var uniqueId = x.TwinWithRelationship.Twin.Contents.GetValueOrDefault("uniqueID")?.ToString();
                    var twinInfo = new TwinInfo(new Locations((Dictionary<string, string>)locationInfo), name);
                    var twinIds = new TwinIdentifiers { uniqueId = uniqueId };

                    void AddValidationResults(Result resultType, CheckType checkType, object validationResults)
                    {
                        var result = new ValidationResults
                        {
                            RunInfo = System.Text.Json.JsonSerializer.Serialize(new RunInfo(twinsValidationJob.Details.StartTime)),
                            ModelId = x.TwinWithRelationship.Twin.Metadata.ModelId,
                            RuleId = x.RuleTemplate.Id,
                            ResultSource = ResultSourceStaticDataQuality,
                            TwinDtId = x.TwinWithRelationship.Twin.Id,
                            TwinInfo = System.Text.Json.JsonSerializer.Serialize(twinInfo),
                            TwinIdentifiers = System.Text.Json.JsonSerializer.Serialize(twinIds),
                            ResultInfo = System.Text.Json.JsonSerializer.Serialize(validationResults, new JsonSerializerOptions
                            {
                                Converters = { new JsonStringEnumConverter() }
                            }),
                            ResultType = resultType,
                            CheckType = checkType
                        };
                        allResults.Add(result);
                    }
                    AddValidationResults(x.PropertyValidationResults.Any() ? Result.Error : Result.Ok, CheckType.Properties, x.PropertyValidationResults);
                    AddValidationResults(x.PathValidationResults.Any() ? Result.Error : Result.Ok, CheckType.Relationships, x.PathValidationResults);
                }

                // TODO: We may want to move this to the end of this fn so that we only write results
                //  when the entire job succeeds so that results are either all present for a successful job or absent for an error job.
                await IngestDataToValidationTableAsync(allResults);

                twinsValidationJob.SummaryDetails.ProcessedEntities = nTwins;
                twinsValidationJob.LastUpdateTime = DateTime.UtcNow;
                updateJobSummary(allResults, twinsValidationJob.SummaryDetails);

                // Note: this will update the job with summary details
                _ = await _jobsService.CreateOrUpdateJobEntry(twinsValidationJob.ToUnifiedJob(includeDetail: true));
            }
            while (continuationToken != null);

            twinsValidationJob.Details.Status = AsyncJobStatus.Done;
        }
        finally
        {
            startTime.Stop();
            _logger.LogInformation("Completed processing validation scan {jobId} of {nTwins} in {elapsed}",
                                twinsValidationJob.JobId, nTwins, startTime.Elapsed);

            twinsValidationJob.Details.EndTime = DateTime.UtcNow;
            twinsValidationJob.LastUpdateTime = DateTime.UtcNow;
        }
    }

    private void updateJobSummary(List<ValidationResults> allresults, TwinValidationJobSummaryDetails summaryDetails)
    {
        var summaryResults = allresults.GroupBy(r => r.ResultType).ToDictionary(g => g.Key, g => g.Count());
        _logger.LogInformation("Validation page result summary: {resultSummary}", JsonConvert.SerializeObject(summaryResults));

        var summaryModels = allresults.GroupBy(r => r.ModelId).ToDictionary(g => g.Key, g => g.Count());
        _logger.LogInformation("Validation page model summary: {modelSummary}", JsonConvert.SerializeObject(summaryModels));

        // Create per-model-resultType-checkType nested dicts
        Dictionary<string, Dictionary<Result, Dictionary<CheckType, int>>> detailedSummary =
                allresults.GroupBy(vr => vr.ModelId).ToDictionary(
                    modelGrp => modelGrp.Key,
                    modelGrp => modelGrp.GroupBy(vr => vr.ResultType).ToDictionary(
                        resultTypeGrp => resultTypeGrp.Key,
                        resultTypeGrp => resultTypeGrp.GroupBy(vr => vr.CheckType).ToDictionary(
                            checkTypeGrp => checkTypeGrp.Key,
                            checkTypeGrp => checkTypeGrp.Count())));

        _logger.LogInformation("Validation page by-model summary: {detailedSummary}", JsonConvert.SerializeObject(detailedSummary));

        if (summaryDetails.ErrorsByModel == null)
            return;

        // TODO: Add NumUnitXXX When we have separate unit-checker apart from property checking
        // TODO: We could just expose the detailedSummaryDict and let the client do the rollups
        //    so we don't have to modify this every time we add/remove checktypes
        foreach (var (model, countsForModel) in detailedSummary)
        {
            summaryDetails.ErrorsByModel.TryGetValue(model, out var counts);
            if (counts == null)
                counts = summaryDetails.ErrorsByModel[model] = new TwinValidationJobSummaryDetailErrors();

            counts.NumOK += countsForModel.GetValueOrDefault(Result.Ok)?.Values.Sum() ?? 0;
            counts.NumErrors += countsForModel.GetValueOrDefault(Result.Error)?.Values.Sum() ?? 0;

            counts.NumPropertyOK += countsForModel.GetValueOrDefault(Result.Ok)
                                                        ?.GetValueOrDefault(CheckType.Properties) ?? 0;
            counts.NumRelationshipOK += countsForModel.GetValueOrDefault(Result.Ok)
                                                        ?.GetValueOrDefault(CheckType.Relationships) ?? 0;

            counts.NumPropertyErrors += countsForModel.GetValueOrDefault(Result.Error)
                                                            ?.GetValueOrDefault(CheckType.Properties) ?? 0;
            counts.NumRelationshipErrors += countsForModel.GetValueOrDefault(Result.Error)
                                                            ?.GetValueOrDefault(CheckType.Relationships) ?? 0;
        }
    }
}
