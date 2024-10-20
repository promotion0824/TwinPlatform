using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Willow.Api.Client;
using Willow.Common;
using Willow.Data;
using Willow.ExceptionHandling.Exceptions;

namespace Willow.Platform.Users
{
    public class UserService : IUserService
    {
        private readonly IReadRepository<Guid, User> _customerUserRepo;
        private readonly IReadRepository<SiteObjectIdentifier, Workgroup> _workgroupRepo;
        private readonly IRestApi _directoryApi;

        public UserService(IReadRepository<Guid, User> customerUserRepo, IReadRepository<SiteObjectIdentifier, Workgroup> workgroupRepo, IRestApi directoryApi)
        {
            _customerUserRepo = customerUserRepo;
            _workgroupRepo   = workgroupRepo;

            _directoryApi = directoryApi;
        }

        public Task<User> GetCustomerUser(Guid customerId, Guid userId)
        {
            return _directoryApi.Get<User>($"customers/{customerId}/users/{userId}");
        }

        public async Task<IUser> GetUser(Guid siteId, Guid userId, UserType type = UserType.Unknown)
        {
            var result = await GetUsers(siteId, new List<(Guid, UserType)> { (userId, type) }, type);

            if(result.Count == 0)
                throw new NotFoundException();

            return result.First();
        }

        public async Task<User> GetManagedUser(Guid customerId, Guid userId)
        {
            var user = await _customerUserRepo.Get(userId);

            if(user == null || user.CustomerId != customerId)
                throw new NotFoundException();

            return user;
        }

        public async Task<IList<IUser>> GetUsers(Guid siteId, IEnumerable<(Guid UserId, UserType UserType)> userIds, UserType types)
        {
            var list = new List<IUser>();

            // Search for customers or users with unknown type
            if((types & UserType.Customer) == UserType.Customer)
            {
                var users = await GetCustomerUsersAsInterface(userIds.Where( u=> u.UserType.HasFlag(UserType.Customer)).Select( u=> u.UserId));

                list.AddRange(users);
            }

            if(list.Count != userIds.Count())
            {
                // Search for workgroups or users with unknown type
                if(list.Count != userIds.Count() && (types & UserType.Workgroup) == UserType.Workgroup)
                {
                    var ids        = userIds.Where( u=> u.UserType.HasFlag(UserType.Workgroup)).Select( u=> u.UserId ).Except(list.Select( i=> i.Id) );
                    var workgroups = await GetWorkgroupsAsUsers(siteId, ids);

                    list.AddRange(workgroups);
                }
            }

            return list;
        }

        #region Private

        private async Task<IList<IUser>> GetCustomerUsersAsInterface(IEnumerable<Guid> userIds)
        {
            return await GetCustomerUsers(userIds);
        }

        private Task<IList<IUser>> GetCustomerUsers(IEnumerable<Guid> userIds)
        {
            return GetPrincipals( ()=>
            {
                return _customerUserRepo.Get(userIds);
            });
        }

        private Task<IList<IUser>> GetWorkgroupsAsUsers(Guid siteId, IEnumerable<Guid> userIds)
        {
            return GetPrincipals( ()=>
            {
                return _workgroupRepo.Get(siteId, userIds);
            });
        }

        private static async Task<IList<IUser>> GetPrincipals(Func<IAsyncEnumerable<IUser>> fnGet)
        {
            try
            {
                var asyncResult = fnGet();

                if(asyncResult == null)
                    return new List<IUser>();

                return await asyncResult.ToList();
            }
            catch(Exception ex) when (ex.Message.Contains("Handler did not return a response message", StringComparison.InvariantCultureIgnoreCase))
            {
                return new List<IUser>();
            }
            catch(Exception ex) when (ex.Message.Contains("Error response is in media type unknown", StringComparison.InvariantCultureIgnoreCase))
            {
                return new List<IUser>();
            }
            catch(NotFoundException)
            {
                return new List<IUser>();
            }
            catch(Exception)
            {
                return new List<IUser>();
            }
        }

        #endregion
    }
}
