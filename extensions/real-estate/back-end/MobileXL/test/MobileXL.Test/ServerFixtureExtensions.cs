using System.Net.Http;
using System;
using MobileXL.Models;
using Microsoft.AspNetCore.WebUtilities;

namespace Willow.Tests.Infrastructure
{
    public static class ServerFixtureExtensions
    {
        private static readonly string[] s_allPermissions = new string[]
        {
            Permissions.ManageUsers,
            Permissions.ViewUsers,
            Permissions.ManagePortfolios,
            Permissions.ViewPortfolios,
            Permissions.ManageSites,
            Permissions.ViewSites,
            Permissions.ManageConnectors,
            Permissions.ManageFloors,
            Permissions.UseTimeMachine,
            Permissions.ManageApps,
            Permissions.ViewApps
        };
        public static HttpClient CreateClientWithCustomerUserRole(this ServerFixture serverFixture, Guid? customerUserId)
        {
            customerUserId = customerUserId ?? Guid.NewGuid();
            return serverFixture.CreateClient(new string[] { UserTypeNames.CustomerUser }, customerUserId.Value);
        }

        public static HttpClient CreateClientWithCustomerUserPermissionOnSite(this ServerFixture serverFixture, Guid? customerUserId, string permissionId, Guid siteId)
        {
            return CreateClientWithPermission(serverFixture, customerUserId, permissionId, PermissionResourceType.Site, siteId);
        }

        private static HttpClient CreateClientWithPermission(ServerFixture serverFixture, Guid? customerUserId, string permissionId, PermissionResourceType resourceType, Guid resourceId)
        {
            customerUserId = customerUserId ?? Guid.NewGuid();
            SetupPermissionRequest(serverFixture.Arrange().GetDirectoryApi(), customerUserId.Value, permissionId, resourceType, resourceId, true);
            return serverFixture.CreateClient(new string[] { UserTypeNames.CustomerUser }, customerUserId);
        }

        public static HttpClient CreateClientWithCustomerUserDeniedPermissionOnSite(this ServerFixture serverFixture, Guid? customerUserId, string deniedPermissionId, Guid siteId)
        {
            return CreateClientWithAllPermissions(serverFixture, customerUserId, deniedPermissionId, PermissionResourceType.Site, siteId);
        }

        private static HttpClient CreateClientWithAllPermissions(ServerFixture serverFixture, Guid? customerUserId, string deniedPermissionId, PermissionResourceType resourceType, Guid resourceId)
        {
            customerUserId = customerUserId ?? Guid.NewGuid();
            foreach(var permissionId in s_allPermissions)
            {
                if (permissionId == deniedPermissionId)
                {
                    SetupPermissionRequest(serverFixture.Arrange().GetDirectoryApi(), customerUserId.Value, permissionId, resourceType, resourceId, false);
                }
                else
                {
                    SetupPermissionRequest(serverFixture.Arrange().GetDirectoryApi(), customerUserId.Value, permissionId, resourceType, resourceId, true);
                }
            }
            return serverFixture.CreateClient(new string[] { UserTypeNames.CustomerUser }, customerUserId);
        }

        private static void SetupPermissionRequest(DependencyServiceHttpHandler directoryApiHttpHandler, Guid customerUserId, string permissionId, PermissionResourceType resourceType, Guid resourceId, bool isAuthorized)
        {
            var url = $"users/{customerUserId}/permissions/{permissionId}/eligibility";
            switch(resourceType)
            {
                case PermissionResourceType.Customer:
                    url = QueryHelpers.AddQueryString(url, "customerId", resourceId.ToString());
                    break;
                case PermissionResourceType.Portfolio:
                    url = QueryHelpers.AddQueryString(url, "portfolioId", resourceId.ToString());
                    break;
                case PermissionResourceType.Site:
                    url = QueryHelpers.AddQueryString(url, "siteId", resourceId.ToString());
                    break;
                default:
                    throw new ArgumentException($"Unkown ResourceType: {resourceType}", nameof(resourceType));
            }
            directoryApiHttpHandler
                .SetupRequest(HttpMethod.Get, url)
                .ReturnsJson(new { IsAuthorized = isAuthorized });
        }
    }
}
