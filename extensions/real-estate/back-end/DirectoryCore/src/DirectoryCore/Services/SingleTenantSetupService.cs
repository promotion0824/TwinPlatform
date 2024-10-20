using System;
using System.Threading.Tasks;
using DirectoryCore.Configs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryCore.Services.UserSetupService
{
    public interface ISingleTenantSetupService
    {
        // <summary>
        // If the IsSingleTenant config value is true, set up the required
        // data for it to be possible to log in to a single tenant instance.
        // </summary>
        Task SetupSingleTenantData();
    }

    public class SingleTenantSetupService : ISingleTenantSetupService
    {
        private readonly WillowContextOptions _willowContextOptions;
        private readonly SingleTenantOptions _singleTenantOptions;
        private readonly ILogger<SingleTenantSetupService> _logger;
        private readonly ICustomersService _customerService;

        public SingleTenantSetupService(
            IOptions<WillowContextOptions> options,
            IOptions<SingleTenantOptions> SingleTenantOptions,
            ILogger<SingleTenantSetupService> logger,
            ICustomersService customerService
        )
        {
            _customerService = customerService;
            _willowContextOptions = options.Value;
            _singleTenantOptions = SingleTenantOptions.Value;
            _logger = logger;
        }

        public async Task SetupSingleTenantData()
        {
            if (!_singleTenantOptions.IsSingleTenant)
            {
                _logger.LogInformation(
                    "Not running in single tenant, so no need to setup single tenant data"
                );
                return;
            }

            var customerId = _willowContextOptions.CustomerInstanceConfiguration.Id;
            await _customerService.SetupSingleTenantData(
                new()
                {
                    Id = customerId,
                    Name = _willowContextOptions.CustomerConfiguration.DisplayName,
                    // We do not have configuration variables for these values. We may not ever
                    // end up needing them, we might want to replace anything that uses these Customer
                    // fields with direct use of configuration values anyway.
                    Country = "",
                    SigmaConnectionId = "",
                    AccountExternalId = ""
                }
            );
        }
    }
}
