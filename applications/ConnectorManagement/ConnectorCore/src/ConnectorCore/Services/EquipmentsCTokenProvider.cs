namespace ConnectorCore.Services
{
    using System;
    using ConnectorCore.Common.Abstractions;
    using ConnectorCore.Entities;
    using Willow.Infrastructure.Exceptions;

    internal class EquipmentsCTokenProvider : IContinuationTokenProvider<EquipmentEntity, Guid>
    {
        public string GetToken(EquipmentEntity item)
        {
            var id = item.Id.ToString();

            var r = $"{id}";

            return r;
        }

        public Guid ParseToken(string token)
        {
            if (!Guid.TryParse(token, out var id))
            {
                throw new BadRequestException("Token has incorrect format: id can't be parsed.");
            }

            return id;
        }
    }
}
