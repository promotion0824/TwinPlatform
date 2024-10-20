namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface ISetPointCommandsRepository
    {
        Task<SetPointCommandEntity> GetItemAsync(Guid entityId);

        Task<IList<SetPointCommandEntity>> GetBySiteIdAsync(Guid siteId);

        Task<IList<SetPointCommandEntity>> GetByConnectorIdAsync(Guid connectorId);

        Task InsertAsync(SetPointCommandEntity entity);

        Task UpdateAsync(SetPointCommandEntity entity);

        Task DeleteAsync(Guid siteId, Guid entityId);
    }
}
