using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Willow.Database
{
    public interface IDatabase
    {
        Task<IEnumerable<T>> QueryList<T>(string sql, object param = null);
        Task<IEnumerable<T>> QueryList<T>(string sql, Dictionary<string, object> dictionaryParam);
        Task<T> Query<T>(string sql, object param = null);
    }

    public class Database : IDatabase
    {
        private readonly string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<T>> QueryList<T>(string sql, object param = null)
        {
            using (var connection = NewConnection())
            {
                return await connection.QueryAsync<T>(
                    sql,
                    param: param,
                    commandType: CommandType.Text
                );
            }
        }

        public async Task<T> Query<T>(string sql, object param = null)
        {
            var resultList = await QueryList<T>(sql, param);

            return resultList.FirstOrDefault();
        }

        public async Task<IEnumerable<T>> QueryList<T>(
            string sql,
            Dictionary<string, object> dictionaryParam
        )
        {
            var parameters = new DynamicParameters(dictionaryParam);
            return await QueryList<T>(sql, parameters);
        }

        protected virtual DbConnection NewConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.UseManagedIdentity();
            connection.Open();
            return connection;
        }
    }
}
