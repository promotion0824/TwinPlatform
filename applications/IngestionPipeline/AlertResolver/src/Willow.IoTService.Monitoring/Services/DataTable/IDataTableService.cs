using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Azure.Data.Tables;

namespace Willow.IoTService.Monitoring.Services.DataTable
{
    public interface IDataTableService
    {
        Task SaveEntity<T>(string tableName, T entity) where T : class, ITableEntity, new();
        Task SaveEntities<T>(string tableName, List<T> entities) where T : class, ITableEntity, new();
        Task<List<T>> GetEntitiesList<T>(string tableName, Expression<Func<T, Boolean>> predicate) where T : class, ITableEntity, new();
        Task<List<T>> GetEntitiesList<T>(string tableName) where T : class, ITableEntity, new();
        Task DeleteEntity(string tableName, string partitionKey, string rowKey);
        Task DeleteEntities<T>(string tableName, List<T> entities) where T : class, ITableEntity, new();
    }
}