using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AdminPortalXL.Features.Directory;
using AdminPortalXL.Models.Directory;
using Willow.Infrastructure.MultiRegion;

namespace AdminPortalXL.Services
{
    public interface IRegionalDirectoryApi
    {
        Task<string> GetCustomerRegionId(Guid customerId);
        Task<Customer> GetCustomer(Guid customerId);
        Task<List<Customer>> GetCustomers();
        Task<Customer> CreateCustomer(CreateCustomerRequest request);
        Task<Customer> UpdateCustomer(Guid customerId, UpdateCustomerRequest request);
        Task<Customer> UpdateCustomerLogo(string regionId, Guid customerId, byte[] logoFileContent);
        Task<ImpersonateInfo> Impersonate(string regionId, Guid customerId);
    }

    public class RegionalDirectoryApi : IRegionalDirectoryApi
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMultiRegionSettings _multiRegionSettings;

        public RegionalDirectoryApi(IHttpClientFactory httpClientFactory, IMultiRegionSettings multiRegionSettings)
        {
            _httpClientFactory = httpClientFactory;
            _multiRegionSettings = multiRegionSettings;
        }

        public async Task<Customer> CreateCustomer(CreateCustomerRequest request)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.RegionalDirectoryCore, request.RegionId))
            {
                var response = await client.PostAsJsonAsync("customers", new
                {
                    name = request.CustomerName,
                    country = request.Country,
                    accountExternalId = request.AccountExternalId,
                    sigmaConnectionId = request.SigmaConnectionId
                });
                response.EnsureSuccessStatusCode(ApiServiceNames.RegionalDirectoryCore, request.RegionId);
                var newCustomer = await response.Content.ReadAsAsync<Customer>();
                return newCustomer;
            }
        }

        public async Task<Customer> UpdateCustomer(Guid customerId, UpdateCustomerRequest request)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.RegionalDirectoryCore, request.RegionId))
            {
                var response = await client.PutAsJsonAsync($"customers/{customerId}", new
                {
                    sigmaConnectionId = request.SigmaConnectionId
                });
                response.EnsureSuccessStatusCode(ApiServiceNames.RegionalDirectoryCore, request.RegionId);
                var newCustomer = await response.Content.ReadAsAsync<Customer>();
                return newCustomer;
            }
        }

        public async Task<string> GetCustomerRegionId(Guid customerId)
        {
            var customer = await GetCustomer(customerId);
            return customer?.RegionId;
        }

        public async Task<Customer> GetCustomer(Guid customerId)
        {
            var customers = await GetCustomers();
            var customer = customers.FirstOrDefault(c => c.Id == customerId);
            return customer;
        }

        public async Task<List<Customer>> GetCustomers()
        {
            var tasks = _multiRegionSettings.RegionIds.Select(regionId => GetCustomersForOneRegion(regionId));
            var results = await Task.WhenAll(tasks);
            var customers = results.SelectMany(r => r);
            return customers.ToList();
        }

        public async Task<ImpersonateInfo> Impersonate(string regionId, Guid customerId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.RegionalDirectoryCore, regionId))
            {
                var response = await client.PostAsync($"customers/{customerId}/impersonate", null);
                response.EnsureSuccessStatusCode(ApiServiceNames.RegionalDirectoryCore, regionId);
                return await response.Content.ReadAsAsync<ImpersonateInfo>();
            }
        }

        public async Task<Customer> UpdateCustomerLogo(string regionId, Guid customerId, byte[] logoFileContent)
        {
            var dataContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(logoFileContent)
            {
                Headers = { ContentLength = logoFileContent.Length }
            };
            dataContent.Add(fileContent, "logoImage", "logoFile");

            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.RegionalDirectoryCore, regionId))
            {
                var response = await client.PutAsync($"customers/{customerId}/logo", dataContent);
                response.EnsureSuccessStatusCode(ApiServiceNames.RegionalDirectoryCore, regionId);
                var customer = await response.Content.ReadAsAsync<Customer>();
                return customer;
            }
        }

        private async Task<List<Customer>> GetCustomersForOneRegion(string regionId)
        {
            using (var client = _httpClientFactory.CreateClient(ApiServiceNames.RegionalDirectoryCore, regionId))
            {
                var response = await client.GetAsync("customers?active=true");
                response.EnsureSuccessStatusCode(ApiServiceNames.RegionalDirectoryCore, regionId);
                var customersInRegion = await response.Content.ReadAsAsync<List<Customer>>();
                customersInRegion.ForEach(c => c.RegionId = regionId);
                return customersInRegion;
            }
        }
    }
}
