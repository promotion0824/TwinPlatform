using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Willow.Directory.Models;
using Willow.Platform.Users;

namespace Willow.Management
{
    public interface IManagementAccessService
    {
       /// <summary>
        /// Ensure the currently logged in user has access to the given managed user (the user whose information is being edited)
        /// </summary>
        /// <param name="customerId">Id of customer/client</param>
        /// <param name="currentUserId">Currently logged in user</param>
        /// <param name="managedUserId">Id the user whose information is being edited</param>
        /// <param name="delete">Whether the user is being deleted</param>
        /// <returns>The edited user and their current role assignments</returns>
        Task<(User User, List<RoleAssignmentDto> RoleAssignments)> EnsureAccessUser(Guid customerId, Guid currentUserId, Guid managedUserId, bool delete = false);

        Task EnsureCanCreateUser(Guid customerId, Guid currentUserId);
    }
}
