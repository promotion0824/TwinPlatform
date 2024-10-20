using Kusto.Data.Common;
using System.Text.Json;
using Willow.AzureDataExplorer.Command;
using Willow.AzureDataExplorer.Model;

namespace Willow.AzureDataExplorer.Infra;

public interface IAzureDataExplorerInfra
{
    Task DropTableAsync(string database, string table, bool ifexists = true);
    Task SwapTablesAsync(string database, string table, string secondTable);
    Task CreateTableAsync(string database, string table, IEnumerable<Tuple<string, string>> rowFields, bool ingestionTimeEnabled = true);
    Task CreateOrAlterFunction(string database, string name, string body, string? folder = null);
    Task CreateMaterializedView(string database, string name, string table, string body, bool backFill = true, bool dropAndRecreate = true);
    Task<Model.DatabaseSchema?> GetDatabaseSchemaAsync(string database);
    Task<IEnumerable<string>> GetMaterializedViews(string database);

    Task AlterMergeTable(string database, string table, string newColumnName, string columnType);
    Task AlterTableHotCaching(string database, string table, int cachePeriodInDays);
}

public class AzureDataExplorerInfra : IAzureDataExplorerInfra
{
    private readonly IAzureDataExplorerCommand _azureDataExplorerCommand;
    public AzureDataExplorerInfra(IAzureDataExplorerCommand azureDataExplorerCommand)
    {
        _azureDataExplorerCommand = azureDataExplorerCommand;
    }

    public async Task DropTableAsync(string database, string table, bool ifexists = true)
    {
        var command = $".drop tables({table})";
        if (ifexists)
            command += " ifexists";

       using var reader =  await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, command);
    }

    public async Task SwapTablesAsync(string database, string table, string secondTable)
    {
        var command = $".rename tables {table}={secondTable}, {secondTable}={table}";
        using var reader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, command);
    }

    public async Task<Model.DatabaseSchema?> GetDatabaseSchemaAsync(string database)
    {
        var command = $".show database ['{database}'] schema as json";
        using var schemaReader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, command);
        DatabasesSchema? databasesSchema = null;

        while (schemaReader.Read())
        {
            var schema = schemaReader["DatabaseSchema"];
            if (schema != null)
                databasesSchema = JsonSerializer.Deserialize<DatabasesSchema>(schema.ToString() ?? string.Empty);
        }


        if (databasesSchema?.Databases != null && databasesSchema.Databases.TryGetValue(database, out var db))
        {
            return db;
        }

        return null;
    }

    public async Task<IEnumerable<string>> GetMaterializedViews(string database)
    {
        var command = $".show materialized-views details | project MaterializedViewName";

        using var schemaReader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, command);

        List<string> result = [];
        while(schemaReader.Read())
        {
            if(schemaReader["MaterializedViewName"] is string value) {  result.Add(value); }
        }
        return result;
    }

    public async Task<IEnumerable<string>> GetFunctions(string database)
    {
        var command = $".show materialized-views details | project MaterializedViewName";

        using var schemaReader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, command);

        List<string> result = [];
        while (schemaReader.Read())
        {
            if (schemaReader["MaterializedViewName"] is string value) { result.Add(value); }
        }
        return result;
    }

    public async Task CreateTableAsync(string database, string table, IEnumerable<Tuple<string, string>> rowFields, bool ingestionTimeEnabled = true)
    {
        var commandCreate = CslCommandGenerator.GenerateTableCreateCommand(
            table,
            rowFields);

        using var reader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, commandCreate);

        if (ingestionTimeEnabled)
        {
            var command = $".alter table {table} policy ingestiontime true";
            using var ingestionReader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, command);
        }
    }

    public async Task CreateOrAlterFunction(string database, string name, string body, string? folder = null)
    {
        var command = $".create-or-alter function with {(folder != null ? $"(folder='{folder}')" : string.Empty)} {name}()  {{ {body} }}";
        using var reader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, command);
    }

    public async Task CreateMaterializedView(
                                    string database,
                                    string name,
                                    string table,
                                    string body,
                                    bool backFill = true,
                                    bool dropAndRecreate = true)
    {
        // Note warning from MS docs:
        // On large source tables, the backfill option might take a long time to complete.
        // If this process transiently fails while running, it won't be automatically retried.
        // You must then re-execute the create command.

        if (dropAndRecreate)
        {
            var dropCmd = $".drop materialized-view {name} ifexists";
            using var dropCmdReader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, dropCmd);

            var createCmd = $".create {(backFill ? "async " : "")} materialized-view {(backFill ? "with (backfill=true)" : string.Empty)} {name} on table {table} {{ {body} }}";
            using var createCmdReader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, createCmd);
        }
        else
        {
            // Create the view if it doesn't exist already, otherwise leave it alone
            // This assumes that the view query has not changed.
            var createCmd = $".create {(backFill ? "async " : "")} ifnotexists materialized-view {(backFill ? "with (backfill=true)" : string.Empty)} {name} on table {table} {{ {body} }}";
            using var reader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, createCmd);

            // Alter the existing view --May not be compatible with all options:
            // https://learn.microsoft.com/en-us/azure/data-explorer/kusto/management/materialized-views/materialized-view-create-or-alter
            //var createCmd = $".create-or-alter materialized-view {(backFill ? "with (backfill=true)" : string.Empty)} {name} on table {table} {{ {body} }}";
            //await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, createCmd);
        }
    }

    public async Task AlterMergeTable(
                                    string database,
                                    string table,
                                    string newColumnName,
                                    string columnType)
    {
        var alterMergeTableCmd = $".alter-merge table {table} ({newColumnName}: {columnType})";
        using var reader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, alterMergeTableCmd);
    }

    /// <summary>
    /// Alter or Overwrite ADX table hot caching policy
    /// </summary>
    /// <param name="database">Name of the database</param>
    /// <param name="table">Name of the ADX table</param>
    /// <param name="cachePeriodInDays">Number of cache period in days.</param>
    /// <returns></returns>
    public async Task AlterTableHotCaching(string database, string table, int cachePeriodInDays)
    {
        var command = $".alter table {table} policy caching hot = {cachePeriodInDays}d";
        using var reader = await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, command);
    }
}
