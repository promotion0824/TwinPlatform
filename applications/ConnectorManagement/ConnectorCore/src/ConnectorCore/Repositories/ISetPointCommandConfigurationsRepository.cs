namespace ConnectorCore.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface ISetPointCommandConfigurationsRepository
    {
        Task<IList<SetPointCommandConfigurationEntity>> GetListAsync();
    }
}
