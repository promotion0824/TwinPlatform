using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Api.Client;
using Willow.Directory.Models;
using WorkflowCore.Models;
using WorkflowCore.Services.Apis.Requests;

namespace WorkflowCore.Services.Apis
{
    public interface IDirectoryApiService
    {
        Task<List<Site>> GetSites(bool isInspectionEnabled);
        Task<Site> GetSite(Guid siteId);
        Task<List<User>> GetCustomerUsers(Guid customerId);
        Task<User> GetUser(Guid userId);
        Task<UserPreferences> GetUserPreferences(Guid customerId, Guid userId);
        Task<Customer> GetCustomer(Guid customerId);
        Task<List<UserProfile>> GetUsersProfilesAsync(GetUsersProfilesRequest getUsersProfilesRequest);
        Task<List<RoleAssignment>> GetUserRoleAssignments(Guid userId);
    }

    public class DirectoryApiService : IDirectoryApiService
    {
        private readonly IRestApi _restApi;

        public DirectoryApiService(IRestApi restApi)
        {
            _restApi = restApi;
        }

        public Task<List<User>> GetCustomerUsers(Guid customerId)
        {
            return _restApi.Get<List<User>>($"customers/{customerId}/users");
        }

        public Task<Customer> GetCustomer(Guid customerId)
        {
            return _restApi.Get<Customer>($"customers/{customerId}");
        }

        public Task<List<Site>> GetSites(bool isInspectionEnabled)
        {
            return _restApi.Get<List<Site>>($"sites?isInspectionEnabled={isInspectionEnabled}");
        }

        public Task<Site> GetSite(Guid siteId)
        {
            return _restApi.Get<Site>($"sites/{siteId}");
        }

        public Task<User> GetUser(Guid userId)
        {
            return _restApi.Get<User>($"users/{userId}");
        }

        public Task<UserPreferences> GetUserPreferences(Guid customerId, Guid userId)
        {
            return _restApi.Get<UserPreferences>($"customers/{customerId}/users/{userId}/preferences");
        }

        public Task<List<UserProfile>> GetUsersProfilesAsync(GetUsersProfilesRequest getUsersProfilesRequest)
        {
            return _restApi.Post<GetUsersProfilesRequest, List<UserProfile>>("users/profiles", getUsersProfilesRequest);
        }

        public Task<List<RoleAssignment>> GetUserRoleAssignments(Guid userId)
        {
            return _restApi.Get<List<RoleAssignment>>($"users/{userId}/permissionAssignments");
        }
    }  
}
