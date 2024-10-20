namespace ConnectorCore.Database
{
    using Microsoft.Extensions.Configuration;

    internal class DbConnectionStringProvider : IDbConnectionStringProvider
    {
        private readonly IConfiguration configuration;

        public DbConnectionStringProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string GetConnectionString()
        {
            return configuration.GetConnectionString("ConnectorCoreDbConnection");
        }
    }
}
