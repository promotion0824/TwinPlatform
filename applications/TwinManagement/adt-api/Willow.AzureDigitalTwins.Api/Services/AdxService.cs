using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.AzureDataExplorer.Builders;
using Willow.AzureDataExplorer.Command;
using Willow.AzureDataExplorer.Options;
using Willow.AzureDataExplorer.Query;
using Willow.AzureDigitalTwins.Api.Custom;
using Willow.AzureDigitalTwins.Api.Diagnostic;
using Willow.AzureDigitalTwins.Api.Helpers;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Services
{
    public interface IAdxService
    {
        Task<Page<TwinWithRelationships>> GetTwins(
            GetTwinsInfoRequest request,
            int pageSize = 100,
            string continuationToken = null);

        Task<Dictionary<string, int>> GetTwinCountByModelAsync(string locationId = null, string modelId = null);
        Task<int> GetTwinsCount(GetTwinsInfoRequest request);
        Task<Page<TwinWithRelationships>> GetTwinsByIds(
            string[] id,
            bool includeRelationships = false);
        Task<IDictionary<string, IDictionary<ExportColumn, string>>> GetRawTwinsById(params string[] ids);
        Task<Page<TwinWithRelationships>> QueryTwinsAsync(string query, bool includeOutgoingRelationships = false, bool includeIncomingRelationships = false, int pageSize = 100, string continuationToken = null);
        Task<Page<JsonDocument>> QueryAsync(string query, int pageSize = 100, string continuationToken = null);
    }

    public class AdxService : IAdxService
    {
        private readonly IAzureDataExplorerQuery _azureDataExplorerQuery;
        private readonly AzureDataExplorerOptions _azureDataExplorerOptions;
        private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;

        private readonly IAzureDataExplorerCommand _azureDataExplorerCommand;
        private readonly ILogger<AdxService> _logger;
        private readonly IAdxSetupService _adxSetupService;
        private readonly HealthCheckADX _healthCheckADX;

        public AdxService(IAzureDataExplorerQuery azureDataExplorerQuery,
            IOptions<AzureDataExplorerOptions> azureDataExplorerOptions,
            IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
            IAzureDataExplorerCommand azureDataExplorerCommand,
            ILogger<AdxService> logger,
            IAdxSetupService adxSetupService,
            HealthCheckADX healthCheckADX
            )
        {
            _azureDataExplorerQuery = azureDataExplorerQuery;
            _azureDataExplorerOptions = azureDataExplorerOptions.Value;
            _azureDigitalTwinModelParser = azureDigitalTwinModelParser;
            _azureDataExplorerCommand = azureDataExplorerCommand;
            _logger = logger;
            _adxSetupService = adxSetupService;
            _healthCheckADX = healthCheckADX;
        }

        // Get all twins with relationships from ADX plus health check
        public async Task<Page<TwinWithRelationships>> GetTwins(
            GetTwinsInfoRequest request,
            int pageSize = 100,
            string continuationToken = null)
        {
            try
            {
                var twinWithRelationship = request.OrphanOnly
                    ? await this.GetOrphans(request.ModelId, pageSize, continuationToken)
                    : await this.GetTwinsHelper(request, pageSize, continuationToken);

                HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckADX, HealthCheckADX.Healthy, _logger);

                return twinWithRelationship;
            }
            catch
            {
                HealthStatusChangeHelper.ChangeHealthStatus(_healthCheckADX, HealthCheckADX.FailingCalls, _logger);

                throw;
            }
        }

        //Sample query built:
        //ActiveTwins
        //| where
        //ModelId in ("dtmi:com:willowinc:TransferSwitch;1","dtmi:com:willowinc:AutomaticTransferSwitch;1","dtmi:com:willowinc:MaintenanceBypassSwitch;1") | project TwinColumn = Raw, Id, ExportTime
        //| join kind = leftouter(ActiveRelationships
        //) on $left.Id == $right.SourceId
        //| summarize
        //take_any(TwinColumn, ExportTime), OutgoingRelationships = make_set(Raw) by Id
        private async Task<Page<TwinWithRelationships>> GetTwinsHelper(
            GetTwinsInfoRequest request,
            int pageSize = 100,
            string continuationToken = null)
        {
            var adxSchema = await _adxSetupService.GetAdxTableSchema();

            var twinEntityColumn = adxSchema.FirstOrDefault(x => x.Destination == EntityType.Twins && x.IsFullEntityColumn);

            if (twinEntityColumn is null)
                throw new NotImplementedException("No IsFullEntity column defined for Twins");

            var relationshipEntityColumn = adxSchema.FirstOrDefault(x => x.Destination == EntityType.Relationships && x.IsFullEntityColumn);

            if (relationshipEntityColumn is null)
                throw new NotImplementedException("No IsFullEntity column defined for Relationships");

            var twins = new List<TwinWithRelationships>();
            PageQueryResult result = null;

            if (!string.IsNullOrEmpty(continuationToken))
            {
                var pagedQuery = System.Text.Json.JsonSerializer.Deserialize<PageQueryResult>(continuationToken);

                result = await _azureDataExplorerQuery.GetPageQueryAsync(_azureDataExplorerOptions.DatabaseName, pagedQuery, pageSize);
            }
            else
            {
                var locationSearchColumns = adxSchema.Where(x => x.Destination == EntityType.Twins && x.UseForLocationSearch).ToList();
                var ingestionColumnName = adxSchema.First(x => x.Destination == EntityType.Twins && x.IsIngestionTimeColumn).Name;

                if (request.LocationId != null && !locationSearchColumns.Any())
                {
                    throw new NotImplementedException("Missing location search column in adx schema");
                }

                var query = QueryBuilderHelper.Create(AdxConstants.TwinsFunctionName);

                ModelsFilter(request.ModelId, request.ExactModelMatch, query);
                QueryBuilderHelper.AppendLocationFilter(request.LocationId, query, locationSearchColumns);
                QueryBuilderHelper.AppendSearchStringFilter(request.SearchString, query);
                QueryBuilderHelper.AppendTimeFilter(request.StartTime, request.EndTime, ingestionColumnName, query);
                QueryBuilderHelper.AppendQueryFilter(request.QueryFilter?.Filter, query);

                (query as IQuerySelector).Project($"{AdxConstants.twinColumnAlias} = {twinEntityColumn.Name}", AdxConstants.twinIdColumnName, AdxConstants.exportTimeColumnAlias, AdxConstants.locationColumnAlias);

                QueryBuilderHelper.AppendRelationships(request.IncludeRelationships, request.IncludeIncomingRelationships, relationshipEntityColumn, query);

                (query as IQueryFilterGroup).Sort(AdxConstants.twinIdColumnName);

                _logger.LogTrace("GetTwins ADX query: {Query}", query.GetQuery());

                result = await _azureDataExplorerQuery.CreatePagedQueryAsync(_azureDataExplorerOptions.DatabaseName, query as IQuerySelector, pageSize);
            }

            while (result.ResultsReader.Read())
            {
                // Returns null if unable to parse any invalid twins from ADX
                var twinWithRelationship = ConstructTwinWithRelationship(result.ResultsReader[AdxConstants.twinColumnAlias].ToString(),
                       result.ResultsReader,
                       twinEntityColumn,
                       relationshipEntityColumn,
                       request.IncludeRelationships,
                       request.IncludeIncomingRelationships);

                // Ignore nulls in the response
                if (twinWithRelationship is not null)
                {
                    twins.Add(twinWithRelationship);
                }
            }

            result.ResultsReader = null;
            return new Page<TwinWithRelationships> { Content = twins, ContinuationToken = result.NextPage > 0 ? System.Text.Json.JsonSerializer.Serialize(result) : null };
        }

        public async Task<Page<TwinWithRelationships>> GetTwinsByIds(string[] ids, bool includeRelationships = false)
        {
            var adxSchema = await _adxSetupService.GetAdxTableSchema();

            var twinEntityColumn = adxSchema.FirstOrDefault(x => x.Destination == EntityType.Twins && x.IsFullEntityColumn);

            if (twinEntityColumn is null)
                throw new NotImplementedException("No IsFullEntity column defined for Twins");

            var relationshipEntityColumn = adxSchema.FirstOrDefault(x => x.Destination == EntityType.Relationships && x.IsFullEntityColumn);

            if (relationshipEntityColumn is null)
                throw new NotImplementedException("No IsFullEntity column defined for Relationships");

            var query = QueryBuilderHelper.Create(AdxConstants.TwinsFunctionName);

            QueryBuilderHelper.AppendMultipleIdFilter(query, ids);

            (query as IQuerySelector).Project($"{AdxConstants.twinColumnAlias} = {twinEntityColumn.Name}", AdxConstants.twinIdColumnName, AdxConstants.exportTimeColumnAlias, AdxConstants.locationColumnAlias);

            if (includeRelationships)
            {
                QueryBuilderHelper.AppendRelationships(includeRelationships: true, includeIncomingRelationships: true, relationshipEntityColumn, query);
            }
            PageQueryResult result = await _azureDataExplorerQuery.CreatePagedQueryAsync(_azureDataExplorerOptions.DatabaseName, query as IQuerySelector, ids.Length, includeRowNumber: false);

            var twinWithRelationships = new List<TwinWithRelationships>();

            while (result.ResultsReader.Read())
            {
                var twinWithRelationship = ConstructTwinWithRelationship(result.ResultsReader[AdxConstants.twinColumnAlias].ToString(),
                                       result.ResultsReader,
                                       twinEntityColumn,
                                       relationshipEntityColumn,
                                       includeOutgoingRelationships: includeRelationships,
                                       includeIncomingRelationships: includeRelationships);
                twinWithRelationships.Add(twinWithRelationship);

            }
            result.ResultsReader = null;
            return new Page<TwinWithRelationships> { Content = twinWithRelationships };
        }

        public async Task<IDictionary<string, IDictionary<ExportColumn, string>>> GetRawTwinsById(params string[] ids)
        {
            var twinColumns = (await _adxSetupService.GetAdxTableSchema()).Where(x => x.Destination == EntityType.Twins);

            var query = QueryBuilderHelper.Create(AdxConstants.TwinsFunctionName);
            QueryBuilderHelper.AppendMultipleIdFilter(query, ids);
            IDataReader resultsReader = await _azureDataExplorerCommand.ExecuteQueryAsync(_azureDataExplorerOptions.DatabaseName, query.GetQuery());

            Dictionary<string, IDictionary<ExportColumn, string>> results = new();
            while (resultsReader.Read())
            {
                var twinColumnResult = twinColumns.ToDictionary(x => x,
                    // string empty checks were done to avoid DBNull and ultimately convert them to nulls
                    y => !string.IsNullOrWhiteSpace(resultsReader[y.Name]?.ToString()) ? resultsReader[y.Name].ToString() : null);
                results.Add(resultsReader[AdxConstants.twinIdColumnName].ToString(), twinColumnResult);
            }

            return results;
        }

        public async Task<int> GetTwinsCount(GetTwinsInfoRequest request)
        {
            var adxSchema = await _adxSetupService.GetAdxTableSchema();
            var locationSearchColumns = adxSchema.Where(x => x.Destination == EntityType.Twins && x.UseForLocationSearch).ToList();
            var ingestionColumnName = adxSchema.First(x => x.Destination == EntityType.Twins && x.IsIngestionTimeColumn).Name;

            if (request.LocationId != null && !locationSearchColumns.Any())
            {
                throw new NotImplementedException("Missing location search column in adx schema");
            }

            var query = QueryBuilderHelper.Create(AdxConstants.TwinsFunctionName);

            ModelsFilter(request.ModelId, request.ExactModelMatch, query);
            QueryBuilderHelper.AppendLocationFilter(request.LocationId, query, locationSearchColumns);
            QueryBuilderHelper.AppendSearchStringFilter(request.SearchString, query);
            QueryBuilderHelper.AppendTimeFilter(request.StartTime, request.EndTime, ingestionColumnName, query);

            (query as IQueryFilterGroup).GetCount();
            return await _azureDataExplorerQuery.GetTotalCount(_azureDataExplorerOptions.DatabaseName, query.GetQuery());
        }

        public async Task<Dictionary<string, int>> GetTwinCountByModelAsync(string locationId = null, string modelId = null)
        {
            const string countAlias = "TwinCount";
            var query = QueryBuilderHelper.Create(AdxConstants.TwinsFunctionName);

            if (locationId != null)
            {
                var adxSchema = await _adxSetupService.GetAdxTableSchema();
                var locationSearchColumns = adxSchema.Where(x => x.Destination == EntityType.Twins && x.UseForLocationSearch).ToList();

                if (!locationSearchColumns.Any())
                {
                    _logger.LogWarning("Missing location search column in adx schema");
                    return new Dictionary<string, int>();
                }

                QueryBuilderHelper.AppendLocationFilter(locationId, query, locationSearchColumns);
            }

            if (modelId != null)
                ModelsFilter(new string[] { modelId }, false, query);

            (query as IQuerySelector)
                .Summarize()
                .Count(countAlias)
                .By(AdxConstants.ModelIdColumnName);

            var results = await _azureDataExplorerCommand.ExecuteQueryAsync(_azureDataExplorerOptions.DatabaseName, query.GetQuery());
            var twinCountByModel = new Dictionary<string, int>();

            while (results.Read())
            {
                var twinCount = Convert.ToInt32(results[countAlias]);
                var model = results[AdxConstants.ModelIdColumnName].ToString();

                twinCountByModel.Add(model, twinCount);
            }
            return twinCountByModel;
        }

        //Query used:
        //ActiveTwins
        //| join kind = leftouter(
        //ActiveRelationships
        //) on $left.Id == $right.SourceId
        //| join kind = leftouter(
        //ActiveRelationships
        //) on $left.Id == $right.TargetId
        //|project TwinColumn = Raw, Id, RelationshipName = Name1
        //| where RelationshipName == ""
        public async Task<Page<TwinWithRelationships>> GetOrphans(string[] modelIds,
                        int pageSize = 100,
                        string continuationToken = null)
        {
            var adxSchema = await _adxSetupService.GetAdxTableSchema();

            var twins = new List<TwinWithRelationships>();
            PageQueryResult result = null;

            var twinEntityColumn = adxSchema.FirstOrDefault(x => x.Destination == EntityType.Twins && x.IsFullEntityColumn);
            if (twinEntityColumn is null)
                throw new NotImplementedException("No IsFullEntity column defined for Twins");

            var query = QueryBuilderHelper.Create(AdxConstants.TwinsFunctionName);
            ModelsFilter(modelIds, exactModelMatch: true, query);

            var orderbyParams = new List<OrderByParam>
                {
                    new OrderByParam(AdxConstants.twinIdColumnName, Order.asc)
                };

            (query as IQuerySelector).Join(
                QueryBuilderHelper.Create(AdxConstants.RelationshipsFunctionName).GetQuery(),
                AdxConstants.twinIdColumnName,
                AdxConstants.SourceIdColumnName,
                "leftanti");
            (query as IQuerySelector).Join(
                QueryBuilderHelper.Create(AdxConstants.RelationshipsFunctionName).GetQuery(),
                AdxConstants.twinIdColumnName,
                AdxConstants.TargetIdColumnName,
                "leftanti");
            (query as IQuerySelector).Project($"{AdxConstants.twinColumnAlias} = {twinEntityColumn.Name}", AdxConstants.twinIdColumnName);

            (query as IQueryFilterGroup).OrderBy(orderbyParams.ToArray());

            result = await _azureDataExplorerQuery.CreatePagedQueryAsync(_azureDataExplorerOptions.DatabaseName, query as IQuerySelector, pageSize);

            while (result.ResultsReader.Read())
            {
                bool hasTwin = TryGetTwin(result.ResultsReader[AdxConstants.twinColumnAlias].ToString(), twinEntityColumn, out BasicDigitalTwin twin);

                if (hasTwin)
                {
                    twins.Add(new TwinWithRelationships() { Twin = twin });
                }
            }

            result.ResultsReader = null;
            return new Page<TwinWithRelationships> { Content = twins, ContinuationToken = result.NextPage > 0 ? System.Text.Json.JsonSerializer.Serialize(result) : null };
        }

        public async Task<Page<TwinWithRelationships>> QueryTwinsAsync(string query, bool includeOutgoingRelationships = false, bool includeIncomingRelationships = false, int pageSize = 100, string continuationToken = null)
        {
            var adxSchema = await _adxSetupService.GetAdxTableSchema();

            var twinEntityColumn = adxSchema.FirstOrDefault(x => x.Destination == EntityType.Twins && x.IsFullEntityColumn);

            if (twinEntityColumn is null)
                throw new NotImplementedException("No IsFullEntity column defined for Twins");

            var relationshipEntityColumn = adxSchema.FirstOrDefault(x => x.Destination == EntityType.Relationships && x.IsFullEntityColumn);

            if (relationshipEntityColumn is null)
                throw new NotImplementedException("No IsFullEntity column defined for Relationships");
            var q = QueryBuilderHelper.Create(query);
            PageQueryResult result = null;

            try
            {
                if (string.IsNullOrEmpty(continuationToken))
                {
                    result = await _azureDataExplorerQuery.CreatePagedQueryAsync(_azureDataExplorerOptions.DatabaseName, q as IQuerySelector, pageSize, includeRowNumber: false);
                }
                else
                {
                    var pagedQuery = System.Text.Json.JsonSerializer.Deserialize<PageQueryResult>(continuationToken);

                    result = await _azureDataExplorerQuery.GetPageQueryAsync(_azureDataExplorerOptions.DatabaseName, pagedQuery, pageSize);
                }

            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }

            List<TwinWithRelationships> twinWithRelationships = new();

            while (result.ResultsReader.Read())
            {
                var twinWithRelationship = ConstructTwinWithRelationship(result.ResultsReader["Raw"].ToString(),
                                                       result.ResultsReader,
                                                       twinEntityColumn,
                                                       relationshipEntityColumn,
                                                       includeOutgoingRelationships,
                                                       includeIncomingRelationships);
                if (twinWithRelationship != null) twinWithRelationships.Add(twinWithRelationship);
            }
            result.ResultsReader = null;
            return new Page<TwinWithRelationships> { Content = twinWithRelationships, ContinuationToken = result.NextPage > 0 ? System.Text.Json.JsonSerializer.Serialize(result) : null };

        }

        public async Task<Page<JsonDocument>> QueryAsync(string query, int pageSize = 100, string continuationToken = null)
        {
            var adxSchema = await _adxSetupService.GetAdxTableSchema();

            var twinEntityColumn = adxSchema.FirstOrDefault(x => x.Destination == EntityType.Twins && x.IsFullEntityColumn);
            if (twinEntityColumn is null)
                throw new NotImplementedException("No IsFullEntity column defined for Twins");

            var q = QueryBuilderHelper.Create(query);
            PageQueryResult result = null;
            List<JsonDocument> documents = new();

            try
            {
                if (string.IsNullOrEmpty(continuationToken))
                {
                    result = await _azureDataExplorerQuery.CreatePagedQueryAsync(_azureDataExplorerOptions.DatabaseName, q as IQuerySelector, pageSize, includeRowNumber: false);
                }
                else
                {
                    var pagedQuery = System.Text.Json.JsonSerializer.Deserialize<PageQueryResult>(continuationToken);

                    result = await _azureDataExplorerQuery.GetPageQueryAsync(_azureDataExplorerOptions.DatabaseName, pagedQuery, pageSize);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
            var rowsDictionary = ReadData(result.ResultsReader);
            string json = System.Text.Json.JsonSerializer.Serialize(rowsDictionary);
            JsonDocument jsonDoc = JsonDocument.Parse(json);
            documents.Add(jsonDoc);
            return new Page<JsonDocument> { Content = documents, ContinuationToken = result.NextPage > 0 ? System.Text.Json.JsonSerializer.Serialize(result) : null };
        }

        private static IDictionary<string, object> ReadData(IDataReader reader)
        {
            if (reader == null)
                return new Dictionary<string, object>();

            var wrapper = new Dictionary<string, object>();
            var rows = new List<Dictionary<string, object>>();

            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader[i]);
                }
                rows.Add(row);
            }

            wrapper.Add("Rows", rows);
            return wrapper;
        }

        private TwinWithRelationships ConstructTwinWithRelationship(string twinColumn,
            IDataReader resultReader,
            ExportColumn twinEntityColumn,
            ExportColumn relationshipEntityColumn,
            bool includeOutgoingRelationships,
            bool includeIncomingRelationships)
        {
            bool hasExportTime = TryGetTime(resultReader[AdxConstants.exportTimeColumnAlias].ToString(), out DateTimeOffset? exportTime);

            bool hasLocation = TryGetLocation(resultReader[AdxConstants.locationColumnAlias].ToString(), out Dictionary<string, string> location);

            bool hasTwin = TryGetTwin(
                twinColumn,
                twinEntityColumn,
                out BasicDigitalTwin twin);


            if (hasTwin)
            {
                bool hasTwinMetaData = TryGetTwinMetaData(twin, out DigitalTwinPropertyMetadata metadata);

                var twinData = hasExportTime || hasLocation || hasTwinMetaData ? new Dictionary<string, object>
                                    {
                                        {AdxConstants.locationColumnAlias, location },
                                        {AdxConstants.exportTimeColumnAlias, exportTime },
                                        {AdxConstants.lastUpdateTimeColumnAlias, metadata?.LastUpdatedOn }
                                    } : null;

                var twinWithRelationship = new TwinWithRelationships
                {
                    Twin = twin,
                    OutgoingRelationships = GetRelationships(includeOutgoingRelationships, resultReader, AdxConstants.outgoingRelationshipsColumnAlias, relationshipEntityColumn),
                    IncomingRelationships = GetRelationships(includeIncomingRelationships, resultReader, AdxConstants.incomingRelationshipsColumnAlias, relationshipEntityColumn),
                    TwinData = twinData
                };
                return twinWithRelationship;
            }
            return null;
        }


        private static List<BasicRelationship> GetRelationships(bool include, IDataReader reader, string columnName, ExportColumn column)
        {
            if (include && reader[columnName] is JArray relationshipsArray && relationshipsArray.HasValues)
            {
                return relationshipsArray.Select(x => column.IsCustomColumn
                            ? Newtonsoft.Json.JsonConvert.DeserializeObject<RealEstateRelationship>(x.ToString(), new RealEstateRelationshipJsonConverter()).ToBasicRelationship()
                            : System.Text.Json.JsonSerializer.Deserialize<BasicRelationship>(x.ToString())
                        ).ToList();
            }

            return Enumerable.Empty<BasicRelationship>().ToList();
        }

        private bool TryGetTwin(string twin, ExportColumn column, out BasicDigitalTwin result)
        {
            result = null;
            if (string.IsNullOrEmpty(twin))
            {
                _logger.LogWarning("Twin string should NOT be null or empty");
                return false;
            }

            // Check if the Full Entity Column is a custom column
            // IsCustomColumn = true :- Column contains non BasicDigitalTwin serialized string JSON
            // IsCustomColumn = false :- Column contains BasicDigitalTwin serialized string JSON
            if (!column.IsCustomColumn)
            {
                result = System.Text.Json.JsonSerializer.Deserialize<BasicDigitalTwin>(twin);
                return true;
            }

            try
            {
                var customTwin = JsonConvert.DeserializeObject<RealEstateTwin>(twin, new RealEstateTwinJsonConverter());
                result = customTwin.ToBasicDigitalTwin();
            }
            catch (Exception ex)
            {
                // Exception occurs if unable to parse ADX Raw column to BasicDigialTwin
                // so we return null and the twin record will be ignore from the response.
                _logger.LogError(ex, "Error converting ADX Raw column to BasicDigitalTwin. Validate ADX Twin data.");

                //TODO: Add metrics to count the number of invalid twins
            }

            return result is not null;
        }

        private bool TryGetTime(string time, out DateTimeOffset? result)
        {
            result = null;
            if (string.IsNullOrEmpty(time))
            {
                _logger.LogWarning("ExportTime string should NOT be null or empty");
                return false;
            }

            try
            {
                result = DateTimeOffset.Parse(time);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing ExportTime string: {Time}", time);
                return false;
            }
        }

        private bool TryGetLocation(string location, out Dictionary<string, string> result)
        {
            result = null;
            if (string.IsNullOrEmpty(location))
            {
                _logger.LogWarning("Location string should NOT be null or empty");
                return false;
            }

            try
            {
                result = JsonConvert.DeserializeObject<Dictionary<string, string>>(location);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Location string: {Location}", location);
                return false;
            }
        }

        private static bool TryGetTwinMetaData(BasicDigitalTwin twin, out DigitalTwinPropertyMetadata metadata)
        {
            metadata = null;
            if (twin.Metadata != null)
            {
                metadata = twin.Metadata.PropertyMetadata?[RealEstateTwin.TwinPropertyMetaData];
                return metadata != null;
            }
            return false;
        }

        private void ModelsFilter(string[] modelIds, bool exactModelMatch, IQueryWhere query)
        {
            if (modelIds == null || !modelIds.Any())
                return;

            var descendants = _azureDigitalTwinModelParser.GetInterfaceDescendants(modelIds);
            QueryBuilderHelper.AppendModelsFilter(modelIds, exactModelMatch, query, descendants.Select(s => s.Key).ToList());
        }
    }
}
