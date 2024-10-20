using Kusto.Data.Common;
using Kusto.Data.Ingestion;
using Kusto.Ingest;
using Polly;
using System.Data;
using Willow.AzureDataExplorer.Builders;
using Willow.AzureDataExplorer.Command;

namespace Willow.AzureDataExplorer.Ingest
{
    public interface IAzureDataExplorerIngest
    {
        Task IngestFromStorageAsync(string blob, string database, string table, string mapping, DataSourceFormat dataSourceFormat);
        Task CreateTableMappingAsync(string database, string table, IngestionMappingKind kind, string mappingName, IEnumerable<(string, string)> columnMappings, bool removeIfRequired);
        Task IngestInline(string database, string table, List<string> values);
        Task IngestFromDataTableAsync(string database, string table, DataTable datatable);

        Task IngestFromDataReaderAsync<T>(string database, string table, List<string> propertyNames, IEnumerable<T> data);
    }

    public class AzureDataExplorerIngest : IAzureDataExplorerIngest
    {
        private readonly IClientBuilder _clientBuilder;
        private readonly IAzureDataExplorerCommand _azureDataExplorerCommand;

        private readonly Polly.Retry.AsyncRetryPolicy _retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(2));

        public AzureDataExplorerIngest(IClientBuilder clientBuilder, IAzureDataExplorerCommand azureDataExplorerCommand)
        {
            _clientBuilder = clientBuilder;
            _azureDataExplorerCommand = azureDataExplorerCommand;
        }

        public async Task IngestFromStorageAsync(string blob, string database, string table, string mapping, DataSourceFormat dataSourceFormat)
        {
            var retryPolicy = Policy.Handle<Kusto.Ingest.Exceptions.DirectIngestClientException>(x => !x.IsPermanent)
                .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(30));

            await retryPolicy.ExecuteAsync(async () =>
            {
                var properties =
                        new KustoQueuedIngestionProperties(database, table)
                        {
                            Format = dataSourceFormat,
                            IngestionMapping = new IngestionMapping()
                            {
                                IngestionMappingReference = mapping
                            }
                        };
                var kustoIngestClient = await _clientBuilder.GetKustoIngestClient;
                await kustoIngestClient.IngestFromStorageAsync(blob, properties);
            });
        }

        public async Task IngestFromDataReaderAsync<T>(string database, string table, List<string> propertyNames, IEnumerable<T> data)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var properties =
                        new KustoIngestionProperties(database, table);

                var kustoIngestClient = await _clientBuilder.GetKustoIngestClient;
                await kustoIngestClient.IngestFromDataReaderAsync(GetDataAsIDataReader(data.ToList(), propertyNames), properties);
            });
        }

        public async Task IngestFromDataTableAsync(string database, string table, DataTable datatable)
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var properties =
                        new KustoIngestionProperties(database, table);

                var kustoIngestClient = await _clientBuilder.GetKustoIngestClient;
                await kustoIngestClient.IngestFromDataReaderAsync(datatable.CreateDataReader(), properties);
            });
        }

        static IDataReader GetDataAsIDataReader<T>(IEnumerable<T> values, List<string> propertyNames) => new Kusto.Cloud.Platform.Data.EnumerableDataReader<T>(values, propertyNames.ToArray());

        public async Task CreateTableMappingAsync(string database, string table, IngestionMappingKind kind, string mappingName, IEnumerable<(string, string)> columnMappings, bool removeIfRequired)
        {
            var commandMap =
                CslCommandGenerator.GenerateTableMappingCreateOrAlterCommand(
                    kind,
                    table,
                    mappingName,
                    columnMappings.Select(x => new ColumnMapping
                    {
                        ColumnName = x.Item1,
                        Properties = new Dictionary<string, string>()
                        {
                            { "path", x.Item2 }
                        }
                    }),
                    removeIfRequired);

            await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, commandMap);
        }

        public async Task IngestInline(string database, string table, List<string> values)
        {
            var valuesString = string.Join(",", values.Select(x => string.IsNullOrEmpty(x) ? string.Empty : $"\"{x.Replace("\"", "\"\"")}\""));

            await _retryPolicy.ExecuteAsync(async () =>
            {
                var command = $".ingest inline into table {table} <| {valuesString}";
                await _azureDataExplorerCommand.ExecuteControlCommandAsync(database, command);
            });
        }
    }
}
