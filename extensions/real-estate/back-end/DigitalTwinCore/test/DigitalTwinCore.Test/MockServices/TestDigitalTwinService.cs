using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.AdtApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DigitalTwinCore.Test.MockServices
{
    public class TestDigitalTwinService : DigitalTwinService
    {
        private readonly IConfiguration _config;

        public TestDigitalTwinService(IAdtApiService adtApiService, IConfiguration config, ILogger<DigitalTwinService> logger = null)
            : base(adtApiService, config, logger)
        {
            _config = config;
        }

        public IAdtApiService AdtApiService => _adtApiService;

        public void Reload()
        {
            _digitalTwinCache = new DigitalTwinCache(_adtApiService, SiteAdtSettings.InstanceSettings, _config);
        }
    }
}
