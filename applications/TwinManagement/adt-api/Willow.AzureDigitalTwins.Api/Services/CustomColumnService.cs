using Azure.DigitalTwins.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Api.Common.Extensions;
using Willow.AzureDigitalTwins.Api.Custom;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;
using Willow.Model.Adx.Model;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Services;

/// <summary>
/// Interface for Calculating Twin, Model and Relationship custom columns
/// </summary>
public interface ICustomColumnService
{
    /// <summary>
    /// Method to calculate all entity columns (with optional filter) based on the cached adx schema
    /// </summary>
    /// <typeparam name="T">Type of Twin, Model or Relationship</typeparam>
    /// <param name="entity">Typed entity</param>
    /// <param name="customColumnDestination">Entity Type</param>
    /// <param name="deleted">Bool indicating whether columns are being calculated for deleted entity.</param>
    /// <param name="columnFilter">Predicate to filter the list of columns to calculate.</param>
    /// <param name="reEvaluate">True: Force re evaluate custom column values. False: Reuse existing values.</param>
    /// <returns>Dictionary of export column as key, and column value as value.</returns>
    public Task<IDictionary<ExportColumn, string>> CalculateEntityColumns<T>(T entity,
        EntityType customColumnDestination,
        bool deleted = false,
        Func<ExportColumn, bool> columnFilter = null,
        bool reEvaluate = false);

    /// <summary>
    ///  Method to calculate individual entity column
    /// </summary>
    /// <typeparam name="T">Type Entity Twin, Model or Relationship</typeparam>
    /// <param name="column">ExportColumn</param>
    /// <param name="entity">Typed Entity</param>
    /// <param name="jsonSerialized">Entity Serialized Version</param>
    /// <param name="jObject">Entity JObject Instance</param>
    /// <param name="deleted">Bool indicating whether columns are being calculated for deleted entity.</param>
    /// <param name="reEvaluate">True: Force re evaluate custom column values. False: Reuse existing values.</param>
    /// <returns>Calculated Value as string</returns>
    public Task<string> CalculateColumn<T>(ExportColumn column,
    T entity,
    JsonElement jsonSerialized,
    JObject jObject,
    bool deleted = false,
    bool reEvaluate = false);

    /// <summary>
    /// Calculate and return uniqueId column for twin
    /// </summary>
    /// <param name="adtPropName"> Adt Prop Name</param>
    /// <param name="twin">Requested twin in context</param>
    /// <param name="existingTwinFunc">Function to get the existing twin in ADT; if not null</param>
    /// <returns>Awaitable task to get unique Id.</returns>
    public Task<string> GetUniqueIdForTwin(string adtPropName, BasicDigitalTwin twin, Func<Task<BasicDigitalTwin>> existingTwinFunc);

    /// <summary>
    /// Calculate and return TrendId column for twin
    /// </summary>
    /// <param name="adtPropName">ADT Property Name</param>
    /// <param name="twin">Requested Twin</param>
    /// <param name="existingTwinFunc">Function to get the existing Twin, if not null</param>
    /// <returns>Awaitable task to return trend Id.</returns>
    public Task<string> GetTrendIdForTwin(string adtPropName, BasicDigitalTwin twin, Func<Task<BasicDigitalTwin>> existingTwinFunc);

}

/// <summary>
/// Class implementation for Calculating Twin, Model and Relationship custom columns
/// </summary>
public class CustomColumnService : ICustomColumnService
{
    private readonly ILogger<CustomColumnService> _logger;
    private readonly IAdxSetupService _adxSetupService;
    private readonly IAzureDigitalTwinReader _azureDigitalTwinsReader;
    private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;
    private readonly IMemoryCache _memoryCache;

    private ConcurrentDictionary<string, string> _uniqueIdMap = null;
    const string UniqueIdMappingCacheEntry = "UniqueIdMapping";
    private readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public CustomColumnService(ILogger<CustomColumnService> logger,
        IAzureDigitalTwinReader azureDigitalTwinsReader,
        IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
        IAdxSetupService adxSetupService,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _azureDigitalTwinsReader = azureDigitalTwinsReader;
        _azureDigitalTwinModelParser = azureDigitalTwinModelParser;
        _adxSetupService = adxSetupService;
        _memoryCache = memoryCache;
        jsonSerializerOptions.Converters.Add(new DateTimeOffsetConverterWithoutOffset());
    }

    /// <summary>
    /// Calculate all entity columns (with optional filter) based on the cached adx schema
    /// </summary>
    /// <typeparam name="T">Type of Twin, Model or Relationship</typeparam>
    /// <param name="entity">Typed entity</param>
    /// <param name="customColumnDestination">Entity Type</param>
    /// <param name="deleted">Bool indicating whether columns are being calculated for deleted entity.</param>
    /// <param name="columnFilter">Predicate to filter the list of columns to calculate.</param>
    /// <param name="reEvaluate">True: Force re evaluate custom column values. False: Reuse existing values.</param>
    /// <returns>Dictionary of export column as key, and column value as value.</returns>
    public async Task<IDictionary<ExportColumn, string>> CalculateEntityColumns<T>(T entity,
        EntityType customColumnDestination,
        bool deleted = false,
        Func<ExportColumn, bool> columnFilter = null, bool reEvaluate = false)
    {
        var columns = await _adxSetupService.GetAdxTableSchema();
        if (columnFilter is not null)
        {
            columns = columns.Where(columnFilter);
        }
        if (columns == null || !columns.Any())
        {
            _logger.LogError("Missing Adx schema from cache. Verify if ADX initialized properly.");
            return null;
        }

        var columnValues = new ConcurrentDictionary<string, string>();
        var serializedEntity = JsonSerializer.Serialize(entity);
        var jsonSerialized = JsonDocument.Parse(serializedEntity).RootElement;
        var jObject = JObject.Parse(serializedEntity);
        var destColumns = columns.Where(x => x.Destination == customColumnDestination);

        var destColumnTasks = destColumns.Select(async column =>
        {
            columnValues.TryAdd(column.Name, await CalculateColumn(column, entity, jsonSerialized, jObject, deleted, reEvaluate));
        });
        await Task.WhenAll(destColumnTasks);

        return destColumns.ToDictionary(x => x, y => columnValues[y.Name]);
    }


    /// <summary>
    ///  Calculate individual entity column
    /// </summary>
    /// <typeparam name="T">Type Entity Twin, Model or Relationship</typeparam>
    /// <param name="column">ExportColumn</param>
    /// <param name="entity">Typed Entity</param>
    /// <param name="jsonSerialized">Entity Serialized Version</param>
    /// <param name="jObject">Entity JObject Instance</param>
    /// <param name="deleted">Bool indicating whether columns are being calculated for deleted entity.</param>
    /// <param name="reEvaluate">True: Force re evaluate custom column values. False: Reuse existing values.</param>
    /// <returns>Calculated Value as string</returns>
    public async Task<string> CalculateColumn<T>(ExportColumn column,
        T entity,
        JsonElement jsonSerialized,
        JObject jObject,
        bool deleted = false,
        bool reEvaluate = false)
    {
        try
        {
            if (column.IsDeleteColumn)
            {
                return deleted.ToString();
            }

            if (column.IsIngestionTimeColumn)
            {
                return DateTime.UtcNow.ToString("s");
            }

            if (column.IsCustomColumn)
            {
                var customValue = await Customization(column, entity, reEvaluate: reEvaluate);
                if (customValue != null) return customValue;
                // If null continue with the further evaluation
            }

            if (column.SourceType == CustomColumnSource.Query && !deleted)
            {
                var query = FormatQuery(column, jsonSerialized);
                return await GetQueryResult(column, query);
            }

            if (column.SourceType == CustomColumnSource.Path)
            {
                var token = column.Source != null ? jObject?.SelectToken(column.Source) : null;
                return token?.ToString();
            }

            if (column.SourceType == CustomColumnSource.Complex && column.Children.Any())
            {
                // need to switch by Models | Twins | Relationships
                var childValues = new Dictionary<string, string>();
                foreach (var child in column.Children)
                {
                    var calcValue = await CalculateColumn(child, entity, jsonSerialized, jObject, deleted, reEvaluate);
                    childValues.Add(child.Name, calcValue);
                }
                return JsonSerializer.Serialize(childValues);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting column {columnName}", column.Name);
        }
        return null;
    }

    private Task<string> Customization<T>(ExportColumn column, T data, bool reEvaluate = false)
    {
        return column.Destination switch
        {
            EntityType.Models => GetModelCustomValue(column, data as DigitalTwinModelExportData),
            EntityType.Relationships => GetRelationshipCustomValue(column, data as BasicRelationship),
            EntityType.Twins => GetTwinCustomValue(column, data as BasicDigitalTwin, reEvaluate),
            _ => null
        };
    }

    private async Task<string> GetTwinCustomValue(ExportColumn column, BasicDigitalTwin twin, bool reEvaluate = false)
    {
        // TODO: All these hard-coded prop names should be removed either because they
        //  will no longer be needed (TrendId, etc.) or replaced by a enum field 
        //  type such as ColType.RawTwin or ColType.ConcatenateArrayToCommaDelimtedList, etc.

        return column.Name.ToLower() switch
        {
            "tags" => (twin.Contents?.ContainsKey("tags") == true
                ? string.Join(",",
                    ((JsonElement)twin.Contents["tags"]).Deserialize<Dictionary<string, object>>()
                    ?.Select(x => x.Key).ToArray())
                : null),

            "uniqueid" => await GetUniqueIdForTwin(column.AdtPropName, twin, null),
            "trendid" => await GetTrendIdForTwin(column.AdtPropName, twin, null),

            "raw" => JsonSerializer.Serialize(RealEstateTwin.MapFrom(twin), jsonSerializerOptions),

            // If columns adt property name is configured in schema and
            // If re evaluation is not required by the caller and
            // If twin contents already has the custom column value
            // then return the value from twin content (with running a ADT query or Path query)
            // otherwise return null
            _ => (!(reEvaluate || string.IsNullOrEmpty(column.AdtPropName))
                && twin.Contents?.ContainsKey(column.AdtPropName) is true) ?
                    twin.Contents[column.AdtPropName].ToString() : null
        };
    }

    private static async Task<string> GetRelationshipCustomValue(ExportColumn column, BasicRelationship relationships)
    {
        var customMap = new Dictionary<string, Task<string>>
                {
                    { "raw", Task.FromResult(JsonSerializer.Serialize(RealEstateRelationship.MapFrom(relationships), new JsonSerializerOptions
                                                                                                    {
                                                                                                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                                                                        WriteIndented = true
                                                                                                    }))
                    }
                };

        return customMap.ContainsKey(column.Name.ToLower()) ? await customMap[column.Name.ToLower()] : null;
    }

    private async Task<string> GetQueryResult(ExportColumn customColumnRequest, string query)
    {
        var queryTask = _memoryCache.GetOrCreate(query, (entry) =>
        {
            entry.SetAbsoluteExpiration(TimeSpan.FromSeconds(10));
            var pageable2 = _azureDigitalTwinsReader.QueryAsync<Dictionary<string, object>>(query);
            return pageable2.AsPages().FirstAsync();
        });

        var page = await queryTask;

        var twinResponse = page?.Values.FirstOrDefault();

        if (twinResponse is not null && twinResponse.Any())
        {
            if (!string.IsNullOrWhiteSpace(customColumnRequest.Select))
            {
                return JObject.Parse(JsonSerializer.Serialize(twinResponse)).SelectToken(customColumnRequest.Select)?.ToString();
            }
            else
            {
                return twinResponse.Values.First().ToString();
            }
        }

        return null;
    }

    private static string SafeParameter(string parameter)
    {
        return parameter.Replace("'", "\\'");
    }


    private static string FormatQuery(ExportColumn customColumn, JsonElement entity)
    {
        return string.Format(customColumn.QueryFormat,
                              customColumn.QueryPaths.Select(p => entity.TryGetProperty(p.Item2, out JsonElement prop) ? SafeParameter(prop.GetString()) : string.Empty).ToArray());
    }

    /// <summary>
    /// Logic to set the UniqueId for twin creation or updates
    /// </summary>
    /// <returns>Task</returns>
    public async Task<string> GetUniqueIdForTwin(string adtPropName, BasicDigitalTwin twin, Func<Task<BasicDigitalTwin>> existingTwinFunc)
    {
        const string me = nameof(this.GetUniqueIdForTwin);
        string uniqueId = null;

        var twinProps = _azureDigitalTwinModelParser.GetInterfaceInfo(twin.Metadata.ModelId)?.Properties;
        if(twinProps==null || !twinProps.ContainsKey("uniqueID"))
        {
            return uniqueId;
        }

        // Check if UniqueIdMapping is initialized
        if (_uniqueIdMap is null)
        {
            //Get the UniqueIdMapping from Memory Cache
            if (_memoryCache.TryGetValue<ConcurrentDictionary<string, string>>(UniqueIdMappingCacheEntry, out var cacheResult))
            {
                _uniqueIdMap = cacheResult;
            }
            else
            {
                _uniqueIdMap = new();
            }
        }

        // Attempt to get uniqueID from the  requested twin
        if (twin.Contents.GetValueOrDefault(adtPropName) is not null)
        {
            uniqueId = twin.Contents.GetValueOrDefault(adtPropName)?.ToString();
            _logger.LogTrace("{fn}: Getting uniqueId '{uidOld}' from requested twin '{id}'", me, uniqueId, twin.Id);
        }
        else
        {
            _logger.LogWarning("{fn}: UniqueId property value does not exist in requested twin '{id}'", me, twin.Id);
        }

        // Attempt to get uniqueID from twin (if it is already existing)
        if (string.IsNullOrEmpty(uniqueId))
        {
            var existingTwin = await existingTwinFunc?.Invoke();
            uniqueId = existingTwin?.Contents?.GetValueOrDefault(adtPropName)?.ToString();
            if (uniqueId is not null)
            {
                _logger.LogTrace("{fn}: Getting uniqueId {uniqueId} from the existing twin with Id {twinId}", me, uniqueId, twin.Id);
            }
        }

        // Get UniqueId from the Cache if the value from existing twin is null
        if (string.IsNullOrEmpty(uniqueId) && _uniqueIdMap.GetValueOrDefault(twin.Id) is not null)
        {
            uniqueId = _uniqueIdMap.GetValueOrDefault(twin.Id);
            _logger.LogTrace("{fn}: Getting uniqueId {uniqueId} from the Cache for twin with Id {twinId}", me, uniqueId, twin.Id);

        }

        // UniqueId still null, Create a new UniqueId
        if (string.IsNullOrEmpty(uniqueId))
        {
            uniqueId = Guid.NewGuid().ToString();
            _logger.LogTrace("{fn}: Creating new UniqueID '{uid}' for twin '{id}'", me, uniqueId, twin.Id);

        }

        // update the mapping
        _uniqueIdMap[twin.Id] = uniqueId;
        // update it back in to Memory cache and set it to absolute expire 24 hours from now
        _memoryCache.Set(UniqueIdMappingCacheEntry, _uniqueIdMap, new MemoryCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) });

        return uniqueId;

    }

    /// <summary>
    /// Logic to set the TrendId for twin creation or updates
    /// </summary>
    /// <returns>Null if not capability twin</returns>
    public async Task<string> GetTrendIdForTwin(string adtPropName, BasicDigitalTwin twin, Func<Task<BasicDigitalTwin>> existingTwinFunc)
    {
        const string me = nameof(this.GetTrendIdForTwin);

        if (!_azureDigitalTwinModelParser.IsDescendantOf(ModelDefinitions.Capability, twin.Metadata.ModelId))
        {
            return null;
        }
        // Twin is a Capability, so reuse any existing TrendId or create a new one
        var requestedTrendId = twin.Contents.GetValueOrDefault(adtPropName)?.ToString();

        string readTwinTrendId = null;
        if (requestedTrendId is null)
        {
            var existingTwin = await existingTwinFunc?.Invoke();
            readTwinTrendId = existingTwin?.Contents?.GetValueOrDefault(adtPropName)?.ToString();
        }

        var overrideTrendId = requestedTrendId ?? readTwinTrendId ?? Guid.NewGuid().ToString();
        if (overrideTrendId != requestedTrendId)
        {
            _logger.LogWarning("{fn}: Overriding existing trendId  '{tidOld}' with '{tidNew} for twin '{id}'",
                me, requestedTrendId, overrideTrendId, twin.Id);
            return overrideTrendId;
        }
        return requestedTrendId;
    }

    private async Task<string> GetModelCustomValue(ExportColumn column, DigitalTwinModelExportData model)
    {
        return column.Name switch
        {
            "allExtends" => GetAllExtendsForModel(model),

            _ => null

        };
    }

    private string GetAllExtendsForModel(DigitalTwinModelExportData model)
    {
        var allExtends = _azureDigitalTwinModelParser.GetAllAncestors(model.Id);

        return string.Join(", ", allExtends);
    }
}
