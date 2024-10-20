using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.ConnectorApi;
using Willow.Api.DataValidation;
using Willow.Management;
using Willow.DataValidation;

namespace PlatformPortalXL.Services
{
    public interface IScanValidationService
    {
        Task<List<ValidationErrorItem>> ValidateCreateConnectorScan(Guid siteId, Guid connectorId, string configuration);
    }

    public class ScanValidationService : IScanValidationService
    {
        private readonly IConnectorApiService _connectorApi;

        public ScanValidationService(IConnectorApiService connectorApi)
        {
            _connectorApi = connectorApi;
        }

        public async Task<List<ValidationErrorItem>> ValidateCreateConnectorScan(Guid siteId, Guid connectorId, string configuration)
        {
            var errors = new List<ValidationErrorItem>();
            var connector = await _connectorApi.GetConnectorById(siteId, connectorId);
            if (connector.IsEnabled)
            {
                errors.Add(new ValidationErrorItem
                {
                    Name = nameof(connectorId),
                    Message = "Cannot create new Scan request while Connector is Enabled."
                });
            }
            else
            {
                errors.AddRange(await ValidateConnectorScanConfiguration(connector.ConnectorTypeId, configuration));
            }

            var scansForController = await _connectorApi.GetConnectorScansAsync(connectorId);
            if (scansForController.Any(x => x.Status == ScanStatus.New || x.Status == ScanStatus.Scanning))
            {
                errors.Add(new ValidationErrorItem
                {
                    Name = nameof(connectorId),
                    Message = "Cannot create new Scan request while other Scan requests not finished."
                });
            }
            return errors;
        }

        private async Task<List<ValidationErrorItem>> ValidateConnectorScanConfiguration(Guid connectorTypeId, string configuration)
        {
            var connectorType = await _connectorApi.GetConnectorTypeAsync(connectorTypeId);
            if (connectorType.ScanConfigurationSchemaId == null)
            {
                return new List<ValidationErrorItem>();
            }

            try
            {
                var createScanRequest = JsonConvert.DeserializeObject<CreateScanRequest>(configuration);
                var errors = new List<(string Name, string Message)>();
                createScanRequest.Validate(errors);
                return errors.Select(i => new ValidationErrorItem(i.Name, i.Message)).ToList();
            }
            catch (JsonReaderException ex)
            {
                return new List<ValidationErrorItem> { new ValidationErrorItem
                        { Name = ex.Path, Message = $"{ex.Path} is invalid" }
                };
            }
        }
    }
}
