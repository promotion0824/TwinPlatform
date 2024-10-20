using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Willow.Platform.Users
{
    public interface IUserService
    {
        Task<User>            GetCustomerUser(Guid customerId, Guid userId);
        Task<IUser>           GetUser(Guid siteId, Guid userId, UserType type = UserType.Unknown);
        Task<IList<IUser>>    GetUsers(Guid siteId, IEnumerable<(Guid UserId, UserType UserType)> userIds, UserType types);
        Task<User>            GetManagedUser(Guid customerId, Guid userId);
    }
}
