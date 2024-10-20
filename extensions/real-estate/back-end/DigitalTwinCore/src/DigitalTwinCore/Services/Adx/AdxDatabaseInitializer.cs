using System.Threading.Tasks;
using DigitalTwinCore.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DigitalTwinCore.Services.Adx
{
    public interface IAdxDatabaseInitializer
    {
        // <summary>
        // If the ADX:EnsureDatabaseObjectsExist setting is true, and there are no tables
        // in the database specified by the ADX:ClusterUri and ADX:DatabaseName settings,
        // create the required tables, materialized views, and functions. If any tables
        // already exist, do nothing.
        // </summary>
        Task EnsureDatabaseObjectsExist();
    }

    public class AdxDatabaseInitializer : IAdxDatabaseInitializer
    {
        private readonly IAdxHelper _adxHelper;
        private readonly AzureDataExplorerSettings _settings;
        private readonly ILogger<AdxDatabaseInitializer> _logger;

        public AdxDatabaseInitializer(
            IOptions<AzureDataExplorerSettings> options,
            IAdxHelper adxHelper,
            ILogger<AdxDatabaseInitializer> logger)
        {
            _adxHelper = adxHelper;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task EnsureDatabaseObjectsExist()
        {
            if (_settings.EnsureDatabaseObjectsExist)
            {
                _logger.LogInformation(
                    "Ensuring database {DatabaseName} "
                    + "in cluster {ClusterUri} has tables",
                    _settings.DatabaseName,
                    _settings.ClusterUri
                );
                var queryProvider = AdxHelper.MakeCslAdminProvider(_settings.ClusterUri);
                var databaseName = _settings.DatabaseName;
                await _adxHelper.SetupADXIfEmpty(queryProvider, databaseName);
            }
            else
            {
                _logger.LogInformation(
                    "ADX:EnsureDatabaseObjectsExist was not set; "
                    + "not checking database objects"
                );
            }
        }
    }
}
