namespace ConnectorCore.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface ISetPointCommandConfigurationsService
    {
        Task<IList<SetPointCommandConfigurationEntity>> GetListAsync();
    }
}
