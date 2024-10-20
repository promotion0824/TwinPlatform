using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Willow.IoTService.Monitoring.Options;

namespace Willow.IoTService.Monitoring.Services.DataTable
{
    public class DataTableService : IDataTableService
    {
        private readonly DataTableOptions _dataTableOptions;
        private TableServiceClient? _tableServiceClient;
        private static readonly object _tableServiceClientInitLock = new object();

        public DataTableService(DataTableOptions dataTableOptions)
        {
            _dataTableOptions = dataTableOptions;
        }

        public async Task SaveEntity<T>(string tableName, T entity) where T : class, ITableEntity, new()
        {
            var tableClient = await GetTableClient(tableName);

            await tableClient.UpsertEntityAsync(entity);
        }

        public async Task SaveEntities<T>(string tableName, List<T> entities) where T : class, ITableEntity, new()
        {
            var tableClient = await GetTableClient(tableName);

            var upsertBatch = new List<TableTransactionAction>();
            upsertBatch.AddRange(entities.Select(f => new TableTransactionAction(TableTransactionActionType.UpsertMerge, f)));

            await tableClient.SubmitTransactionAsync(upsertBatch);
        }

        private async Task<AsyncPageable<T>> GetEntitiesPageable<T>(string tableName, Expression<Func<T, Boolean>> predicate) where T : class, ITableEntity, new()
        {
            var tableClient = await GetTableClient(tableName);

            var queryResults = tableClient.QueryAsync<T>(predicate);

            return queryResults;
        }

        private async Task<AsyncPageable<T>> GetAllEntitiesPageable<T>(string tableName) where T : class, ITableEntity, new()
        {
            var tableClient = await GetTableClient(tableName);

            var queryResults = tableClient.QueryAsync<T>();

            return queryResults;
        }

        public async Task<List<T>> GetEntitiesList<T>(string tableName, Expression<Func<T, Boolean>> predicate) where T : class, ITableEntity, new()
        {
            var queryResults = await GetEntitiesPageable<T>(tableName, predicate);

            var entitiesList = new List<T>();

            await foreach (Page<T> page in queryResults.AsPages())
            {
                foreach (T entity in page.Values)
                {
                    entitiesList.Add(entity);
                }
            }

            return entitiesList;
        }

        public async Task<List<T>> GetEntitiesList<T>(string tableName) where T : class, ITableEntity, new()
        {
            var queryResults = await GetAllEntitiesPageable<T>(tableName);

            var entitiesList = new List<T>();

            await foreach (Page<T> page in queryResults.AsPages())
            {
                foreach (T entity in page.Values)
                {
                    entitiesList.Add(entity);
                }
            }

            return entitiesList;
        }

        public async Task DeleteEntity(string tableName, string partitionKey, string rowKey)
        {
            var tableClient = await GetTableClient(tableName);

            await tableClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task DeleteEntities<T>(string tableName, List<T> entities) where T : class, ITableEntity, new()
        {
            var tableClient = await GetTableClient(tableName);

            var deleteBatch = new List<TableTransactionAction>();
            deleteBatch.AddRange(entities.Select(f => new TableTransactionAction(TableTransactionActionType.Delete, f)));

            await tableClient.SubmitTransactionAsync(deleteBatch);
        }

        private async Task<TableClient> GetTableClient(string tableName)
        {
            if (_tableServiceClient == null)
            {
                lock (_tableServiceClientInitLock)
                {
                    if (_tableServiceClient == null)
                    {
                        _tableServiceClient = new TableServiceClient(_dataTableOptions.ConnectionString);
                    }
                }
            }

            await _tableServiceClient.CreateTableIfNotExistsAsync(tableName);

            return _tableServiceClient.GetTableClient(tableName);
        }
    }
}