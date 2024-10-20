using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Willow.Management
{
    public interface IManagementService
    {
        Task<ManagedUserDto> GetManagedUser(Guid customerId, Guid currentUserId, Guid managedUserId);
        Task<ManagedUserDto> CreateManagedUser(Guid customerId, Guid currentUserId, CreateManagedUserRequest request, string language);
        Task UpdateManagedUser(Guid customerId, Guid currentUserId, Guid managedUserId, UpdateManagedUserRequest request, string language);
        Task DeleteManagedUser(Guid customerId, Guid currentUserId, Guid managedUserId);

        Task<List<ManagedPortfolioDto>> GetManagedPortfolios(Guid customerId, Guid userId);
    }
}
