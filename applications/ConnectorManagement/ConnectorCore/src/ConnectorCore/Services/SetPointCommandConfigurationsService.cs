namespace ConnectorCore.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Repositories;

    internal class SetPointCommandConfigurationsService : ISetPointCommandConfigurationsService
    {
        private readonly ISetPointCommandConfigurationsRepository repository;

        public SetPointCommandConfigurationsService(ISetPointCommandConfigurationsRepository repository)
        {
            this.repository = repository;
        }

        public async Task<IList<SetPointCommandConfigurationEntity>> GetListAsync()
        {
            return await repository.GetListAsync();
        }
    }
}
