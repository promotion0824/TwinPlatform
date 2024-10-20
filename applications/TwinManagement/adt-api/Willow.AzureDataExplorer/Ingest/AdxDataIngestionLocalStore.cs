using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using Willow.AzureDataExplorer.Helpers;
using Willow.AzureDataExplorer.Model;
using Willow.Extensions.Logging;

namespace Willow.AzureDataExplorer.Ingest;

public interface IAdxDataIngestionLocalStore
{
    /// <summary>
    /// Adds a record to the store for ingestion.
    /// </summary>
    /// <param name="database">Name of the ADX database.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="columnValues">Dictionary of datacolumn and values.</param>
    /// <param name="forceFlush">True to flush the data immediately; false when the threshold condition is met.</param>
    /// <returns>Number of rows inserted</returns>
    public Task<long> AddRecordForIngestion(string database, string tableName, IDictionary<AdxColumn, object> columnValues, bool forceFlush = false);

    /// <summary>
    /// Send all the data to ADX for ingestion.
    /// </summary>
    /// <returns>Awaitable task.</returns>
    public Task<long> FlushLocalStore();
}

/// <summary>
/// Adx Column record to hold name and data type
/// </summary>
/// <param name="Name">Name of the column.</param>
/// <param name="ColumnType">Type of the column.</param>
public record AdxColumn(string Name, ColumnType ColumnType)
{
    /// <summary>
    /// ToString() override to return name.
    /// </summary>
    /// <returns>Name of the column.</returns>
    public override string ToString() => Name;
}

/// <summary>
/// Record to hold database name, table name and an Adx row record.
/// </summary>
/// <param name="DatabaseName">Name of the database.</param>
/// <param name="TableName">Name of the table.</param>
/// <param name="ColumnValues">Dictionary of column names and values.</param>
public record AdxRow(string DatabaseName, string TableName, IDictionary<AdxColumn, object> ColumnValues)
{
    /// <summary>
    /// Returns database and table name in the format [dataabse.table].
    /// </summary>
    /// <returns>string.</returns>
    public string GetFullyQualifiedTableName() => $"{DatabaseName}.{TableName}";
}

/// <summary>
/// Record to hold database name, table name and the datatable itself.
/// </summary>
/// <param name="DatabaseName">Name of the database.</param>
/// <param name="TableName">Name of the table.</param>
/// <param name="DataTable">Instance of the datatable.</param>
public record DataTableEntry(string DatabaseName, string TableName, DataTable DataTable)
{
    /// <summary>
    /// Returns database and table name in the format [dataabse.table].
    /// </summary>
    /// <returns>string.</returns>
    public string GetFullyQualifiedTableName() => $"{DatabaseName}.{TableName}";
}

/// <summary>
/// ADX Data Ingestion Local store class implementation. Must be registered as singleton instance.
/// </summary>
public class AdxDataIngestionLocalStore : IAdxDataIngestionLocalStore
{
    private const int MaxThreshold = 50;
    private readonly ConcurrentQueue<AdxRow> _adxRowQueue;
    private readonly IAzureDataExplorerIngest _azureDataExplorerIngest;
    private readonly object createLockObject = new();
    private readonly ILogger<AdxDataIngestionLocalStore> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="logger">ILogger Instance</param>
    /// <param name="azureDataExplorerIngest">Azure Data Explorer Ingest Implementation Instance.</param>
    public AdxDataIngestionLocalStore(ILogger<AdxDataIngestionLocalStore> logger, IAzureDataExplorerIngest azureDataExplorerIngest)
    {
        _logger = logger;
        _azureDataExplorerIngest = azureDataExplorerIngest;
        _adxRowQueue = new ConcurrentQueue<AdxRow>();
    }

    /// <summary>
    /// Adds a record to the store for ingestion.
    /// </summary>
    /// <param name="database">Name of the ADX database.</param>
    /// <param name="tableName">Name of the table.</param>
    /// <param name="columnValues">Dictionary of datacolumn and values.</param>
    /// <param name="forceFlush">True to flush the data immediately; false when the threshold condition is met.</param>
    /// <returns>Number of rows inserted.</returns>
    public async Task<long> AddRecordForIngestion(string database, string tableName, IDictionary<AdxColumn, object> columnValues, bool forceFlush = false)
    {
        _adxRowQueue.Enqueue(new AdxRow(database, tableName, columnValues));

        _logger.LogDebug($"ADX Row added to {tableName} queue for ingestion. Total rows in the queue {_adxRowQueue.Count}");

        // Check for flush condition
        if (forceFlush || _adxRowQueue.Count > MaxThreshold)
        {
            _logger.LogDebug("ADX Flush getting called since ForceFlush is {ForceFlush} or QueueCount:{QueueCount} > maxThreshold:{MaxThreshold}",
                                    forceFlush, _adxRowQueue.Count, MaxThreshold);
            // Flush the data to ADX
            return await FlushLocalStore();
        }
        return 0;
    }

    /// <summary>
    /// Send all the data to ADX for ingestion.
    /// </summary>
    /// <returns>Awaitable task.</returns>
    public async Task<long> FlushLocalStore()
    {
        // Dequeue all rows from the concurrent queue to flush table
        var flushTables = DequeueRowsToFlushTable();
        long ingestedRowCount = 0;

        async Task ingestAndClearTable(DataTableEntry entry)
        {
            _logger.LogInformation("ADX Flushing database:{Database} table:{Table}", entry.DatabaseName, entry.TableName);

            // Send the data table to adx for ingestion.
            await MeasureExecutionTime.ExecuteTimed(() => Task.FromResult(_azureDataExplorerIngest.IngestFromDataTableAsync(entry.DatabaseName, entry.TableName, entry.DataTable)),
                (res, ms) =>
                {
                    _logger.LogInformation($"ADX Flush database:{entry.DatabaseName} table:{entry.TableName} completed for {entry.DataTable.Rows.Count} in {ms} seconds");
                });
            ingestedRowCount += entry.DataTable.Rows.Count;
        }

        // Get all non empty data tables
        var ingestionTasks = flushTables.Where(d => d.Value.DataTable.Rows.Count > 0).Select(t => ingestAndClearTable(t.Value)).ToList();

        // If no tables to flush; just return
        if (ingestionTasks.Count<1)
        {
            return 0;
        }

        _logger.LogInformation("Initiating ADX Flush Operation.");
        // await for all the task to complete
        await Task.WhenAll(ingestionTasks);

        _logger.LogInformation("ADX Flush Operation Completed.");
        return ingestedRowCount;
    }


    private Dictionary<string, DataTableEntry> DequeueRowsToFlushTable()
    {
        // Initialize dictionary of flush tables
        Dictionary<string, DataTableEntry> flushTables = new();

        // Dequeue the items to clone
        while (_adxRowQueue.TryDequeue(out AdxRow? row))
        {
            try
            {
                var tableEntry = GetOrCreateDataTableEntry(flushTables, row);
                var newRow = tableEntry.DataTable.NewRow();
                newRow.ItemArray = row.ColumnValues.Values.ToArray();
                tableEntry.DataTable.Rows.Add(newRow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding twin : {ADXRecord} to dataTable for ingestion.",JsonSerializer.Serialize(row.ColumnValues.Values));
            }
        }

        return flushTables;
    }

    private DataTableEntry GetOrCreateDataTableEntry(Dictionary<string, DataTableEntry> flushTables, AdxRow row)
    {
        // We lock the create method, so no two thread creates store for the same table name
        lock (createLockObject)
        {
            // Making additional check to ensure no previous threads have created a store for the same table name
            if (flushTables.TryGetValue(row.GetFullyQualifiedTableName(), out DataTableEntry? tableEntry))
                return tableEntry;
            var datatable = new DataTable(row.TableName);
            foreach (var column in row.ColumnValues.Keys)
            {
                var dataColumn = new DataColumn(column.Name, Type.GetType(column.ColumnType.GetDescription()!)!);
                datatable.Columns.Add(dataColumn);
            }

            tableEntry = new DataTableEntry(row.DatabaseName, row.TableName, datatable);
            flushTables.Add(tableEntry.GetFullyQualifiedTableName(), tableEntry);
            return tableEntry;
        }
    }
}
