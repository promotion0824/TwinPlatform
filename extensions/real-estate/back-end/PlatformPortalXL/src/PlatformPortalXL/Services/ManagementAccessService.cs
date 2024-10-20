using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Web;

using Willow.Common;
using Willow.Data;
using Willow.Directory.Models;
using Willow.Platform.Users;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Willow.ExceptionHandling.Exceptions;

namespace Willow.Management
{
    public class ManagementAccessService : IManagementAccessService
    {
        private readonly IDirectoryApiService _directoryApi;
        private readonly IReadRepository<Guid, User>    _userRepo;

        private const string AdminRoleName = "Admin";
        private const string ViewerRoleName = "Viewer";

        public ManagementAccessService(IDirectoryApiService directoryApi, IReadRepository<Guid, User> userRepo)
        {
            _directoryApi = directoryApi ?? throw new ArgumentNullException(nameof(directoryApi));
            _userRepo  = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        #region IManagementAccessService

         public async Task<(User User, List<RoleAssignmentDto> RoleAssignments)> EnsureAccessUser(Guid customerId, Guid currentUserId, Guid managedUserId, bool delete = false)
        {
            User user = null;
            List<RoleAssignmentDto> managedUserAssignments;

            try
            {
                var currentUserAssignments = await CurrentUserIsValid(customerId, currentUserId);

                user = await _userRepo.Get(managedUserId);

                if (user == null || user.CustomerId != customerId)
                {
                    throw new NotFoundException();
                }

                managedUserAssignments = await _directoryApi.GetRoleAssignments(user.Id);
                if (managedUserAssignments.Any(m => m.CustomerId != customerId))
                {
                    throw new UnauthorizedAccessException();
                }

                // If current user is customer admin then authorized
                if (!currentUserAssignments.IsCustomerAdmin(customerId))
                {
                    if(delete)
                        throw new UnauthorizedAccessException();

                    // If current user is any portfolio admin then authorized
                    if(!currentUserAssignments.Any( a=> a.ResourceType == RoleResourceType.Portfolio && a.RoleId == WellKnownRoleIds.PortfolioAdmin))
                    {
                        // If current user is any site admin then authorized
                        if(!currentUserAssignments.Any( a=> a.ResourceType == RoleResourceType.Site && a.RoleId == WellKnownRoleIds.SiteAdmin))
                            throw new UnauthorizedAccessException();
                    }
                }
            }
            catch(Exception ex)
            {
                ex.Data.Merge(new { CustomerId = customerId, CurrentUserId = currentUserId, ManagedUserId = managedUserId}.ToDictionary() as IDictionary);
                throw;
            }

            return (user, managedUserAssignments);
        }

        public async Task EnsureCanCreateUser(Guid customerId, Guid currentUserId)
        {
            try
            {
                var currentUserAssignments = await CurrentUserIsValid(customerId, currentUserId);

                if(currentUserAssignments.IsCustomerAdmin(customerId))
                    return;

                // If current user is any portfolio admin then authorized
                if(currentUserAssignments.Any( a=> a.ResourceType == RoleResourceType.Portfolio && a.RoleId == WellKnownRoleIds.PortfolioAdmin))
                    return;

                // If current user is any site admin then authorized
                if(!currentUserAssignments.Any( a=> a.ResourceType == RoleResourceType.Site && a.RoleId == WellKnownRoleIds.SiteAdmin))
                    throw new UnauthorizedAccessException();
            }
            catch(Exception ex)
            {
                ex.Data.Merge(new { CustomerId = customerId, CurrentUserId = currentUserId }.ToDictionary() as IDictionary);
                throw;
            }
        }

        #endregion

        #region Private

        private async Task<List<RoleAssignmentDto>> CurrentUserIsValid(Guid customerId, Guid currentUserId)
        {
            // Make sure we're in same customer and current user exists
            try
            {
                var currentUser = await _userRepo.Get(currentUserId);

                if(currentUser == null || currentUser.CustomerId != customerId)
                    throw new NotFoundException();

                if(currentUser.Status != UserStatus.Active)
                    throw new UnauthorizedAccessException();
            }
            catch(NotFoundException)
            {
                throw new UnauthorizedAccessException();
            }

            var currentUserAssignments = await _directoryApi.GetRoleAssignments(currentUserId);

            // If current user has no roles then not authorized
            if(currentUserAssignments.Empty())
                throw new UnauthorizedAccessException();

            // If current user has roles that don't match customer then not authorized
            if(currentUserAssignments.Any( m=> m.CustomerId != customerId))
                throw new UnauthorizedAccessException();

            return currentUserAssignments;
        }

        #endregion
    }
}
