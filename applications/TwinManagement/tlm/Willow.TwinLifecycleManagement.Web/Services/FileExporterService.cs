using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Azure.DigitalTwins.Core;
using CsvHelper;
using DTDLParser;
using DTDLParser.Models;
using Willow.Model.Adt;
using Willow.TwinLifecycleManagement.Web.Helpers;
using Willow.TwinLifecycleManagement.Web.Models;

namespace Willow.TwinLifecycleManagement.Web.Services
{
    public class FileExporterService : IFileExporterService
    {
        private readonly ILogger<FileExporterService> _logger;
        private readonly ITwinsService _twinService;
        private readonly IModelsService _modelsService;

        private sealed record ModelFileInfo(string Id, string Name);

        public FileExporterService(
            ITwinsService twinsService,
            IModelsService modelsService,
            ILogger<FileExporterService> logger)
        {
            _twinService = twinsService;
            _modelsService = modelsService;
            _logger = logger;
        }

        public async Task<byte[]> ExportZippedTwinsAsync(
            string locationId,
            string[] modelIds,
            bool? exactModelMatch,
            bool? includeRelationships,
            bool? includeIncomingRelationships,
            bool? isTemplateExportOnly)
        {
            _logger.LogInformation(
                "ExportZippedTwinsAsync: Starting export:" +
                        " models:{TLMModels}, location:{TLMLocationId}, exactMatch:{TLMExactModelMatch}" +
                        " inclOutRels: {TLMIncludeInRels}, inclInRels:{TLMIncludeOutRels}",
                JsonSerializer.Serialize(modelIds),
                locationId,
                exactModelMatch,
                includeRelationships,
                includeIncomingRelationships);

            var stopwatch = Stopwatch.StartNew();
            var csvFiles = new ConcurrentDictionary<ModelFileInfo, byte[]>();

            var allModels = (await _modelsService.GetModelsInterfaceInfoAsync()).ToList();
            var inputModels = modelIds.Length == 0
                // User didn't specify models, so use all models with any twins present
                ? allModels.Where(m => m.ExactCount > 0).ToList()
                // Otherwise include any parent or child models with twins present from the user's list
                : allModels.Where(m => modelIds.Contains(m.Id)).ToList();

            if (isTemplateExportOnly == true)
            {
                // Produce template file for each model type (include children models if user selected)
                var exportTasks = modelIds.Length == 0
                    ? allModels.Select(m => ProcessTemplateExport(m, exactModelMatch: true, csvFiles)).ToList()
                    : inputModels.Select(m => ProcessTemplateExport(m, exactModelMatch, csvFiles)).ToList();
                await Task.WhenAll(exportTasks);
            }
            else
            {
                // Produce export files with twins
                var modelsWithTwins = inputModels.Where(m => m.TotalCount > 0);

                // If user select no modelIds, system will pull the all the models from ADT
                // so we don't want to include child models from adt api response since we are getting all twins
                if (!modelIds.Any())
                {
                    exactModelMatch = true;
                }

                _logger.LogInformation(
                    "ExportZippedTwinsAsync: ModelList: {TLMModelList}",
                    JsonSerializer.Serialize(modelsWithTwins.Select(m => new { m.Id, m.ExactCount, m.TotalCount })));

                await ProcessModelWithTwins(
                    modelsWithTwins,
                    allModels,
                    locationId,
                    exactModelMatch,
                    includeRelationships,
                    includeIncomingRelationships,
                    csvFiles);
            }

            return await GetCsvBytes(csvFiles, locationId, modelIds, exactModelMatch, includeRelationships, allModels, stopwatch);
        }

        private async Task ProcessModelWithTwins(
            IEnumerable<InterfaceTwinsInfo> modelsWithTwins,
            List<InterfaceTwinsInfo> allModels,
            string locationId,
            bool? exactModelMatch,
            bool? includeRelationships,
            bool? includeIncomingRelationships,
            ConcurrentDictionary<ModelFileInfo, byte[]> csvFiles)
        {
            var queries = new ExportRequestBalancer().Balance(modelsWithTwins);

            _logger.LogInformation(
                "ExportZippedTwinsAsync: Balanced queries: {TLMQueries}",
                JsonSerializer.Serialize(queries.Select(sq => sq.ParallelRequests.Select(pq => pq.Models.Select(m => m.Id)))));

            foreach (var sequentialQuery in queries)
            {
                var parallelRequestsTasks = sequentialQuery.ParallelRequests.Select(async r =>
                {
                    var startTime = Stopwatch.StartNew();

                    await ProcessRequestAsync(
                        r.Models,
                        allModels,
                        locationId,
                        exactModelMatch,
                        includeRelationships,
                        includeIncomingRelationships,
                        csvFiles);

                    _logger.LogInformation(
                        "ExportZippedTwinsAsync: Processed parallel request: duration:{TLMDuration}, seq:{TLMReqSeq}, par:{TLMReqPar}  models:{TLMModels}",
                        startTime.Elapsed,
                        queries.IndexOf(sequentialQuery),
                        sequentialQuery.ParallelRequests.IndexOf(r),
                        JsonSerializer.Serialize(r.Models.Select(m => m.Id)));
                });

                await Task.WhenAll(parallelRequestsTasks);
            }
        }

        private static async Task<byte[]> GetCsvBytes(
            ConcurrentDictionary<ModelFileInfo, byte[]> csvFiles,
            string locationId,
            string[] modelIds,
            bool? exactModelMatch,
            bool? includeRelationships,
            List<InterfaceTwinsInfo> allModels,
            Stopwatch stopwatch)
        {
            using var compressedStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(compressedStream, ZipArchiveMode.Create, leaveOpen: false))
            {
                foreach (var ((id, name), csvBytes) in csvFiles)
                {
                    var entry = zipArchive.CreateEntry($"{name}.csv");

                    using var file = new MemoryStream(csvBytes);
                    await using var entryStream = entry.Open();
                    await file.CopyToAsync(entryStream);
                }

                stopwatch.Stop();
                var statsEntry = zipArchive.CreateEntry("_ExportDetails.txt");
                var statsBytes = Encoding.Unicode.GetBytes(GetExportDetails(locationId, allModels, modelIds, includeRelationships, exactModelMatch, stopwatch));
                using var statsFile = new MemoryStream(statsBytes);
                await using var statsEntryStream = statsEntry.Open();
                await statsFile.CopyToAsync(statsEntryStream);
            }

            return compressedStream.ToArray();
        }

        public async Task<byte[]> ExportZippedTwinsByTwinIdsAsync(string[] twinIds)
        {
            var stopwatch = Stopwatch.StartNew();

            var twins = new List<TwinWithRelationships>();
            foreach (var twinId in twinIds)
            {
                var twin = await _twinService.GetTwinAsync(twinId);
                if (twin != null)
                {
                    twins.Add(twin);
                }
            }
            using var compressedStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(compressedStream, ZipArchiveMode.Create, leaveOpen: false))
            {
                var model = await _modelsService.GetModelAsync("dtmi:com:willowinc:Document;1");
                var twinBytes = WriteCsvToMemory(model, twins);
                var entry = zipArchive.CreateEntry($"Document Twins.csv");
                using var file = new MemoryStream(twinBytes);
                await using (var entryStream = entry.Open())
                {
                    await file.CopyToAsync(entryStream);
                }

                stopwatch.Stop();
                var statsEntry = zipArchive.CreateEntry("_ExportDetails.txt");
                var statsBytes = Encoding.Unicode.GetBytes(GetExportDetails(
                    null,
                    new List<InterfaceTwinsInfo>(),
                    new string[] { "dtmi:com:willowinc:Document;1" },
                    true,
                    false,
                    stopwatch));
                using var statsFile = new MemoryStream(statsBytes);
                await using (var statsEntryStream = statsEntry.Open())
                {
                    await statsFile.CopyToAsync(statsEntryStream);
                }
            }

            return compressedStream.ToArray();
        }

        #region private

        private async Task ProcessRequestAsync(InterfaceTwinsInfo[] models, List<InterfaceTwinsInfo> allModels, string locationId, bool? exactModelMatch,
            bool? includeRelationships,
            bool? includeIncomingRelationships, ConcurrentDictionary<ModelFileInfo, byte[]> csvFiles)
        {
            var modelIds = models.Select(m => m.Id).ToArray();
            var twins = (await _twinService.GetAllTwinsAsync(
                                                            locationId,
                                                            modelIds,
                                                            exactModelMatch == true,
                                                            includeRelationships == true,
                                                            includeIncomingRelationships == true)).ToList();

            if (!twins.Any())
                return;

            var twinsModelIds = twins.Select(i => i.Twin.Metadata.ModelId).Distinct().ToArray();

            foreach (var item in twinsModelIds)
            {
                var modelTwins = twins.Where(t => t.Twin.Metadata.ModelId == item).ToList();
                InterfaceTwinsInfo model = allModels.First(m => m.Id == item);

                if (modelTwins.Any())
                {
                    ProcessModelsTwinsExport(model, modelTwins, csvFiles);
                }
            }
        }

        private async Task ProcessTemplateExport(
            InterfaceTwinsInfo model,
            bool? exactModelMatch,
            ConcurrentDictionary<ModelFileInfo, byte[]> csvFiles)
        {
            bool includeChildren = exactModelMatch != true;
            if (includeChildren)
            {
                var modelFamily = await _modelsService.GetModelFamilyAsync(model.Id);

                // Export each model template of the whole family
                foreach (var currentModel in modelFamily)
                {
                    ProcessModelsTwinsExport(currentModel, twins: null, csvFiles);
                }
            }
            else
            {
                // Export only user specified model template.
                ProcessModelsTwinsExport(model, twins: null, csvFiles);
            }
        }

        private void ProcessModelsTwinsExport(
            InterfaceTwinsInfo model,
            IEnumerable<TwinWithRelationships> twins,
            ConcurrentDictionary<ModelFileInfo, byte[]> csvFiles)
        {
            var modelId = model.Id;
            var modelName = model.EntityInfo.DisplayName.FirstOrDefault().Value.Replace('/', '-');
            var fileName = twins?.Any() == true ? modelName : $"_Template_{modelName}";

            var modelInfo = new ModelFileInfo(modelId, fileName);
            // The process currently produce duplicate files for the same model id.
            // This could be due to "include child models" selection.
            bool isExisted = csvFiles.ContainsKey(modelInfo);
            _logger.LogInformation($"{modelInfo.Id} existed in csv files: {isExisted}");
            // Check dup content for now. Set to false to reduce compute time when we're sure the content always the same.
            bool checkDuplicatedContent = true;
            if (!isExisted)
            {
                var csvBytes = WriteCsvToMemory(model.EntityInfo, twins);
                bool isAdded = csvFiles.TryAdd(modelInfo, csvBytes);
                _logger.LogInformation($"Add {modelInfo.Id} to csv files: {isAdded}");
            }
            else if (csvFiles.ContainsKey(modelInfo))
            {
                _logger.LogError($"TLM Export: Ignored attempt to add csv file for the existing model {modelInfo.Name}");
            }
        }

        private static string GetExportDetails(string locationId, IEnumerable<InterfaceTwinsInfo> allModels,
            string[] modelIds, bool? includeRelationships, bool? exactModelMatch, Stopwatch stopwatch)
        {
            void AppendModelTable(StringBuilder stringBuilder, List<InterfaceTwinsInfo> models)
            {
                if (!models.Any())
                {
                    stringBuilder.AppendLine("- No models -");
                    return;
                }

                var nameLength = models.Max(m => m.EntityInfo.DisplayName.FirstOrDefault().Value.Length);
                var idLength = models.Max(m => m.Id.Length);
                const int countLength = 7;

                var title = $" {"Name".PadRight(nameLength)} | {"Id".PadRight(idLength)} | {"Count".PadRight(countLength)}";
                var delimiter = "".PadRight(title.Length, '-');

                stringBuilder.AppendLine(title);
                stringBuilder.AppendLine(delimiter);
                foreach (var m in models)
                {
                    var name = m.EntityInfo.DisplayName.FirstOrDefault().Value;
                    stringBuilder.AppendLine($" {name.PadRight(nameLength)} | {m.Id.PadRight(idLength)} | {m.ExactCount.ToString().PadRight(countLength)}");
                }

                stringBuilder.AppendLine(delimiter);
            }

            var elapsedTime = stopwatch.Elapsed.ToString(@"d\.hh\:mm\:ss");
            var allModelInfos = allModels.ToList();
            var requestedModels = allModelInfos.Where(i => !modelIds.Any() || modelIds.Contains(i.Id)).ToList();
            var builder = new StringBuilder();

            builder.AppendLine("Export details");
            builder.AppendLine("==============");
            builder.AppendLine();
            builder.AppendLine($"Completed at:           {DateTime.Now:U} UTC");
            builder.AppendLine($"Elapsed time:           {elapsedTime}");
            builder.AppendLine($"Location:               {locationId ?? "Any"}");
            builder.AppendLine($"Include relationships:  {((includeRelationships ?? false) ? "Yes" : "No")}");
            builder.AppendLine($"Include children:       {((!exactModelMatch ?? false) ? "Yes" : "No")}");
            builder.AppendLine();
            builder.AppendLine($"Total model count:      {allModelInfos.Count}");
            builder.AppendLine($"Requested model count:  {(modelIds.Length > 0 ? modelIds.Length : "all")}");
            builder.AppendLine($"Requested models:       {(modelIds.Length > 0 ? string.Join(",", modelIds) : "all")}");
            builder.AppendLine($"Retrieved model count:  {requestedModels.Count(m => m.ExactCount > 0)}");
            builder.AppendLine($"Twin-less model count:  {requestedModels.Count(m => m.ExactCount == 0)}");

            if (requestedModels.Any(m => m.ExactCount == 0))
            {
                builder.AppendLine();
                builder.AppendLine();
                builder.AppendLine("Twin-less Models");
                builder.AppendLine("================");

                if (modelIds.Any())
                {
                    var emptyModels = requestedModels
                        .Where(m => m.ExactCount == 0)
                        .OrderBy(m => m.EntityInfo.DisplayName.FirstOrDefault().Value);

                    builder.AppendLine();
                    AppendModelTable(builder, emptyModels.ToList());
                }
                else
                {
                    builder.AppendLine($"{requestedModels.Count(m => m.ExactCount == 0)} Models in the ADT instance have no Twins.");
                }
            }

            return builder.ToString();
        }

        private byte[] WriteCsvToMemory(
            DTInterfaceInfo interfaceInfo,
            IEnumerable<TwinWithRelationships> twinsWithRelationships)
        {
            var enumeratedTwins = twinsWithRelationships?.ToList();
            var twinFileColumns = GetTwinFileColumns(interfaceInfo, enumeratedTwins);
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            WriteHeader(csv, twinFileColumns);
            enumeratedTwins?.ForEach(twin =>
            {
                csv.NextRecord();
                WriteRecord(csv, twinFileColumns, twin);
            });

            csv.Flush();
            return memoryStream.ToArray();
        }

        private void WriteRecord(CsvWriter csv, List<TwinFileColumn> twinFileColumns, TwinWithRelationships twin)
        {
            foreach (var column in twinFileColumns)
            {
                csv.WriteField(GetFieldValue(twin, column));
            }
        }

        private static Dictionary<string, object> GetMapContent(string json)
        {
            try
            {
                var map = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                return map;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static object GetMapValue(Dictionary<string, object> map, IEnumerable<string> keys)
        {
            foreach (var key in keys)
            {
                if (map.ContainsKey(key))
                {
                    var innerMap = GetMapContent(map[key].ToString());
                    if (innerMap == null)
                        return map[key];
                    else
                    {
                        return GetMapValue(innerMap, keys.Skip(1));
                    }
                }
            }

            return null;
        }

        private object GetFieldValue(TwinWithRelationships twin, TwinFileColumn column)
        {
            try
            {
                if (column.ContentInfo == null)
                {
                    if (column.Name == "model")
                    {
                        return twin.Twin.Metadata.ModelId;
                    }
                    else if (column.Name == "id")
                    {
                        return twin.Twin.Id;
                    }
                }
                else if (column.ContentInfo.EntityKind == DTEntityKind.Property)
                {
                    if ((column.ContentInfo as DTPropertyInfo).Schema.EntityKind == DTEntityKind.Map)
                    {
                        if (!twin.Twin.Contents.ContainsKey(column.ContentInfo.Name))
                        {
                            return null;
                        }

                        var keys = column.Name.Split(".").Skip(1).ToList();
                        var map = GetMapContent(twin.Twin.Contents[column.ContentInfo.Name].ToString());
                        return GetMapValue(map, keys);
                    }
                    else if (twin.Twin.Contents.ContainsKey(column.ContentInfo.Name))
                    {
                        if (column.PropertyFieldInfo != null &&
                            twin.Twin.Contents[column.ContentInfo.Name] is JsonElement propertyObject)
                        {
                            // check if it is immediate property
                            if (propertyObject.TryGetProperty(column.PropertyFieldInfo.Name, out var subProperty))
                            {
                                return ParsePropertyValue(subProperty);
                            }

                            // if not, then the property value may be nested inside another objects
                            // traverse the path to get the target field value
                            foreach (var subPath in column.Name.Split('.').Skip(1))
                            {
                                if (propertyObject.TryGetProperty(subPath, out subProperty))
                                {
                                    propertyObject = subProperty;
                                }
                                else
                                {
                                    return null;
                                }
                            }

                            return ParsePropertyValue(subProperty);
                        }
                        else
                        {
                            if (column.ContentInfo.Name == "tags")
                            {
                                return ParseTags(twin.Twin.Contents[column.ContentInfo.Name]);
                            }

                            return twin.Twin.Contents[column.ContentInfo.Name];
                        }
                    }
                }
                else if (column.ContentInfo.EntityKind == DTEntityKind.Relationship)
                {
                    var twinRelationships =
                        twin.OutgoingRelationships?.Where(r => r.Name == column.ContentInfo.Name).ToList();

                    if (twinRelationships?.Count > column.Index)
                    {
                        // Case where the column name is not a 3 part relationship property
                        // but we still want to return the TargetId for that relationship
                        if (column.RelationshipPropertyInfo is null || twinRelationships[column.Index].Properties.Count == 0)
                        {
                            return twinRelationships[column.Index].TargetId;
                        }

                        // Case where column name contains 3 part relationship properties
                        var threePartmatchingRelationships = new List<BasicRelationship>();
                        foreach (var relationship in twinRelationships)
                        {
                            if (column.RelationshipPropertyInfo != null && relationship.Properties.TryGetValue(column.RelationshipPropertyInfo.Name, out object value) &&
                                (column.Name == $"{relationship.Name}.{column.RelationshipPropertyInfo.Name}.{value}"))
                            {
                                threePartmatchingRelationships.Add(relationship);
                            }

                            if (threePartmatchingRelationships.Count > column.Index)
                            {
                                return threePartmatchingRelationships[column.Index].TargetId;
                            }
                        }
                    }
                }
                else if (column.ContentInfo.EntityKind == DTEntityKind.Component &&
                         twin.Twin.Contents.ContainsKey(column.ContentInfo.Name) &&
                         twin.Twin.Contents[column.ContentInfo.Name] is JsonElement propertyObject &&
                         propertyObject.TryGetProperty(column.PropertyInfo.Name, out var componentProperty))
                {
                    if (column.PropertyFieldInfo == null)
                    {
                        // Component property value
                        return ParsePropertyValue(componentProperty);
                    }
                    else
                    {
                        if (componentProperty.TryGetProperty(column.PropertyFieldInfo.Name, out var componentSubProperty))
                        {
                            // Component property is object - return sub property value
                            return ParsePropertyValue(componentSubProperty);
                        }
                    }
                }
            }
            catch (Exception)
            {
                _logger.LogError("TLM Export: Error while retrieving field {FieldName} from the twin {Id}", column.Name, twin.Twin.Id);
                _logger.LogError("Error twin payload:{Twin}", JsonSerializer.Serialize(twin));
                throw;
            }
            return null;
        }

        private static string ParseTags(object tags)
        {
            return string.Join(',', ((JsonElement)tags).EnumerateObject().Select(x => x.Name));
        }

        private object ParsePropertyValue(JsonElement componentProperty)
        {
            try
            {
                switch (componentProperty.ValueKind)
                {
                    case JsonValueKind.String:
                        return componentProperty.GetString();
                    case JsonValueKind.Number:
                        if (componentProperty.TryGetInt64(out long parsedLong))
                        {
                            return parsedLong;
                        }
                        else
                        {
                            return componentProperty.GetDecimal();
                        }

                    case JsonValueKind.True:
                        return true;
                    case JsonValueKind.False:
                        return false;
                    case JsonValueKind.Object:
                        {
                            string stringValue = null;

                            // Enumerating objects as we need to get the value from a multi part column property
                            foreach (var item in componentProperty.EnumerateObject())
                            {
                                var value = ParsePropertyValue(item.Value);
                                stringValue = value.ToString();
                            }

                            return stringValue;
                        }

                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                _logger.LogError("TLM Export: Error parsing the value:{Value}", componentProperty.GetRawText());
                throw;
            }
        }

        private static void WriteHeader(CsvWriter csv, List<TwinFileColumn> twinFileColumns)
        {
            foreach (var column in twinFileColumns)
            {
                csv.WriteField(column.Name);
            }
        }

        private static List<TwinFileColumn> GetTwinFileColumnFromProperty(DTContentInfo item)
        {
            var output = new List<TwinFileColumn>();
            var propItem = item as DTPropertyInfo;

            // Check if the property schema is Object Kind
            if (propItem.Schema.EntityKind == DTEntityKind.Object)
            {
                output.AddRange(GetTwinColumnsFromNestedFieldObject(item, propItem.Name, propItem.Schema as DTObjectInfo));
            }
            else
            {
                output.Add(new TwinFileColumn { Name = $"{item.Name}", ContentInfo = item });
            }

            return output;
        }

        private static List<TwinFileColumn> GetTwinColumnsFromNestedFieldObject(DTContentInfo contentInfo, string parentName, DTObjectInfo objectField)
        {
            List<TwinFileColumn> output = new();

            foreach (var subField in objectField.Fields)
            {

                var localFieldName = $"{parentName}.{subField.Name}";
                if (subField.Schema.EntityKind == DTEntityKind.Object)
                {
                    output.AddRange(GetTwinColumnsFromNestedFieldObject(contentInfo, localFieldName, subField.Schema as DTObjectInfo));
                }
                else
                {
                    output.Add(new TwinFileColumn
                    {
                        Name = localFieldName,
                        ContentInfo = contentInfo,
                        PropertyFieldInfo = subField
                    });
                }
            }

            return output;
        }

        private static List<TwinFileColumn> GetTwinFileColumnFromComponent(DTContentInfo item)
        {
            var output = new List<TwinFileColumn>();
            foreach (var subItem in (item as DTComponentInfo).Schema.Contents.Values.OfType<DTPropertyInfo>())
            {
                if (subItem.Schema.EntityKind == DTEntityKind.Object)
                {
                    foreach (var componentPropertySubItem in (subItem.Schema as DTObjectInfo).Fields)
                    {
                        output.Add(new TwinFileColumn
                        {
                            Name = $"{item.Name}.{subItem.Name}.{componentPropertySubItem.Name}",
                            ContentInfo = item,
                            PropertyInfo = subItem,
                            PropertyFieldInfo = componentPropertySubItem,
                        });
                    }
                }
                else
                {
                    output.Add(new TwinFileColumn
                    { Name = $"{item.Name}.{subItem.Name}", ContentInfo = item, PropertyInfo = subItem });
                }
            }

            return output;
        }

        private static List<TwinFileColumn> GetTwinFileColumnFromRelationshipInfo(DTContentInfo item)
        {
            var relationshipColumns = new List<TwinFileColumn>();
            var relationshipInfo = item as DTRelationshipInfo;
            var props = relationshipInfo.Properties;
            if (props.Any())
            {
                props.ToList().ForEach(propertyInfo =>
                {
                    relationshipColumns.Add(new TwinFileColumn
                    {
                        Name = relationshipInfo.Name,
                        ContentInfo = relationshipInfo,
                        RelationshipPropertyInfo = propertyInfo,
                    });
                });
            }
            else
            {
                relationshipColumns.Add(new TwinFileColumn { ContentInfo = relationshipInfo, Name = relationshipInfo.Name });
            }

            return relationshipColumns;
        }

        private static List<TwinFileColumn> GetTwinFileColumnFromTwinRelColumnNames(DTRelationshipInfo relationshipInfo, DTPropertyInfo propertyInfo, IEnumerable<string> twinsRelationshipColumnNames)
        {
            var relationshipColumns = new List<TwinFileColumn>();
            int i = 0;
            foreach (var columnName in twinsRelationshipColumnNames)
            {
                relationshipColumns.Add(new TwinFileColumn
                {
                    Name = columnName,
                    ContentInfo = relationshipInfo,
                    RelationshipPropertyInfo = propertyInfo,
                    Index = i++,
                });
            }

            return relationshipColumns;
        }

        private static List<TwinFileColumn> GetTwinFileColumnFromTwinRelationships(DTContentInfo item, DTRelationshipInfo relationshipInfo, List<TwinWithRelationships> twinsWithRelationships)
        {
            var relationshipColumns = new List<TwinFileColumn>();
            foreach (var propertyInfo in relationshipInfo.Properties)
            {
                var outgoingRelationships = twinsWithRelationships
                    .Where(x => x.OutgoingRelationships != null)
                    .SelectMany(t => t.OutgoingRelationships);

                var relationshipColumnByPropValues = outgoingRelationships
                        .Where(r => r.Name == item.Name && r.Properties != null && r.Properties
                            .ContainsKey(propertyInfo.Name)).GroupBy(g => Convert.ToString(g.Properties[propertyInfo.Name]));

                if (relationshipColumnByPropValues.Any())
                {
                    foreach (var group in relationshipColumnByPropValues)
                    {
                        int maxRelCount = group.GroupBy(t => t.SourceId).Max(m => m.Count());
                        var relationshipColumnsProps = GetTwinFileColumnFromTwinRelColumnNames(relationshipInfo, propertyInfo, Enumerable.Repeat($"{item.Name}.{propertyInfo.Name}.{group.Key}", maxRelCount));
                        relationshipColumns.AddRange(relationshipColumnsProps);
                    }
                }
                else if (outgoingRelationships.Any())
                {
                    var r = GetTwinFileColumnFromTwinRelColumnNames(relationshipInfo, propertyInfo, outgoingRelationships.Select(x => x.Name));
                    relationshipColumns.AddRange(r);
                }

            }

            return relationshipColumns;
        }

        private static List<TwinFileColumn> GetTwinFileColumnFromRelationship(DTContentInfo item, List<TwinWithRelationships> twinsWithRelationships)
        {
            var relationshipColumns = new List<TwinFileColumn>();
            var relationshipInfo = item as DTRelationshipInfo;

            var relationshipCount = twinsWithRelationships
                .Select(t => t.OutgoingRelationships?.Count(r => r.Name == item.Name)).Max();

            if (relationshipCount > 0 && relationshipInfo.Properties.Any())
            {
                relationshipColumns = GetTwinFileColumnFromTwinRelationships(item, relationshipInfo, twinsWithRelationships);
            }
            else if (relationshipCount > 0)
            {
                for (int i = 0; i < relationshipCount; i++)
                {
                    relationshipColumns.Add(new TwinFileColumn { ContentInfo = relationshipInfo, Name = relationshipInfo.Name, Index = i });
                }
            }
            else
            {
                // Get relationship properties even no twins have these relationships.
                // Allow export of all relationship columns even no twins have these relationships.
                relationshipColumns = GetTwinFileColumnFromRelationshipInfo(item);
            }

            return relationshipColumns;
        }

        private static List<TwinFileColumn> GetTwinFileColumns(
            DTInterfaceInfo interfaceInfo,
            List<TwinWithRelationships> twinsWithRelationships)
        {
            var output = new List<TwinFileColumn> {
                new TwinFileColumn { Name = "model" },
                new TwinFileColumn { Name = "id" },
            };

            var mapOfMaps = new Dictionary<string, DTPropertyInfo>();

            foreach (var item in interfaceInfo.Contents.Values)
            {
                bool isPropertyKind = item.EntityKind == DTEntityKind.Property;
                if (isPropertyKind)
                {
                    var propItem = item as DTPropertyInfo;
                    bool isMapKind = propItem.Schema.EntityKind == DTEntityKind.Map;
                    if (isMapKind)
                    {
                        mapOfMaps.TryAdd(propItem.Name, propItem);
                    }
                    else
                    {
                        output.AddRange(GetTwinFileColumnFromProperty(item));
                    }
                }
                else if (item.EntityKind == DTEntityKind.Component)
                {
                    var twinFileColumns = GetTwinFileColumnFromComponent(item);
                    output.AddRange(twinFileColumns);
                }
                else if (item.EntityKind == DTEntityKind.Relationship)
                {
                    var twinFileColumns = twinsWithRelationships?.Any() == true
                        ? GetTwinFileColumnFromRelationship(item, twinsWithRelationships)
                        : GetTwinFileColumnFromRelationshipInfo(item);
                    output.AddRange(twinFileColumns);
                }
            }

            GetTwinFileColumnFromMap(output, mapOfMaps, twinsWithRelationships);

            return output;
        }

        private static void GetTwinFileColumnFromMap(
            List<TwinFileColumn> output,
            Dictionary<string, DTPropertyInfo> mapOfMaps,
            List<TwinWithRelationships> twinsWithRelationships)
        {
            if (mapOfMaps.Any() && twinsWithRelationships?.Any() == true)
            {
                var twinsMaps = twinsWithRelationships
                    .Where(x => x.Twin.Contents.Any(c => mapOfMaps.ContainsKey(c.Key))).ToList();

                foreach (var map in mapOfMaps)
                {
                    var filteredTwins = twinsMaps.Where(x => x.Twin.Contents.ContainsKey(map.Key)).ToList();

                    filteredTwins.ForEach(x =>
                    {
                        var json = GetMapContent(x.Twin.Contents[map.Key].ToString());

                        AddMapColumns(output, json, map.Value);
                    });
                }
            }
        }

        private static void AddMapColumns(List<TwinFileColumn> twinFileColumns, Dictionary<string, object> map, DTPropertyInfo item, string parentsNodeName = "")
        {
            foreach (var mapContent in map)
            {
                var innerMap = GetMapContent(mapContent.Value.ToString());
                if (innerMap != null)
                {
                    parentsNodeName += $".{mapContent.Key}";

                    AddMapColumns(twinFileColumns, innerMap, item, parentsNodeName);

                    parentsNodeName = string.Empty;
                }
                else
                {
                    var name = $"{item.Name}{parentsNodeName}.{mapContent.Key}";
                    if (twinFileColumns.All(x => x.Name != name))
                    {
                        twinFileColumns.Add(new TwinFileColumn { Name = name, ContentInfo = item });
                    }
                }
            }
        }

        #endregion
    }
}
