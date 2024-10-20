namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface IScanService
    {
        Task<List<ScanEntity>> GetByConnectorIdAsync(Guid connectorId);

        Task<ScanEntity> CreateAsync(ScanEntity newScanEntity);

        Task StopAsync(Guid connectorId, Guid scanId);

        Task<ScanEntity> GetById(Guid scanId);

        Task PatchAsync(
            Guid connectorId,
            Guid scanId,
            ScanStatus? status,
            string errorMessage,
            int? errorCount,
            DateTime? startTime,
            DateTime? endTime);
    }
}
