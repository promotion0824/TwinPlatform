using Microsoft.Extensions.Configuration;

namespace AssetCoreTwinCreator.Database
{
    public interface IDatabaseConfiguration
    {
        string BuildConnectionString { get; }
    }

    public class DatabaseConfiguration : IDatabaseConfiguration
    {
        public string BuildConnectionString { get; }

        public DatabaseConfiguration(IConfiguration configuration)
        {
            BuildConnectionString = configuration["ConnectionStrings:AssetDb"];
        }
    }
}