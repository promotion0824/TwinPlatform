namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using Dapper;

    internal class SetPointCommandConfigurationsRepository : ISetPointCommandConfigurationsRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public SetPointCommandConfigurationsRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<IList<SetPointCommandConfigurationEntity>> GetListAsync()
        {
            using var conn = await connectionProvider.GetConnection();
            var sql = @"SELECT * FROM [dbo].[SetPointCommandConfiguration]";

            var data = await conn.QueryAsync<SetPointCommandConfigurationEntity>(sql);
            return data.ToList();
        }
    }
}
