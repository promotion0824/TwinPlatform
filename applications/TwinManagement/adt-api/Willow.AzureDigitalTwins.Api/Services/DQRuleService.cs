using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.Api.Common.Extensions;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Services.Cache.Models;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.DataQuality.Execution.Checkers;
using Willow.DataQuality.Model.Rules;
using Willow.DataQuality.Model.Serialization;
using Willow.DataQuality.Model.Validation;
using Willow.Extensions.Logging;
using Willow.Model.Adt;
using Willow.Storage.Blobs;

namespace Willow.AzureDigitalTwins.DataQuality.Api.Services;

using DQRuleCacheType = ConcurrentDictionary<string, RuleTemplate>;

public interface IDQRuleService
{
    Task<Stream> DownloadRuleFileAsync(string fileName);
    Task<bool> DeleteRuleFileAsync(string ruleId);
    Task DeleteAllRulesFileAsync();
    Task<string> UploadRuleFileAsync(string fileName, Stream content);
    Task InitializeValidationRules();
    Task<IEnumerable<RuleTemplateValidationResult>> GetValidationResults(IEnumerable<TwinWithRelationships> twins, IDictionary<string, List<UnitInfo>> unitInfo = null);
    Task<IEnumerable<RuleTemplate>> GetValidationRules();
    Task<List<string>> GetRuleModels(IEnumerable<string> filterModels, bool exactModelOnly);
}

public class DQRuleService : IDQRuleService
{
    const int BaseDegreeOfParallelism = 15;

    private readonly IBlobService _blobService;
    private readonly string _container;
    private readonly IMemoryCache _rulesCache;
    private const string CacheRulesKey = "RulesCache";
    private readonly IRuleTemplateChecker _ruleTemplateChecker;
    private readonly ILogger<DQRuleService> _logger;
    private readonly IRuleTemplateSerializer _ruleTemplateSerializer;
    private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;
    private readonly IAdxService _adxService;

    protected int _degreeOfParallelism = BaseDegreeOfParallelism;

    private ConcurrentDictionary<string, RuleTemplate> RulesDict =>
        _rulesCache.Get<DQRuleCacheType>(CacheRulesKey)
            ?? throw new InvalidOperationException($"Static rule cache not initialized");
    private IEnumerable<RuleTemplate> Rules => RulesDict.Values;
    private RuleTemplate AddRule(RuleTemplate rule) => RulesDict[rule.Id] = rule;
    private void DeleteRule(string name) => RulesDict.TryRemove(name, out _);

    public DQRuleService(IBlobService blobService,
        IConfiguration configuration,
        IMemoryCache memoryCache,
        IRuleTemplateChecker ruleTemplateChecker,
        IRuleTemplateSerializer ruleTemplateSerializer,
        ILogger<DQRuleService> logger,
        AzureDigitalTwinsSettings azureDigitalTwinsSettings,
        IAdxService adxService,
        IAzureDigitalTwinModelParser azureDigitalTwinModelParser)
    {
        _blobService = blobService;
        _container = configuration.GetValue<string>("BlobStorage:DQRuleContainer");
        _rulesCache = memoryCache;
        _ruleTemplateChecker = ruleTemplateChecker;
        _ruleTemplateSerializer = ruleTemplateSerializer;
        _logger = logger;
        _azureDigitalTwinModelParser = azureDigitalTwinModelParser;
        _adxService = adxService;

        _degreeOfParallelism = Math.Min(10, Math.Max(1,
            (azureDigitalTwinsSettings.PercentDegreeOfParallelism ?? 100) * BaseDegreeOfParallelism / 100));

    }

    public Task<Stream> DownloadRuleFileAsync(string fileName)
    {
        return _blobService.DownloadFile(_container, fileName);
    }

    public async Task<bool> DeleteRuleFileAsync(string ruleId)
    {
        return await _blobService.DeleteBlob(_container, ruleId);
    }

    public async Task DeleteAllRulesFileAsync()
    {
        var rulesFiles = await _blobService.GetBlobItems(_container);
        foreach (var rulesFile in rulesFiles)
        {
            var isDeleted = await _blobService.DeleteBlob(_container, rulesFile.Name);
            if (isDeleted)
            {
                // update rule cache
                DeleteRule(rulesFile.Name);

            }
        }

    }

    public async Task<string> UploadRuleFileAsync(string fileName, Stream content)
    {
        try
        {
            _logger.LogInformation("Uploading rule from {fileName}", fileName);

            var rule = ReadRuleFromStream(content);
            string ruleString = _ruleTemplateSerializer.Serialize(rule);
            using var stream = new MemoryStream().FromString(ruleString);
            await _blobService.UploadFile(_container, rule.Id, stream, overwrite: true);
            _logger.LogInformation("Uploaded rule {ruleId} from {fileName}", rule.Id, fileName);

            // It may be the case that IDs only need to be unique in each templateId category -
            //  but at the moment we only have one templateId: "basic-governance-by-modelId"
            AddRule(rule); // update rule cache once we've added to storage
        }
        catch (Exception ex)
        {
            _logger.LogError("Uploaded rule file error. File name: {fileName}, Error: {exStackTrace}", fileName, ex.StackTrace);
            return ex.Message;
        }

        return null;
    }

    // This is called only at service startup to pre-load rules - new rules write-thru the cache to BLOB storage
    public async Task InitializeValidationRules()
    {
        _logger.LogInformation("InitalizeValidationRules from container {container}", _container);

        if (_rulesCache.TryGetValue(CacheRulesKey, out _))
            _rulesCache.Remove(CacheRulesKey);

        List<Azure.Storage.Blobs.Models.BlobItem> blobItems = null;

        await _rulesCache.GetOrCreateAsync<DQRuleCacheType>(CacheRulesKey,
            async (entry) =>
            {
                entry.SetPriority(CacheItemPriority.NeverRemove);
                var rules = new ConcurrentDictionary<string, RuleTemplate>();

                try
                {
                    blobItems = (await _blobService.GetBlobItems(_container)).ToList();

                    foreach (var blobItem in blobItems)
                    {
                        RuleTemplate rule = null;
                        try
                        {
                            using var fileStream = await _blobService.DownloadFile(_container, blobItem.Name);
                            rule = _ruleTemplateSerializer.Deserialize(fileStream.ConvertString());
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Error loading rule from: {file}", blobItem.Name);
                            // Keep attempting to load further rules (or we can abort here)
                        }

                        if (rule != null)
                        {
                            rules[rule.Id] = rule;
                            _logger.LogInformation("Loaded rule: {ruleId}", rule.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error initializing data-quality rules");
                }
                return rules;
            });

        _logger.LogInformation("Loaded {rulesCount} of {totalCount} rules",
            _rulesCache.Get<DQRuleCacheType>(CacheRulesKey)?.Count ?? -1,
            blobItems?.Count ?? -1);
    }

    /// <summary>
    /// Get all validation rules. This is currently a synchronous operation that returns
    ///   all the rules, which were pre-loaded at startup.
    /// </summary>
    public Task<IEnumerable<RuleTemplate>> GetValidationRules()
    {
        return Task.FromResult(Rules ?? Enumerable.Empty<RuleTemplate>());
    }

    /// <summary>
    /// Get all models (incl. child models when specified) mentioned by all rules
    ///  intersected with the user's query filter, intersected with all models that have twins.
    /// </summary>
    public async Task<List<string>> GetRuleModels(IEnumerable<string> filterModels, bool exactModelOnly)
    {
        // Generate list of all models (incl. child models, if specified) that are potentially utilized by all DQ rules
        var ruleModels = new List<string>();
        foreach (var rule in Rules)
        {
            if (rule.ExactModelOnly)
                ruleModels.Add(rule.PrimaryModelId);
            else
            {
                var descendants = _azureDigitalTwinModelParser.GetInterfaceDescendants(new[] { rule.PrimaryModelId });
                ruleModels.AddRange(descendants.Keys);
            }
        }

        // Generate list of all models (incl. child models, if specified) that are specified by the user's query filter
        var filteredModels = new List<string>();
        if (filterModels is not null) foreach (var model in filterModels)
            {
                if (exactModelOnly)
                    filteredModels.Add(model);
                else
                {
                    var descendants = _azureDigitalTwinModelParser.GetInterfaceDescendants(new[] { model });
                    filteredModels.AddRange(descendants.Keys);
                }
            }

        // All models in common between rules and user filter
        var allFilteredRuleModels = (filteredModels.Any() ?
                            ruleModels.Intersect(filteredModels) : ruleModels).ToList();

        // Fetch models with twin counts (models with 0 twins already excluded)
        var modelsWithTwinCounts = (await _adxService.GetTwinCountByModelAsync()).Keys.ToList();

        // Final result = AllRuleModels ∩ AllFilteredModels ∩ AllModelsWithTwins
        var allApplicableModels = allFilteredRuleModels.Intersect(modelsWithTwinCounts).ToList();

        // TODO: output this info to the async job, as well as summary results 
        _logger.LogInformation("GetRuleModels:  " +
                            "ruleRefModelCount: {allRuleModelsCount} " +
                            "userQueryModelCount: {allFilteredModelsCount} " +
                            "hasTwinsModelCount: {modelsWithTwinCountsCount} " +
                            "finalIntersectionModelCount: {allApplicableModelsCount} ",
                                ruleModels.Count, filteredModels.Count,
                                modelsWithTwinCounts.Count, allApplicableModels.Count);

        return allApplicableModels.ToList();
    }


    public async Task<IEnumerable<RuleTemplateValidationResult>> GetValidationResults(IEnumerable<TwinWithRelationships> twins, IDictionary<string, List<UnitInfo>> unitInfo)
    {
        var results = new ConcurrentBag<RuleTemplateValidationResult>();
        var rules = Rules;

        await MeasureExecutionTime.ExecuteTimed(async () =>
        {
            await Parallel.ForEachAsync(twins,
            new ParallelOptions { MaxDegreeOfParallelism = _degreeOfParallelism },
            async (twinWithRelationship, t) =>
            {
                foreach (var rule in rules)
                {
                    var unitInfos = unitInfo?.GetValueOrDefault(twinWithRelationship.Twin.Metadata.ModelId);
                    var result = await _ruleTemplateChecker.Check(twinWithRelationship, rule, unitInfos);

                    if (result.IsApplicableModel)
                        results.Add(result);
                }
            });
            return Task.FromResult(true);
        },
        (res, ms) =>
        {
           _logger.LogInformation($"Validated {twins.Count()} twins from {rules.Count()} rules in {ms} ms");
        });
        _logger.LogInformation($"{typeof(IDQRuleService).Name} - DegreeOfParallelism: {{par}}", _degreeOfParallelism);

        return results;
    }

    // When the user uploads a rule file, it doesn't have a discriminator to differentiate the various type of rule checks,
    //    so we need to load into a DOM first and examine the properties (and later the ontology) to infer the rule types.
    //    Then we create our strongly-typed rule sub-classes, at which point the polymorphic serialization code will
    //    serialize the file with the discriminator so that the proper sub-classes can be deserialized when loaded again later.
    private RuleTemplate ReadRuleFromStream(Stream stream)
    {
        using var streamReader = new StreamReader(stream);
        string text = streamReader.ReadToEnd();

        // Derived RuleTemplateProperty Derived Type manually
        var ruleTemplateProperties = new List<RuleTemplateProperty>();
        using var doc = JsonDocument.Parse(text);
        bool hasProperties = doc.RootElement.TryGetProperty("properties", out var propertiesElement);
        if (hasProperties)
        {
            // Enumerate all properties of the rule input text
            foreach (JsonElement property in propertiesElement.EnumerateArray())
            {
                var prop = GetRuleTemplatePropertySubType(property);
                ruleTemplateProperties.Add(prop);
            }
        }

        // Deserialize rule template
        var rule = _ruleTemplateSerializer.Deserialize(text);
        // Replacing base types with sub types for properties
        rule.Properties = ruleTemplateProperties;
        return rule;
    }

    private static RuleTemplateProperty GetRuleTemplatePropertySubType(JsonElement property)
    {
        var propertyDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(property.ToString());
        var baseType = new RuleTemplateProperty
        {
            Name = propertyDictionary["name"].ToString(),
            Required = Boolean.Parse(propertyDictionary["required"].ToString())
        };

        var subTypes = new List<RuleTemplateProperty>();
        if (propertyDictionary.TryGetValue("pattern", out object patternType))
        {
            var patternProperty = new RuleTemplatePropertyPattern
            {
                Name = baseType.Name,
                Required = baseType.Required,
                Pattern = patternType.ToString()
            };
            subTypes.Add(patternProperty);
        }

        if (propertyDictionary.ContainsKey("allowedValues"))
        {
            var allowedValuesElement = property.GetProperty("allowedValues");
            var allowedValueProperty = GetAllowedValuesType(propertyDictionary, baseType, allowedValuesElement);
            subTypes.Add(allowedValueProperty);
        }

        bool hasMinValue = propertyDictionary.TryGetValue("minValue", out object minValue);
        bool hasMaxValue = propertyDictionary.TryGetValue("maxValue", out object maxValue);
        bool hasUnit = propertyDictionary.TryGetValue("unit", out object unit);
        if (hasMinValue || hasMaxValue)
        {
            subTypes.Add(GetRangeType(minValue, maxValue, unit, baseType));
        }

        return subTypes.Count switch
        {
            0 => baseType,
            1 => subTypes.First(),
            _ => throw new FormatException($"A property currently cannot have than one type of check - found: {string.Join(", ", subTypes)}"),
        };
    }

    private static RuleTemplateProperty GetAllowedValuesType(Dictionary<string, object> propertyDictionary, RuleTemplateProperty baseType, JsonElement allowedValuesElement)
    {
        var allowedValuesNumeric = new List<double>();
        var allowedValuesString = new List<string>();
        foreach (var item in allowedValuesElement.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Number && double.TryParse(item.ToString(), out double value))
            {
                allowedValuesNumeric.Add(value);
            }
            else
            {
                allowedValuesString.Add(item.ToString());
            }
        }

        if (allowedValuesNumeric.Any() && allowedValuesString.Any())
        {
            throw new FormatException($"An allowedValues property cannot have mixed types - found: {string.Join(", ", allowedValuesNumeric)}, {string.Join(", ", allowedValuesString)}");
        }

        if (allowedValuesNumeric.Any())
        {
            return new RuleTemplatePropertyNumericAllowedValues
            {
                Name = baseType.Name,
                Required = baseType.Required,
                AllowedValues = allowedValuesNumeric,
                Unit = propertyDictionary.GetValueOrDefault("unit")?.ToString()
            };
        }

        if (allowedValuesString.Any())
        {
            return new RuleTemplatePropertyStringAllowedValues
            {
                Name = baseType.Name,
                Required = baseType.Required,
                AllowedValues = allowedValuesString
            };
        }

        return baseType;
    }

    private static bool TryGetPropertyDateRange(RuleTemplateProperty baseType, object minValue, object maxValue, out RuleTemplatePropertyDateRange property)
    {
        property = new RuleTemplatePropertyDateRange
        {
            Name = baseType.Name,
            Required = baseType.Required
        };

        if (minValue != null && DateTime.TryParse(minValue.ToString(), out DateTime minValueDate))
        {
            property.MinValue = minValueDate;
        }

        if (maxValue != null && DateTime.TryParse(maxValue.ToString(), out DateTime maxValueDate))
        {
            property.MaxValue = maxValueDate;
        }

        // Not a date range type, there is no min/max value of type date range
        if (!property.MinValue.HasValue && !property.MaxValue.HasValue)
        {
            property = null;
            return false;
        }

        if (minValue != null && maxValue != null && (!property.MinValue.HasValue || !property.MaxValue.HasValue))
        {
            throw new FormatException($"minValue/maxValue type mismatched. Property name: {baseType.Name}, minValue: {minValue}, maxValue: {maxValue}");
        }

        return true;
    }

    private static bool TryGetPropertyNumericRange(RuleTemplateProperty baseType, object minValue, object maxValue, object unit, out RuleTemplatePropertyNumericRange property)
    {
        property = new RuleTemplatePropertyNumericRange
        {
            Name = baseType.Name,
            Required = baseType.Required,
            MinValue = null,
            MaxValue = null,
            Unit = null
        };

        if (minValue != null && double.TryParse(minValue.ToString(), out double minValueDouble))
        {
            property.MinValue = minValueDouble;
        }

        if (maxValue != null && double.TryParse(maxValue.ToString(), out double maxValueDouble))
        {
            property.MaxValue = maxValueDouble;
        }

        // Not a numeric range type, there is no min/max value of type numeric
        if (!property.MinValue.HasValue && !property.MaxValue.HasValue)
        {
            property = null;
            return false;
        }

        property.Unit = unit?.ToString();

        if (minValue != null && maxValue != null && (!property.MinValue.HasValue || !property.MaxValue.HasValue))
        {
            throw new FormatException($"minValue/maxValue type mismatched. Property name: {baseType.Name}, minValue: {minValue}, maxValue: {maxValue}");
        }

        return true;
    }

    private static RuleTemplateProperty GetRangeType(object minValue, object maxValue, object unit, RuleTemplateProperty baseType)
    {
        bool hasNumericRange = TryGetPropertyNumericRange(baseType, minValue, maxValue, unit, out RuleTemplatePropertyNumericRange numericRangeProp);
        bool hasDateRange = TryGetPropertyDateRange(baseType, minValue, maxValue, out RuleTemplatePropertyDateRange dateRangeProp);

        return (hasNumericRange, hasDateRange) switch
        {
            (true, true) => throw new FormatException($"A property currently cannot have than one type of check. Property name: {baseType.Name}, minValue: {minValue}, maxValue: {maxValue}"),
            (true, false) => numericRangeProp,
            (false, true) => dateRangeProp,
            _ => throw new FormatException($"minValue/maxValue: Only numeric and date ranges supported. Property name: {baseType.Name}, minValue: {minValue}, maxValue: {maxValue}")
        };
    }
}
