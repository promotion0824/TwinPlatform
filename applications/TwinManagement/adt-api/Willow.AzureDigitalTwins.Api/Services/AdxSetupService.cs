using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDataExplorer.Helpers;
using Willow.AzureDataExplorer.Infra;
using Willow.AzureDataExplorer.Model;
using Willow.AzureDataExplorer.Options;
using Willow.Model.Adt;
using Willow.Model.Adx;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Services;

/// <summary>
/// /// ADX Database setup method abstraction
/// </summary>
public interface IAdxSetupService
{
    /// <summary>
    /// Method to check if primary adx tables [Twins,Models,Relationships] are created
    /// </summary>
    /// <returns>True if initialized; false otherwise</returns>
    public Task<bool> IsAdxInitializedAsync();

    /// <summary>
    /// Method to order export columns in the order they appear in the ADX tables and cache it
    /// </summary>
    /// <param name="schemaName">Configured Schema name</param>
    /// <returns>List of ordered export columns</returns>
    public Task<IEnumerable<ExportColumn>> CacheCurrentTableSchema(string schemaName);

    /// <summary>
    /// Lazy initialize ADX settings
    /// </summary>
    /// <returns>Handler to the task.</returns>
    public Task InitializeAdxLazy();

    /// <summary>
    /// Get the cached ADX table column definition
    /// </summary>
    /// <returns>List of column definition</returns>
    public Task<IEnumerable<ExportColumn>> GetAdxTableSchema();
}

/// <summary>
/// ADX Database setup class
/// </summary>
public class AdxSetupService : IAdxSetupService
{
    public const string SchemaFolderName = "Schema";

    private readonly string _adxSchemaKey;

    private readonly IAzureDataExplorerInfra _azureDataExplorerInfra;
    private readonly AzureDataExplorerOptions _azureDataExplorerOptions;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<AdxSetupService> _logger;

    private static readonly SemaphoreSlim adxInitSemaphore = new(initialCount: 1, maxCount: 1);
    private static Lazy<Task> adxInitializeTask { get; set; }
    private readonly JsonSerializerOptions schemaSerializerOptions;

    public AdxSetupService(IAzureDataExplorerInfra azureDataExplorerInfra,
        IMemoryCache memoryCache,
        IOptions<AzureDataExplorerOptions> azureDataExplorerOptions,
        ILogger<AdxSetupService> logger)
    {
        _azureDataExplorerInfra = azureDataExplorerInfra;
        _memoryCache = memoryCache;
        _azureDataExplorerOptions = azureDataExplorerOptions.Value;
        _logger = logger;

        _adxSchemaKey = $"{_azureDataExplorerOptions.DatabaseName}.schema";

        schemaSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true,
        };
        schemaSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    /// <summary>
    /// Method to check if primary adx tables [Twins,Models,Relationships] are created
    /// </summary>
    /// <returns>True if initialized; false otherwise</returns>
    public async Task<bool> IsAdxInitializedAsync()
    {
        await InitializeAdxLazy();
        return _memoryCache.Get<IEnumerable<ExportColumn>>(_adxSchemaKey) is not null;
    }


    /// <summary>
    /// Get the cached ADX table column definition
    /// </summary>
    /// <returns>List of column definition</returns>
    public async Task<IEnumerable<ExportColumn>> GetAdxTableSchema()
    {
        //Check and return if the schema exists
        if (_memoryCache.TryGetValue(_adxSchemaKey, out IEnumerable<ExportColumn> tableColumnSchema))
            return tableColumnSchema;

        // Retry ADX initialization if not successful at least once
        _logger.LogWarning("ADX Schema not found in the memory cache. Retrying ADX initialization.");
        await InitializeAdxLazy();
        if (_memoryCache.TryGetValue(_adxSchemaKey, out tableColumnSchema))
            return tableColumnSchema;

        // TODO: Below caching logic is redundant. Monitor the logs and remove the code if we never hit this code path

        // ADX Schema get cached at the app startup and stays in the memory cache for lifetime of the app
        // ADX Schema cache entry has no expiry and set to never remove under memory pressure.
        // However we noticed at times, the cache entry goes missing and the ADX operations fails
        // As a fallback we recache the schema back in memory cache whenever the entry goes missing.
        _logger.LogWarning("ADX Schema not found in the memory cache. Recaching the schema from ADX.");
        await CacheCurrentTableSchema(_azureDataExplorerOptions.Schema.DefaultSchemaName);
        if (_memoryCache.TryGetValue(_adxSchemaKey, out tableColumnSchema))
            return tableColumnSchema;
        throw new InvalidOperationException("ADX schema not found in the cache. Verify ADX initialized properly.");
    }

    /// <summary>
    /// Get and parse schema from configured schema json files
    /// </summary>
    /// <param name="filterBySchemaName">Filter the results with the name of the schema</param>
    /// <returns>List of Schema object</returns>
    /// <exception cref="InvalidOperationException">If no schema files were found.</exception>
    private IEnumerable<Schema> GetSchemaFromConfig(string filterBySchemaName = null)
    {
        var allSchemaFiles = Directory.EnumerateFiles(SchemaFolderName, "*.json");

        if (!allSchemaFiles.Any())
        {
            _logger.LogCritical($"No files [schema].json were found inside schema folder: {SchemaFolderName}");
            throw new InvalidOperationException();
        }

        var schemas = allSchemaFiles.Select(x =>
        {
            using var fileStream = File.OpenRead(x);

            return
                JsonSerializer.Deserialize<Schema>(fileStream, schemaSerializerOptions);
        });

        if (filterBySchemaName is null)
            return schemas;

        var defaultSchema = schemas.Where(w => string.Equals(w.Name, filterBySchemaName, StringComparison.InvariantCultureIgnoreCase));

        return Enumerable.DefaultIfEmpty(defaultSchema);
    }

    /// <summary>
    /// Initialize ADX database with tables, materialized view and function based on the configured default schema
    /// </summary>
    /// <returns>Handle to the task</returns>
    private async Task CreateAdxInfrastructureAndMigrateAsync(bool enableSchemaMigration)
    {
        var databaseSchema = await GetDatabaseTableSchema();
        var mvViews = await _azureDataExplorerInfra.GetMaterializedViews(_azureDataExplorerOptions.DatabaseName);

        var configuredSchema = GetSchemaFromConfig(_azureDataExplorerOptions.Schema.DefaultSchemaName).FirstOrDefault();
        if (configuredSchema is null)
        {
            _logger.LogCritical("Unable to locate the default schema {name}. Adx initialization failed.", _azureDataExplorerOptions.Schema.DefaultSchemaName);
            return;
        }

        var allTables = configuredSchema.TableDefinitions.GroupBy(g => g.Destination).ToDictionary(x => x.Key, y => y.ToList());

        foreach (var table in allTables)
        {
            bool tableCreatedOrUpdated = false;

            //-------------TABLE-------------//
            // Check if the table exist  
            if (databaseSchema.TryGetValue(table.Key, out IEnumerable<ColumnSchema> currentTableColumns))
            {
                List<ExportColumn> latestTableDefinition = table.Value;

                // check if migration is enabled
                if (enableSchemaMigration)
                {
                    // Get list of columns that are not present in the ADX table
                    var columnsToAdd = latestTableDefinition.Where(w => !currentTableColumns.Any(a => a.Name == w.Name)).ToList();

                    if (columnsToAdd.Count > 0)
                    {
                        // Perform migration since there's missing columns in the current ADX schema
                        try
                        {
                            await MigrateTable(table.Key.ToString(), columnsToAdd);
                            tableCreatedOrUpdated = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error occurred while migrating ADX table {TableName} to new schema.", table.Key);
                        }
                    }
                }
            }
            else
            {
                // table does not exist, create table with the latest schema
                _logger.LogInformation("Creating ADX Table {table}.", table.Key.ToString());
                await _azureDataExplorerInfra.CreateTableAsync(_azureDataExplorerOptions.DatabaseName, table.Key.ToString(), table.Value.Select(z => (z.Name, ((ColumnType)(int)z.Type).GetDescription()).ToTuple()));
                tableCreatedOrUpdated = true;
            }

            //-------------MATERIALIZED VIEW-------------//
            // Check if materialized view configuration exist
            var mvConfiguration = configuredSchema.MaterializedViews.Where(w => string.Equals(w.Table, table.Key.ToString(), StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (mvConfiguration == null) continue;

            // Create or re-create mv every time the table is updated or created or if mv does not exist
            if(tableCreatedOrUpdated || !mvViews.Any(a => a == mvConfiguration.Name))
            {
                // Look for PreScript to run
                if (mvConfiguration.EnforceInfiniteCachingOnTable)
                {
                    await _azureDataExplorerInfra.AlterTableHotCaching(_azureDataExplorerOptions.DatabaseName, mvConfiguration.Table, 9999999);
                }

                _logger.LogDebug("Creating Materialized View with name {Name} for table {table}.",mvConfiguration.Name, mvConfiguration.Table);

                // Note that with backfill=true, the service will encounter errors when accessing ADX until the backfill is complete
                // so we read the backfill from the app settings
                // If the backfill is true, you don't need to manually trigger a full export
                // if backfill is false, you need to manually trigger a full export from ADT to ADX
                await _azureDataExplorerInfra.CreateMaterializedView(
                            _azureDataExplorerOptions.DatabaseName,
                            mvConfiguration.Name, mvConfiguration.Table, mvConfiguration.Body,
                            (mvConfiguration.Backfill && _azureDataExplorerOptions.Schema.AllowBackfill), dropAndRecreate: true);
            }
        }

        //-------------FUNCTIONS-------------//
        // Create or Alter all configured Functions
        var createFunctionTask = configuredSchema.Functions.Select(s =>
        {
            _logger.LogDebug("Creating Function with name {Name} inside folder {folder}.", s.Name, s.Folder);
            return _azureDataExplorerInfra.CreateOrAlterFunction(_azureDataExplorerOptions.DatabaseName, s.Name, s.Body, s.Folder);
        });

        await Task.WhenAll(createFunctionTask);

        _logger.LogInformation("Done creating ADX default tables | materialized-views | functions.");
    }

    /// <summary>
    /// Method to order export columns in the order they appear in the ADX tables and cache it
    /// </summary>
    /// <param name="schemaName">Configured Schema name</param>
    /// <returns>List of ordered export columns</returns>
    public async Task<IEnumerable<ExportColumn>> CacheCurrentTableSchema(string schemaName)
    {
        try
        {
            // Get all the schema from Config
            var configSchema = GetSchemaFromConfig(schemaName).Single();

            var adxDatabaseSchema = await GetDatabaseTableSchema();

            var columns = new List<ExportColumn>();
            foreach (var adxSchemaType in adxDatabaseSchema)
            {
                var typeColumns = configSchema.TableDefinitions.Where(x => x.Destination == adxSchemaType.Key);
                columns.AddRange(adxSchemaType.Value.Select(x =>
                {
                    var typeColumn = typeColumns.FirstOrDefault(c => c.Name == x.Name);
                    if (typeColumn is not null)
                        return typeColumn;

                    _logger.LogTrace($"Missing Adx column in stored columns for {adxSchemaType.Key}, column {x.Name} for schema {configSchema.Name}");

                    return new ExportColumn
                    {
                        Name = x.Name,
                        Destination = adxSchemaType.Key,
                        Type = (CustomColumnType)(int)x.Type
                    };
                }));
            }

            // Cache the table definition
            _memoryCache.Set(_adxSchemaKey, columns, new MemoryCacheEntryOptions()
            {
                Priority = CacheItemPriority.NeverRemove,
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1000)

            });

            return columns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache the ADX Schema.");
        }
        return null;
    }

    private async Task MigrateTable(string tableName, List<ExportColumn> newColumns)
    {
        // Add new columns to Model table by alter-merge
        foreach (var column in newColumns)
        {
            // Convert to equivalent .Net type
            var dotNetType = ((ColumnType)(int)column.Type).GetDescription();

            // todo: move this mapping to Willow.AzureDataExplorer library
            var dotNetToCslTypeMap = new Dictionary<string, string>()
            {
            { "System.Boolean", "bool" },
            { "System.DateTime", "datetime" },
            { "System.Object", "dynamic" },
            { "System.Guid", "guid" },
            { "System.Int32", "int" },
            { "System.Int64", "long" },
            { "System.Double", "real" },
            { "System.String", "string" },
            { "System.TimeSpan", "timespan" },
            };

            _logger.LogInformation($"Altering {tableName} table, adding column {column.Name}");
            await _azureDataExplorerInfra.AlterMergeTable(_azureDataExplorerOptions.DatabaseName, tableName, column.Name, dotNetToCslTypeMap[dotNetType]);
        }
    }

    private async Task<IDictionary<EntityType, IEnumerable<ColumnSchema>>> GetDatabaseTableSchema()
    {
        var databaseSchema = await _azureDataExplorerInfra.GetDatabaseSchemaAsync(_azureDataExplorerOptions.DatabaseName);
        // Organize DB Schema by entity type in to a dictionary
        var adxSchemaByType = databaseSchema.Tables
            .Where(x => Enum.TryParse<EntityType>(x.Key, out EntityType _))
            .ToDictionary(x => Enum.Parse<EntityType>(x.Key), x => x.Value.OrderedColumns);

        return adxSchemaByType;
    }

    /// <summary>
    /// Method to initialize ADX settings
    /// </summary>
    /// <returns>Handler to the task.</returns>
    private async Task InitAdxSettings()
    {
        try
        {
            // Create missing Tables, Materialized Views & Functions
            await CreateAdxInfrastructureAndMigrateAsync(_azureDataExplorerOptions.Schema.EnableMigration);

            // Find the closest schema and cache the column definition
            await CacheCurrentTableSchema(_azureDataExplorerOptions.Schema.DefaultSchemaName);

            _logger.LogInformation("ADX initialization complete.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while initializing ADX settings. ADX initialization failed.");
            adxInitializeTask = new Lazy<Task>(InitAdxSettings);
            _logger.LogInformation("Re-Instantiating ADX initialization task.");

            throw;
        }
    }

    /// <summary>
    /// Lazy initialize ADX settings
    /// </summary>
    /// <returns>Handler to the task.</returns>
    public async Task InitializeAdxLazy()
    {
        try
        {
            // Only one thread can enter the code, other threads will wait until the previous one is released in the final block
            await adxInitSemaphore.WaitAsync();

            adxInitializeTask ??= new Lazy<Task>(InitAdxSettings);

            await adxInitializeTask.Value;
        }
        finally
        {
            adxInitSemaphore.Release();
        }

    }
}
