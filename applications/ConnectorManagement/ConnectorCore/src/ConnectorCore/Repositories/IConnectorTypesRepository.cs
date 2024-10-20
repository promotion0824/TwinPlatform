namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface IConnectorTypesRepository
    {
        Task<IList<ConnectorTypeEntity>> GetAllAsync();

        Task<ConnectorTypeEntity> GetItemAsync(Guid itemKey);

        Task<ConnectorTypeEntity> CreateAsync(ConnectorTypeEntity newItem);
    }
}
