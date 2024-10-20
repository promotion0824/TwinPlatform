using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MobileXL.Models;
using MobileXL.Services.Apis.DirectoryApi;
using Willow.Infrastructure.Exceptions;

using Willow.Common;

namespace MobileXL.Services
{
    public interface IAccessControlService
    {
        Task EnsureAccessSite(string userType, Guid userId, Guid siteId);
        Task<bool> CanCustomerUserAccessSite(Guid customerUserId, string permissionId, Guid siteId);
    }

    public class AccessControlService : IAccessControlService
    {
        private readonly IMemoryCache _cache;
        private readonly IDirectoryApiService _directoryApi;

        public AccessControlService(IMemoryCache cache, IDirectoryApiService directoryApi)
        {
            _cache = cache;
            _directoryApi = directoryApi;
        }

        public async Task EnsureAccessSite(string userType, Guid userId, Guid siteId)
        {
            if (userType == UserTypeNames.CustomerUser)
            {
                await EnsureCustomerUserAccessSite(userId, Permissions.ViewSites, siteId);
            }
            else
            {
                throw new ArgumentException($"Current user has an unknown user type {userType}.");
            }
        }

        public async Task<bool> CanCustomerUserAccessSite(Guid customerUserId, string permissionId, Guid siteId)
        {
            var canAccess = await CheckPermissionWithCache(customerUserId, permissionId, siteId: siteId);
            return canAccess;
        }

        public async Task EnsureCustomerUserAccessSite(Guid customerUserId, string permissionId, Guid siteId)
        {
            var canAccess = await CheckPermissionWithCache(customerUserId, permissionId, siteId: siteId);

            if (!canAccess)
            { 
                this.Throw<UnauthorizedAccessException>("User cannot access site", new { UserId = customerUserId,
                                                                                         SiteId = siteId,
                                                                                         PermissionId = permissionId });
            }
        }

        private async Task<bool> CheckPermissionWithCache(Guid customerUserId, string permissionId, Guid? customerId = null, Guid? portfolioId = null, Guid? siteId = null)
        {
            var canAccess = await _cache.GetOrCreateAsync(
                $"access_permission_{customerUserId}_{permissionId}_{customerId}_{portfolioId}_{siteId}",
                async(entry) =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    return await _directoryApi.CheckPermission(customerUserId, permissionId, customerId: customerId, portfolioId: portfolioId, siteId: siteId);
                }
            );
            return canAccess;
        }
    }
}
