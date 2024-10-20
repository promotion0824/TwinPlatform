namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface ISetPointCommandsService
    {
        Task<IList<SetPointCommandEntity>> GetListBySiteIdAsync(Guid siteId, Guid? equipmentId);

        Task<IList<SetPointCommandEntity>> GetListByConnectorIdAsync(Guid connectorId);

        Task<SetPointCommandEntity> GetItemAsync(Guid setPointCommandId);

        Task<SetPointCommandEntity> InsertAsync(SetPointCommandEntity entity);

        Task<SetPointCommandEntity> UpdateAsync(SetPointCommandEntity entity);

        Task DeleteAsync(Guid siteId, Guid setPointCommandId);
    }
}
