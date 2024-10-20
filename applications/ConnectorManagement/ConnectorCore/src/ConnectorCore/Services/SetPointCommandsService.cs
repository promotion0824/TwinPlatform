namespace ConnectorCore.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Repositories;
    using Willow.Infrastructure.Exceptions;

    internal class SetPointCommandsService : ISetPointCommandsService
    {
        private readonly ISetPointCommandsRepository setPointCommandsRepository;

        public SetPointCommandsService(ISetPointCommandsRepository setPointCommandsRepository)
        {
            this.setPointCommandsRepository = setPointCommandsRepository;
        }

        public async Task DeleteAsync(Guid siteId, Guid setPointCommandId)
        {
            await setPointCommandsRepository.DeleteAsync(siteId, setPointCommandId);
        }

        public async Task<SetPointCommandEntity> GetItemAsync(Guid setPointCommandId)
        {
            var result = await setPointCommandsRepository.GetItemAsync(setPointCommandId);
            if (result == null)
            {
                throw new ResourceNotFoundException("SetPointCommand", setPointCommandId);
            }

            return result;
        }

        public async Task<IList<SetPointCommandEntity>> GetListByConnectorIdAsync(Guid connectorId)
        {
            return await setPointCommandsRepository.GetByConnectorIdAsync(connectorId);
        }

        public async Task<IList<SetPointCommandEntity>> GetListBySiteIdAsync(Guid siteId, Guid? equipmentId)
        {
            return (await setPointCommandsRepository.GetBySiteIdAsync(siteId))
                .Where(s => equipmentId == null || equipmentId.Equals(s.EquipmentId))
                .ToList();
        }

        public async Task<SetPointCommandEntity> InsertAsync(SetPointCommandEntity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            entity.Status = SetPointCommandStatus.Submitted;
            entity.CreatedAt = DateTime.UtcNow;
            entity.LastUpdatedAt = DateTime.UtcNow;

            await setPointCommandsRepository.InsertAsync(entity);

            return await GetItemAsync(entity.Id);
        }

        public async Task<SetPointCommandEntity> UpdateAsync(SetPointCommandEntity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                throw new BadRequestException("Entity id must be a valid GUID");
            }

            entity.LastUpdatedAt = DateTime.UtcNow;

            await setPointCommandsRepository.UpdateAsync(entity);

            return await GetItemAsync(entity.Id);
        }
    }
}
