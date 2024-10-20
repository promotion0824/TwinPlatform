using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using DigitalTwinCore.Exceptions;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services.AdtApi;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Kusto.Ingest;
using Kusto.Language;
using Kusto.Language.Editor;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using DigitalTwinCore.Dto.Adx;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using System.Text.Json.Serialization;
using Willow.Infrastructure;

namespace DigitalTwinCore.Services.Adx
{
    public interface IAdxHelper
    {
        Export QueueExport(Guid siteId, IDigitalTwinService service, SourceInfo source = null);
        IEnumerable<Export> GetExports(Guid siteId);
        Task<IDataReader> Query(IDigitalTwinService service, string query, CancellationToken cancellationToken = default);
        Task AppendTwin(IDigitalTwinService service, BasicDigitalTwin basicDigitalTwin, bool deleted = false, string userId = null);
        Task AppendRelationship(IDigitalTwinService service, BasicRelationship basicRelationship, bool deleted = false);
        Task AppendModel(IDigitalTwinService service, AdtModel adtModel, bool deleted = false);
        Task<IDataReader> Query(string database, string query, CancellationToken cancellationToken = default);
        Task SetupADXIfEmpty(ICslAdminProvider queryProvider, string databaseName);
        Task SetupADX(string databaseName);
        Task CreateOrUpdateTables(ICslAdminProvider queryProvider, string databaseName);
        Task RecreateViews(ICslAdminProvider queryProvider, string databaseName);
        Task RecreateFunctions(ICslAdminProvider queryProvider, string databaseName);
        Task SetupTwinsTable(string database, string table, string dedupView, string function);
        Task SetupRelationshipsTable(string database, string table, string dedupView, string function);
    }

    public class AdxHelper : IAdxHelper
    {
        private const string _twinsAdxTempTable = "Twins_Temp";
        private const string _twinsAdxTableMapping = "TwinsMapping";
        private const string _relationshipsAdxTempTable = "Relationships_Temp";
        private const string _relationshipsAdxTableMapping = "RelationshipsMapping";
        private const string _modelsAdxTempTable = "Models_Temp";
        private const string _modelsAdxTableMapping = "ModelsMapping";

        private const string _logKey = "ADXEXPORT";
        private readonly string _storageAccountConnectionString;
        private readonly AzureDataExplorerSettings _azureDataExplorerSettings;
        private readonly IAdtApiService _adtApiService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AdxHelper> _logger;
        private static readonly ConcurrentDictionary<string, string> AdxVerifiedTables = new();

        private Dictionary<Guid, Export> _exportStatuses
        {
            get => _memoryCache.GetOrCreate("Exports", (c) =>
                {
                    c.SetPriority(CacheItemPriority.NeverRemove);
                    c.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(12);

                    return new Dictionary<Guid, Export>();
                });
        }

        public AdxHelper(IMemoryCache memoryCache,
            IAdtApiService adtApiService,
            IOptions<AzureDataExplorerSettings> azureDataExplorerSettings, Microsoft.Extensions.Logging.ILogger<AdxHelper> logger)
        {
            _azureDataExplorerSettings = azureDataExplorerSettings.Value;
            _storageAccountConnectionString = $"DefaultEndpointsProtocol=https;AccountName={_azureDataExplorerSettings.BlobStorage.AccountName};AccountKey={_azureDataExplorerSettings.BlobStorage.AccountKey};EndpointSuffix=core.windows.net";
            _memoryCache = memoryCache;
            _adtApiService = adtApiService;
            _logger = logger;
        }

        private IKustoIngestClient _kustoIngestClient
        {
            get => GetCachedKustoClient<IKustoIngestClient>("KustoIngestClient", (c) => KustoIngestFactory.CreateDirectIngestClient(c));
        }

        private ICslQueryProvider _cslQueryProvider
        {
            get => GetCachedKustoClient<ICslQueryProvider>("CslQueryProvider", (c) => KustoClientFactory.CreateCslQueryProvider(c));
        }

        private ICslAdminProvider _cslAdminProvider
        {
            get => GetCachedKustoClient<ICslAdminProvider>("CslAdminProvider", (c) => KustoClientFactory.CreateCslAdminProvider(c));
        }

        private T GetCachedKustoClient<T>(string cacheKey, Func<KustoConnectionStringBuilder, T> getClient)
        {
            return _memoryCache.GetOrCreate<T>(cacheKey, (c) =>
            {
                var kustoUri = $"https://{_azureDataExplorerSettings.Cluster.Name}.{_azureDataExplorerSettings.Cluster.Region}.kusto.windows.net/";
                var accessToken = new DefaultAzureCredential().GetToken(new Azure.Core.TokenRequestContext(new[]
                    {
                        $"{kustoUri}/.default"
                    }));
                var connectionStringBuilder = new KustoConnectionStringBuilder(kustoUri)
                    .WithAadTokenProviderAuthentication(() => accessToken.Token);

                c.SetPriority(CacheItemPriority.NeverRemove);
                c.AbsoluteExpiration = accessToken.ExpiresOn.AddMinutes(-3);

                return getClient(connectionStringBuilder);
            });
        }

        public async Task AppendModel(IDigitalTwinService service, AdtModel adtModel, bool deleted = false)
        {
            await SetupModelsTable(service.SiteAdtSettings.AdxDatabase, AdxConstants.ModelsTable, AdxConstants.DedupModelsView);

            var model = Model.MapFrom(adtModel);

            model.DisplayNames.TryGetValue("en", out var displayName);

            var values = new List<string>
            {
                model.Id,
                model.IsDecommissioned?.ToString(),
                DateTime.UtcNow.ToString(),
                displayName ?? "",
                model.ModelDefinition,
                deleted.ToString()
            };

            await IngestInline(service.SiteAdtSettings.AdxDatabase, AdxConstants.ModelsTable, values);
        }

        #region Export
        public Export QueueExport(Guid siteId, IDigitalTwinService service, SourceInfo source = null)
        {
            var exportStatus = new Export(siteId)
            {
                Id = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                Status = ExportStatus.Queued,
                SourceInformation = source
            };

            _exportStatuses.Add(exportStatus.Id, exportStatus);

            _logger.LogInformation("{LogKey}: Export queued, id: {Id}", _logKey, exportStatus.Id);

            TriggerNextExportInLine(siteId, service);

            return exportStatus;
        }

        private void TriggerNextExportInLine(Guid siteId, IDigitalTwinService service)
        {
            if (_exportStatuses.Any(x => x.Value.Status == ExportStatus.Exporting && x.Value.SiteId == siteId))
                return;

            var nextInLine = _exportStatuses.Select(x => x.Value).OrderBy(x => x.CreateTime).FirstOrDefault(x => x.Status == ExportStatus.Queued);
            if (nextInLine == null)
                return;

            _ = Export(nextInLine, service);
        }

        public async Task AppendTwin(IDigitalTwinService service, BasicDigitalTwin basicDigitalTwin, bool deleted = false, string userId = null)
        {
            var twin = Twin.MapFrom(basicDigitalTwin);
            if (!deleted)
                await DecorateTwin(service, twin);

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                NumberHandling = JsonNumberHandling.Strict,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            options.Converters.Add(new DateTimeConverter());

            var values = new List<string> {
                twin.Id,
                twin.DisplayName,
                twin.SiteId?.ToString(),
                twin.ModelId,
                twin.FloorId?.ToString(),
                twin.UniqueId.ToString(),
                twin.GeometryViewerId?.ToString(),
                twin.TrendId,
                twin.ExternalId,
                twin.ConnectorId?.ToString(),
                twin.Tags,
                twin.ExportTime.ToString("s"),
                JsonSerializer.Serialize(twin, options),
                deleted.ToString(),
                userId?.ToString()
            };

            await IngestInline(service.SiteAdtSettings.AdxDatabase, AdxConstants.TwinsTable, values);
        }

        public async Task AppendRelationship(IDigitalTwinService service, BasicRelationship basicRelationship, bool deleted = false)
        {
            await SetupRelationshipsTable(service.SiteAdtSettings.AdxDatabase, AdxConstants.RelationshipsTable, AdxConstants.DedupRelationshipsView, AdxConstants.ActiveRelationshipsFunction);

            var relationship = Relationship.MapFrom(basicRelationship);

            var values = new List<string> { relationship.Id,
                relationship.SourceId,
                relationship.TargetId,
                relationship.Name,
                relationship.ExportTime.ToString(),
                JsonSerializer.Serialize(relationship),
                deleted.ToString()
            };

            await IngestInline(service.SiteAdtSettings.AdxDatabase, AdxConstants.RelationshipsTable, values);
        }

        public IEnumerable<Export> GetExports(Guid siteId)
        {
            return _exportStatuses.Values.Where(x => x.SiteId == siteId);
        }

        private async Task Export(Export export, IDigitalTwinService service)
        {
            try
            {
                _logger.LogInformation("{LogKey}: Export triggered, id: {Id}", _logKey, export.Id);

                var shouldCreateDump = export.SourceInformation == null;
                if (shouldCreateDump)
                {
                    export.SourceInformation = new SourceInfo
                    {
                        AccountName = _azureDataExplorerSettings.BlobStorage.AccountName,
                        ContainerName = _azureDataExplorerSettings.BlobStorage.ContainerName,
                        Path = $"{export.SiteId}/{export.StartTime.ToString("yyyy.MM.dd.HH.mm.ss")}"
                    };
                }

                export.Status = ExportStatus.Exporting;

                await Task.WhenAll(
                    ExportTwins(export, service, shouldCreateDump),
                    ExportRelationships(export, service, shouldCreateDump),
                    ExportModels(export, service, shouldCreateDump)
                    );

                if (!export.HasErrors)
                {
                    export.Status = ExportStatus.Swapping;

                    var swapTwins = SwapTables(service.SiteAdtSettings.AdxDatabase, AdxConstants.TwinsTable, _twinsAdxTempTable, AdxConstants.DedupTwinsView, GetTwinRowFields());
                    var swapRelationships = SwapTables(service.SiteAdtSettings.AdxDatabase, AdxConstants.RelationshipsTable, _relationshipsAdxTempTable, AdxConstants.DedupRelationshipsView, GetRelationshipsRowFields());
                    var swapModels = SwapTables(service.SiteAdtSettings.AdxDatabase, AdxConstants.ModelsTable, _modelsAdxTempTable, AdxConstants.DedupModelsView, GetModelsRowFields());

                    await Task.WhenAll(swapTwins, swapRelationships, swapModels);

                    export.Status = ExportStatus.CreatingMaterializedViews;

                    await RecreateViews(_cslAdminProvider, service.SiteAdtSettings.AdxDatabase);

                    export.Status = ExportStatus.CreatingFunctions;

                    await RecreateFunctions(_cslAdminProvider, service.SiteAdtSettings.AdxDatabase);
                }

                export.Status = export.HasErrors ? ExportStatus.Error : ExportStatus.Done;
            }
            catch (Exception ex)
            {
                export.Status = ExportStatus.Error;
                export.Error = ex.Message;
                _logger.LogError(ex, "{LogKey}: Error on export job {Id}", _logKey, export.Id);
            }
            finally
            {
                export.EndTime = DateTime.UtcNow;
                _logger.LogInformation("{LogKey}: Export job id {Id} done, export job: {Job}", _logKey, export.Id, JsonSerializer.Serialize(export));
                TriggerNextExportInLine(export.SiteId, service);
            }
        }

        private async Task ExportTwins(Export export, IDigitalTwinService service, bool shouldCreateDump = true)
        {
            export.TwinsExport = new DetailStatus();

            await ExportEntities<Twin>(export.TwinsExport,
                export.SourceInformation.Path,
                AdxConstants.TwinsTable,
                _twinsAdxTempTable,
                _twinsAdxTableMapping,
                (x, y) => SetupTwinsTable(x, y, AdxConstants.DedupTwinsView, AdxConstants.ActiveTwinsFunction),
                async (x, service) =>
                {
                    var twinPage = await service.GetTwinsAsync(continuationToken: x);

                    twinPage.Content = twinPage.Content.Where(x =>
                    {
                        try
                        {
                            _ = x.UniqueId;
                            return true;
                        }
                        catch (DigitalTwinCoreException e)
                        {
                            _logger.LogWarning(e, "Could not export {TwinId} because it does not have a uniqueId", x.Id);
                        }

                        return false;
                    });

                    await Parallel.ForEachAsync(twinPage.Content, async (twin, _) =>
                        await DecorateTwin(service, twin)
                    );

                    return twinPage;
                },
                service,
                shouldCreateDump);
        }

        private async Task<Twin> DecorateTwin(IDigitalTwinService service, Twin twin)
        {
            // SiteId is usually fast, it's ok to await it.
            await AppendSiteId(service, twin);
            // Transient racing condition when appending floorId,
            // do not run in parallel to eliminate a possible cause.
            twin.FloorId = (await service.GetTwinFloor(twin.Id))?.UniqueId;

            twin.ModelId = twin.Metadata?.ModelId;

            return twin;
        }

        private async Task ExportEntities<T>(DetailStatus exportDetails,
            string blobRoot,
            string table,
            string tempTable,
            string mapping,
            Func<string, string, Task> setupTable,
            Func<string, IDigitalTwinService, Task<Page<T>>> getEntities,
            IDigitalTwinService service,
            bool shouldCreateDump)
        {
            exportDetails.StartTime = DateTime.UtcNow;

            try
            {
                var blob = $"{blobRoot}/{table}.jsonl";
                if (shouldCreateDump)
                {
                    var dumpTask = Dump(exportDetails, blob, getEntities, service);
                    await Task.WhenAll(dumpTask);
                }

                await DropTableIfExists(service.SiteAdtSettings.AdxDatabase, tempTable);

                await setupTable(service.SiteAdtSettings.AdxDatabase, tempTable);

                await Ingest(blob, service.SiteAdtSettings.AdxDatabase, tempTable, mapping);
            }
            catch (Exception ex)
            {
                exportDetails.Error = $"Error exporting: {ex.Message}";
            }
            finally
            {
                exportDetails.EndTime = DateTime.UtcNow;
            }
        }

        private async Task Dump<T>(DetailStatus exportDetails, string blob, Func<string, IDigitalTwinService, Task<Page<T>>> getEntities, IDigitalTwinService service)
        {
            string continuationToken = null;
            var serviceClient = new BlobServiceClient(_storageAccountConnectionString);
            var containerClient = serviceClient.GetBlobContainerClient(_azureDataExplorerSettings.BlobStorage.ContainerName);

            await containerClient.CreateIfNotExistsAsync();

            var appendBlobClient = containerClient.GetAppendBlobClient(blob);
            if (!appendBlobClient.Exists())
            {
                await appendBlobClient.CreateAsync();
            }

            var pageIndex = 0;
            var exported = 0;
            do
            {
                var page = await getEntities(continuationToken, service);

                if (page.Content.Any())
                {
                    var content = string.Join("\r\n", page.Content.Select(x => JsonSerializer.Serialize(x)));

                    if (pageIndex > 0)
                        content = $"\r\n{content}";

                    using var stream = new MemoryStream();
                    stream.FromString(content);

                    await appendBlobClient.AppendBlockAsync(stream);

                    exported += page.Content.Count();

                    exportDetails.Details = $"Exported {exported}.";
                }
                continuationToken = page.ContinuationToken;
                pageIndex++;
            }
            while (continuationToken != null);

            await appendBlobClient.SealAsync();
        }

        private async Task ExportRelationships(Export export, IDigitalTwinService service, bool shouldCreateDump = true)
        {
            export.RelationshipsExport = new DetailStatus();

            await ExportEntities<Relationship>(export.RelationshipsExport, export.SourceInformation.Path, AdxConstants.RelationshipsTable, _relationshipsAdxTempTable, _relationshipsAdxTableMapping,
                (x, y) => SetupRelationshipsTable(x, y, AdxConstants.DedupRelationshipsView, AdxConstants.ActiveRelationshipsFunction),
                async (x, service) =>
                {
                    var pageable = _adtApiService.QueryTwins<BasicRelationship>(service.SiteAdtSettings.InstanceSettings, "SELECT * FROM RELATIONSHIPS");

                    var page = await pageable.AsPages(x).FirstAsync();

                    return new Page<Relationship> { Content = page.Values.Select(x => Relationship.MapFrom(x)), ContinuationToken = page.ContinuationToken };
                },
                service,
                shouldCreateDump);
        }

        private async Task ExportModels(Export export, IDigitalTwinService service, bool shouldCreateDump = true)
        {
            export.ModelsExport = new DetailStatus();

            await ExportEntities(
                export.ModelsExport,
                export.SourceInformation.Path,
                AdxConstants.ModelsTable,
                _modelsAdxTempTable,
                _modelsAdxTableMapping,
                (x, y) => SetupModelsTable(x, y, AdxConstants.DedupModelsView),
                async (x, service) =>
                {
                    var models = _adtApiService.GetModels(service.SiteAdtSettings.InstanceSettings);

                    var exportTime = DateTime.UtcNow;
                    models.ForEach(m => m.UploadTime = exportTime);

                    return new Page<Model> { Content = models.Select(Model.MapFrom) };
                },
                service,
                shouldCreateDump);
        }

        private async Task DropTableIfExists(string database, string table)
        {
            var command = $".drop tables({table}) ifexists";
            await _cslAdminProvider.ExecuteControlCommandAsync(database, command);
        }

        private async Task IngestInline(string database, string table, List<string> values)
        {
            var valuesString = string.Join(",", values.Select(x => string.IsNullOrEmpty(x) ? string.Empty : $"\"{x.Replace("\"", "\"\"")}\""));

            var retryPolicy = GetRetryPolicy<Exception>(2, 5);

            await retryPolicy.ExecuteAsync(async () =>
            {
                var command = $".ingest inline into table {table} <| {valuesString}";
                await _cslAdminProvider.ExecuteControlCommandAsync(database, command);
            });
        }

        private async Task CreateMaterializedView(
            ICslAdminProvider queryProvider,
            string database,
            string name,
            string table,
            bool enableCaching = false,
            params string[] groupFields)
        {
            var command = $".drop materialized-view {name} ifexists";
            await queryProvider.ExecuteControlCommandAsync(database, command);

            command = $".create async materialized-view with (backfill=true,autoUpdateSchema=true) {name} on table {table} {{ {table} | summarize arg_max(ExportTime, *) by {string.Join(',', groupFields)} }}";
            await queryProvider.ExecuteControlCommandAsync(database, command);

            if (enableCaching)
            {
                command = $".alter materialized-view {name} policy caching hot = 9999999d";
                await _cslAdminProvider.ExecuteControlCommandAsync(database, command);
            }
        }

        private async Task SwapTables(string database, string table, string tempTable, string dedupView, List<(string, string)> rowFields)
        {
            var retryPolicy = GetRetryPolicy<Exception>(2, 3);

            await retryPolicy.ExecuteAsync(async () =>
            {
                // Create non temp table in case it does not exist
                await AlterOrCreateTable(
                    _cslAdminProvider,
                    database,
                    table,
                    dedupView,
                    rowFields.Select(x => x.ToTuple()).ToArray()
                );

                var command = $".rename tables {table}={tempTable}, {tempTable}={table}";
                await _cslAdminProvider.ExecuteControlCommandAsync(database, command);
            });
        }

        private static List<(string, string)> GetTwinRowFields()
        {
            // Note that ADT API also has its own column definitions for this
            // table. We must keep these in sync with the ADT API values (or even
            // better, just have one place that controls the table schema).
            return new List<(string, string)>
                    {
                        ("Id", "System.String"),
                        ("Name", "System.String"),
                        ("SiteId", "System.Guid"),
                        ("ModelId", "System.String"),
                        ("FloorId", "System.Guid"),
                        ("UniqueId", "System.Guid"),
                        ("GeometryViewerId", "System.Guid"),
                        ("TrendId", "System.Guid"),
                        ("ExternalId", "System.String"),
                        ("ConnectorId", "System.Guid"),
                        ("Tags", "System.Object"),
                        ("ExportTime", "System.DateTime"),
                        ("Raw", "System.Object"),
                        ("Deleted", "System.Boolean"),
                        ("UserId", "System.String"),
                        ("Location", "System.Object")
                    };
        }

        private static List<(string, string)> GetRelationshipsRowFields()
        {
            return new List<(string, string)>
                    {
                        ("Id", "System.String"),
                        ("SourceId", "System.String"),
                        ("TargetId", "System.String"),
                        ("Name", "System.String"),
                        ("ExportTime", "System.DateTime"),
                        ("Raw", "System.Object"),
                        ("Deleted", "System.Boolean")
                    };
        }

        private static List<(string, string)> GetModelsRowFields()
        {
            return new List<(string, string)>
                    {
                        ("Id", "System.String"),
                        ("IsDecommissioned", "System.Boolean"),
                        ("ExportTime", "System.DateTime"),
                        ("DisplayName", "System.String"),
                        ("ModelDefinition", "System.Object"),
                        ("Deleted", "System.Boolean")
                    };
        }

        public async Task SetupTwinsTable(string database, string table, string dedupView, string function)
        {
            await AlterOrCreateTable(
                _cslAdminProvider,
                database,
                table,
                dedupView,
                GetTwinRowFields().Select(x => x.ToTuple()).ToArray()
            );

            await CreateOrAlterTableMapping(
                database,
                table,
                _twinsAdxTableMapping,
                new List<ColumnMapping>
                {
                    CreateColumnMapping("Id", "$.id" ),
                    CreateColumnMapping("Name", "$.displayName" ),
                    CreateColumnMapping("SiteId", "$.siteId" ),
                    CreateColumnMapping("ModelId", "$.modelId" ),
                    CreateColumnMapping("FloorId", "$.floorId" ),
                    CreateColumnMapping("UniqueId", "$.uniqueId" ),
                    CreateColumnMapping("GeometryViewerId", "$.geometryViewerId" ),
                    CreateColumnMapping("TrendId", "$.trendId" ),
                    CreateColumnMapping("ExternalId", "$.externalId" ),
                    CreateColumnMapping("ConnectorId", "$.connectorId" ),
                    CreateColumnMapping("Tags", "$.tags" ),
                    CreateColumnMapping("ExportTime", "$.exportTime" ),
                    CreateColumnMapping("Raw", "$" ),
                    CreateColumnMapping("Deleted", "$.deleted" ),
                    CreateColumnMapping("UserId", "$.userId")
                }
            );
            await CreateOrAlterFunction(_cslAdminProvider, database, function, dedupView, "Twins");
        }

        private ColumnMapping CreateColumnMapping(string columnName, string path)
        {
            return new ColumnMapping
            {
                ColumnName = columnName,
                Properties = new Dictionary<string, string>()
                {
                    { "path", path }
                }
            };
        }

        public async Task SetupRelationshipsTable(string database, string table, string dedupView, string function)
        {
            await AlterOrCreateTable(
                _cslAdminProvider,
                database,
                table,
                dedupView,
                GetRelationshipsRowFields().Select(x => x.ToTuple()).ToArray()
            );

            await CreateOrAlterTableMapping(
                database,
                table,
                _relationshipsAdxTableMapping,
                new List<ColumnMapping>
                {
                    CreateColumnMapping("Id", "$.id" ),
                    CreateColumnMapping("SourceId", "$.sourceId" ),
                    CreateColumnMapping("TargetId", "$.targetId" ),
                    CreateColumnMapping("Name", "$.name" ),
                    CreateColumnMapping("ExportTime", "$.exportTime" ),
                    CreateColumnMapping("Raw", "$" ),
                    CreateColumnMapping("Deleted", "$.deleted" )
                }
            );
            await CreateOrAlterFunction(_cslAdminProvider, database, function, dedupView, "Relationships");
        }

        private async Task SetupModelsTable(string database, string tableName, string dedupView)
        {
            await AlterOrCreateTable(
                this._cslAdminProvider,
                database,
                tableName,
                dedupView,
                GetModelsRowFields().Select(x => x.ToTuple()).ToArray()
            );

            await CreateOrAlterTableMapping(
                database,
                tableName,
                _modelsAdxTableMapping,
                new List<ColumnMapping>
                {
                    CreateColumnMapping("Id", "$.id" ),
                    CreateColumnMapping("IsDecommissioned", "$.isDecommissioned" ),
                    CreateColumnMapping("ExportTime", "$.uploadTime" ),
                    CreateColumnMapping("DisplayName", "$.displayNames.en" ),
                    CreateColumnMapping("ModelDefinition", "$.modelDefinition" ),
                    CreateColumnMapping("Deleted", "$.deleted" )
                }
            );
        }

        private async Task AppendSiteId(IDigitalTwinService service, Twin twin)
        {
            var siteId = twin.GetSiteId();
            if (siteId.HasValue)
            {
                twin.SiteId = siteId.Value;
                return;
            }

            twin.SiteId = await service.GetRelatedSiteId(twin.Id);
        }


        private async Task AlterOrCreateTable(ICslAdminProvider queryProvider, string database, string table, string dedupView, Tuple<string, string>[] rowFields)
        {
            var databaseTable = $"{database}/{table}";
            if (AdxVerifiedTables.ContainsKey(databaseTable)) // tables need to be verified only once per database
            {
                return;
            }

            var showCommand = $".show table {table} cslschema";
            var response = (await queryProvider.ExecuteControlCommandAsync(database, showCommand)).Parse<AdxTableSchemaResponse>().ToArray();

            if (response.Any())
            {
                var columns = response[0].GetColumns().Select(x => x.Item1);
                var fields = rowFields.Select(x => x.Item1);

                if(!columns.OrderBy(t => t).SequenceEqual(fields.OrderBy(t => t)))
                {
                    var alterCommand = CslCommandGenerator.GenerateTableAlterCommand(new TableSchema(table,
                        rowFields.Select(x => new ColumnSchema(x.Item1, x.Item2))));
                    await _cslAdminProvider.ExecuteControlCommandAsync(database, alterCommand);
                    await SetupTableCacheAndView();
                }
            }
            else
            {
                var createCommand = CslCommandGenerator.GenerateTableCreateCommand(
                    table,
                    rowFields);

                await _cslAdminProvider.ExecuteControlCommandAsync(database, createCommand);
                await SetupTableCacheAndView();
            }

            AdxVerifiedTables.TryAdd(databaseTable, databaseTable);

            async Task SetupTableCacheAndView()
            {
                var alterPolicyCommand = $".alter table {table} policy caching hot = 9999999d";
                await _cslAdminProvider.ExecuteControlCommandAsync(database, alterPolicyCommand);
                await CreateMaterializedView(queryProvider, database, dedupView, table, true, "Id");
            }
        }

        private async Task CreateOrAlterTableMapping(string database, string table, string mapping, IEnumerable<ColumnMapping> columnMappings)
        {
            var databaseTableMapping = $"{database}/{table}/{mapping}";
            if (AdxVerifiedTables.ContainsKey(databaseTableMapping)) // tables need to be verified only once per database
            {
                return;
            }

            var commandMap =
                CslCommandGenerator.GenerateTableMappingCreateOrAlterCommand(
                    Kusto.Data.Ingestion.IngestionMappingKind.Json,
                    table,
                    mapping,
                    columnMappings,
                    true);

            await _cslAdminProvider.ExecuteControlCommandAsync(database, commandMap);

            AdxVerifiedTables.TryAdd(databaseTableMapping, databaseTableMapping);
        }

        private async Task CreateOrAlterFunction(ICslAdminProvider queryProvider, string database, string name, string source, string folder)
        {
            var databaseFunctionMapping = $"{database}/{folder}/{name}";
            if (AdxVerifiedTables.ContainsKey(databaseFunctionMapping)) // tables need to be verified only once per database
            {
                return;
            }
            var command = $".create-or-alter function with (folder='{folder}') {name}()  {{ {source} | where isnull(Deleted) or Deleted == false }}";
            await queryProvider.ExecuteControlCommandAsync(database, command);
            AdxVerifiedTables.TryAdd(databaseFunctionMapping, databaseFunctionMapping);
        }

        private async Task Ingest(string blobPath, string database, string table, string mapping)
        {
            var retryPolicy = GetRetryPolicy<Kusto.Ingest.Exceptions.DirectIngestClientException>(30, 5, x => !x.IsPermanent);

            await retryPolicy.ExecuteAsync(async () =>
            {
                var properties =
                        new KustoQueuedIngestionProperties(database, table)
                        {
                            Format = DataSourceFormat.json,
                            IngestionMapping = new IngestionMapping()
                            {
                                IngestionMappingReference = mapping
                            }
                        };

                await _kustoIngestClient.IngestFromStorageAsync(GetServiceSasUriForBlob(blobPath), properties);
            });
        }

        private string GetServiceSasUriForBlob(string blobPath)
        {
            var serviceClient = new BlobServiceClient(_storageAccountConnectionString);
            var containerClient = serviceClient.GetBlobContainerClient(_azureDataExplorerSettings.BlobStorage.ContainerName);

            var blobClient = containerClient.GetBlobClient(blobPath);

            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = "b"
            };

            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddHours(2);
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }
        #endregion

        public async Task<IDataReader> Query(IDigitalTwinService service, string query, CancellationToken cancellationToken = default)
        {
            var retryPolicy = GetRetryPolicy<Exception>(1, 5);
            IDataReader reader = null;

            await retryPolicy.ExecuteAsync(async (cancellationToken) =>
            {
                reader = await _cslQueryProvider.ExecuteQueryAsync(service.SiteAdtSettings.AdxDatabase, "set notruncation;\n" + query, new ClientRequestProperties());
            },
            cancellationToken);

            return reader;
        }

        public async Task<IDataReader> Query(string database, string query, CancellationToken cancellationToken = default)
        {
            var parsedQuery = KustoCode.Parse(query);

            var retryPolicy = GetRetryPolicy<Exception>(1, 5);
            IDataReader reader = null;

            var clientOptions = new Dictionary<string, object>
            {
                { ClientRequestProperties.OptionNoTruncation, true }
            };

            var clientProperties = new ClientRequestProperties(clientOptions, null);

            await retryPolicy.ExecuteAsync(async (cancellationToken) =>
            {
                reader = parsedQuery.Kind switch
                {
                    CodeKinds.Command => await _cslAdminProvider.ExecuteControlCommandAsync(database, parsedQuery.Text, clientProperties),
                    CodeKinds.Query => await _cslQueryProvider.ExecuteQueryAsync(database, parsedQuery.Text, clientProperties),
                    _ => throw new ArgumentException($"Kusto {parsedQuery.Kind} is not supported")
                };
            },
            cancellationToken);

            return reader;
        }

        public static ICslAdminProvider MakeCslAdminProvider(string clusterUri)
        {
            return KustoClientFactory.CreateCslAdminProvider(MakeConnectionStringBuilder(clusterUri));
        }

        public static KustoConnectionStringBuilder MakeConnectionStringBuilder(string clusterUri)
        {
            var accessToken = new DefaultAzureCredential().GetToken(new Azure.Core.TokenRequestContext(new[]
            {
                $"{clusterUri}/.default"
            }));
            return new KustoConnectionStringBuilder(clusterUri)
                .WithAadTokenProviderAuthentication(() => accessToken.Token);
        }

        // <summary>
        // Create tables, materialized views, and functions on the database, but only
        // if there aren't any tables in it currently.
        // </summary>
        public async Task SetupADXIfEmpty(ICslAdminProvider queryProvider, string databaseName)
        {
            var reader = await queryProvider.ExecuteControlCommandAsync(databaseName, ".show tables | count");
            reader.Read();

            if (reader.GetInt64(0) == 0)
            {
                _logger.LogInformation("No tables exist in the ADX database {DatabaseName}; creating them", databaseName);
                await CreateOrUpdateTables(queryProvider, databaseName);
                await RecreateViews(queryProvider, databaseName);
                await RecreateFunctions(queryProvider, databaseName);
            }
            else
            {
                _logger.LogInformation("Tables exist in the ADX database {DatabaseName}; leaving untouched", databaseName);
            }
        }

        // <summary>
        // Potentially destructive operation! Creates or alters tables to meet the current schema,
        // and drops and recreates materialized views and functions.
        // </summary>
        public async Task SetupADX(string databaseName)
        {
            await CreateOrUpdateTables(_cslAdminProvider, databaseName);
            await RecreateViews(_cslAdminProvider, databaseName);
            await RecreateFunctions(_cslAdminProvider, databaseName);
        }

        public async Task CreateOrUpdateTables(
            ICslAdminProvider queryProvider,
            string databaseName)
        {
            await AlterOrCreateTable(queryProvider, databaseName, AdxConstants.TwinsTable, AdxConstants.DedupTwinsView, GetTwinRowFields().Select(x => x.ToTuple()).ToArray());
            await AlterOrCreateTable(queryProvider, databaseName, AdxConstants.RelationshipsTable, AdxConstants.DedupRelationshipsView, GetRelationshipsRowFields().Select(x => x.ToTuple()).ToArray());
            await AlterOrCreateTable(queryProvider, databaseName, AdxConstants.ModelsTable, AdxConstants.DedupModelsView, GetModelsRowFields().Select(x => x.ToTuple()).ToArray());
        }

        public async Task RecreateViews(
            ICslAdminProvider queryProvider,
            string databaseName)
        {
            await CreateMaterializedView(queryProvider, databaseName, AdxConstants.DedupTwinsView, AdxConstants.TwinsTable, true, "Id");
            await CreateMaterializedView(queryProvider, databaseName, AdxConstants.DedupRelationshipsView, AdxConstants.RelationshipsTable, true, "SourceId", "TargetId", "Name");
            await CreateMaterializedView(queryProvider, databaseName, AdxConstants.DedupModelsView, AdxConstants.ModelsTable, false, "Id");
        }

        public async Task RecreateFunctions(
            ICslAdminProvider queryProvider,
            string databaseName)
        {
            await CreateOrAlterFunction(queryProvider, databaseName, AdxConstants.ActiveTwinsFunction, AdxConstants.DedupTwinsView, "Twins");
            await CreateOrAlterFunction(queryProvider, databaseName, AdxConstants.ActiveRelationshipsFunction, AdxConstants.DedupRelationshipsView, "Relationships");
            await CreateOrAlterFunction(queryProvider, databaseName, AdxConstants.ActiveModels, AdxConstants.DedupModelsView, "Models");
        }

        private static AsyncRetryPolicy GetRetryPolicy<T>(int seconds, int retryCount) where T : Exception
        {
            return GetRetryPolicy<T>(seconds, retryCount, _ => true);
        }

        private static AsyncRetryPolicy GetRetryPolicy<T>(int seconds, int retryCount, Func<T, bool> predicate) where T : Exception
        {
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(seconds), retryCount: retryCount);

            return Policy.Handle<T>(predicate).WaitAndRetryAsync(delay);
        }
    }
}
