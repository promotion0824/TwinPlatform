namespace ConnectorCore.Services
{
    using System;
    using System.Linq;
    using ConnectorCore.Models;
    using Microsoft.Extensions.Options;

    internal class MSBlobStorageService : IMSBlobStorageService
    {
        private readonly MSBlobStorageRootOptions msRootOptions;

        public MSBlobStorageService(IOptions<MSBlobStorageRootOptions> options)
        {
            msRootOptions = options.Value;
        }

        public bool TryGetMSBlobStorage(Guid connectorId, out ScannerBlobStorageOptions options)
        {
            if (msRootOptions.MSConnectorGuids == null || !msRootOptions.MSConnectorGuids.Contains(connectorId))
            {
                options = null;
                return false;
            }

            options = msRootOptions;
            return true;
        }
    }

    internal class MSBlobStorageRootOptions : ScannerBlobStorageOptions
    {
        public Guid[] MSConnectorGuids { get; set; } = Array.Empty<Guid>();
    }
}
