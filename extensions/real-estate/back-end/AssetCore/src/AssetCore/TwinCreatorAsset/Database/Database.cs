using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace AssetCoreTwinCreator.Database
{
    public interface IDatabase
    {
        Task<IEnumerable<T>> QueryList<T>(DatabaseInstance instance, string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null);
        Task<T> Query<T>(DatabaseInstance instance, string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null);
        Task<IEnumerable<TReturn>> Query<TFirst, TSecond, TReturn>(DatabaseInstance instance, string sql, Func<TFirst, TSecond, TReturn> map, string splitOn, object parameters = null, CommandType? commandType = null, int? commandTimeout = null);
    }

    public enum DatabaseInstance
    {
        Build,
    }

    public class Database : IDatabase
    {
        private readonly IDatabaseConfiguration _configuration;

        public Database(IDatabaseConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IEnumerable<T>> QueryList<T>(DatabaseInstance instance, string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null)
        {
            using (var connection = NewConnection(instance))
            {
                return commandType != null
                    ? await connection.QueryAsync<T>(sql, parameters, commandType: commandType, commandTimeout: commandTimeout)
                    : await connection.QueryAsync<T>(sql, parameters, commandTimeout: commandTimeout);
            }
        }

        public async Task<T> Query<T>(DatabaseInstance instance, string sql, object parameters = null, CommandType? commandType = null, int? commandTimeout = null)
        {
            var resultList = await QueryList<T>(instance, sql, parameters, commandType, commandTimeout);
            return resultList.FirstOrDefault();
        }

        public async Task<IEnumerable<TReturn>> Query<TFirst, TSecond, TReturn>(DatabaseInstance instance, string sql, Func<TFirst, TSecond, TReturn> map,  string splitOn, object parameters = null, CommandType? commandType = null, int? commandTimeout = null)
        {
            using (var connection = NewConnection(instance))
            {
                return commandType != null
                    ? await connection.QueryAsync<TFirst, TSecond, TReturn>(sql, map, parameters, splitOn: splitOn, commandType: commandType, commandTimeout: commandTimeout)
                    : await connection.QueryAsync<TFirst, TSecond, TReturn>(sql, map, parameters, splitOn: splitOn, commandTimeout: commandTimeout);
            }
        }

        protected virtual DbConnection NewConnection(DatabaseInstance instance)
        {
            var connectionString = ConnectionString(instance);
            var connection = new SqlConnection(connectionString);
            connection.UseManagedIdentity();
            connection.Open();
            return connection;
        }

        protected string ConnectionString(DatabaseInstance instance)
        {
            string connectionString;
            switch (instance)
            {
                case DatabaseInstance.Build:
                    connectionString = _configuration.BuildConnectionString;
                    break;
                default:
                    throw new NotSupportedException($"Database instance has not been configured {instance}");
            }

            return connectionString;
        }
    }
}
