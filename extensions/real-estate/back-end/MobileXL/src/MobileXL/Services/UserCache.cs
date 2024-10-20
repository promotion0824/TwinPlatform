using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MobileXL.Models;
using MobileXL.Services.Apis.DirectoryApi;
using Willow.Api.Client;

namespace MobileXL.Services
{
    public interface IUserCache
    {
        Task<CustomerUser> GetCustomerUser(Guid customerId, Guid customerUserId);
        Task<CustomerUser> GetCustomerUser(Guid customerId, Guid customerUserId, bool returnNullIfNotFound);
    }

    public class UserCache : IUserCache
    {
        private readonly IMemoryCache _cache;
        private readonly IDirectoryApiService _directoryApi;

        public UserCache(IMemoryCache cache, IDirectoryApiService directoryApi)
        {
            _cache = cache;
            _directoryApi = directoryApi;
        }

        public async Task<CustomerUser> GetCustomerUser(Guid customerId, Guid customerUserId)
        {
            var customerUser = await _cache.GetOrCreateAsync(
                $"customerUser_{customerId}_{customerUserId}",
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    return await _directoryApi.GetCustomerUser(customerId, customerUserId);
                });
            return customerUser;
        }

        public async Task<CustomerUser> GetCustomerUser(Guid customerId, Guid customerUserId, bool returnNullIfNotFound)
        {
            var customerUser = await _cache.GetOrCreateAsync(
                $"customerUser_{customerId}_{customerUserId}_{returnNullIfNotFound}",
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    try
                    {
                        return await GetCustomerUser(customerId, customerUserId);
                    }
                    catch (RestException ex)
                    {
                        if (returnNullIfNotFound && ex.StatusCode == HttpStatusCode.NotFound)
                        {
                            return null;
                        }
                        throw;
                    }
                });
            return customerUser;
        }

    }
}
