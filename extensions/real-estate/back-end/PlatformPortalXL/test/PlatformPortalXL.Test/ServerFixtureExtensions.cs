using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AutoFixture;
using Microsoft.AspNetCore.WebUtilities;
using PlatformPortalXL.Models;
using PlatformPortalXL.Test.Infrastructure;
using Willow.Common;
using Willow.Directory.Models;

namespace Willow.Tests.Infrastructure
{
    public static class ServerFixtureExtensions
    {
        private static readonly string[] s_allPermissions = new []
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

        public static HttpClient CreateClientWithPermissionOnCustomerAndSite(this ServerFixture serverFixture, Guid? userId, string customerPermissionId, string sitePermissionId, Guid customerId, Guid siteId)
        {
            userId = userId ?? Guid.NewGuid();
            SetupPermissionRequest(serverFixture.Arrange().GetDirectoryApi(), userId.Value, customerPermissionId, RoleResourceType.Customer, customerId, true);
            SetupPermissionRequest(serverFixture.Arrange().GetDirectoryApi(), userId.Value, sitePermissionId, RoleResourceType.Site, siteId, true);
            return serverFixture.CreateClient(null, userId);
        }

        public static HttpClient CreateClientWithPermissionOnCustomer(this ServerFixture serverFixture, Guid? userId, string permissionId, Guid customerId)
        {
            return CreateClientWithPermission(serverFixture, userId, permissionId, RoleResourceType.Customer, customerId);
        }

        public static HttpClient CreateClientWithPermissionOnPortfolio(this ServerFixture serverFixture, Guid? userId, string permissionId, Guid portfolioId)
        {
            return CreateClientWithPermission(serverFixture, userId, permissionId, RoleResourceType.Portfolio, portfolioId);
        }

        public static HttpClient CreateClientWithPermissionOnSite(this ServerFixture serverFixture, Guid? userId, string permissionId, Guid siteId)
        {
            return CreateClientWithPermission(serverFixture, userId, permissionId, RoleResourceType.Site, siteId);
        }

        public static HttpClient CreateClientWithPermissionsOnSite(this ServerFixture serverFixture, Guid? userId, string[] permissionIds, Guid siteId)
        {
            userId = userId ?? Guid.NewGuid();
            foreach (var permissionId in permissionIds)
            {
                SetupPermissionRequest(serverFixture.Arrange().GetDirectoryApi(), userId.Value, permissionId, RoleResourceType.Site, siteId, true);
            }
            return serverFixture.CreateClient(null, userId);
        }

        private static HttpClient CreateClientWithPermission(ServerFixture serverFixture, Guid? userId, string permissionId, RoleResourceType resourceType, Guid resourceId)
        {
            userId = userId ?? Guid.NewGuid();
            SetupPermissionRequest(serverFixture.Arrange().GetDirectoryApi(), userId.Value, permissionId, resourceType, resourceId, true);
            return serverFixture.CreateClient(null, userId);
        }

        public static HttpClient CreateClientWithDeniedPermissionOnCustomer(this ServerFixture serverFixture, Guid? userId, string deniedPermissionId, Guid customerId)
        {
            return CreateClientWithAllPermissions(serverFixture, userId, deniedPermissionId, RoleResourceType.Customer, customerId);
        }

        public static HttpClient CreateClientWithDeniedPermissionOnPortfolio(this ServerFixture serverFixture, Guid? userId, string deniedPermissionId, Guid portfolioId)
        {
            return CreateClientWithAllPermissions(serverFixture, userId, deniedPermissionId, RoleResourceType.Portfolio, portfolioId);
        }

        public static HttpClient CreateClientWithDeniedPermissionOnSite(this ServerFixture serverFixture, Guid? userId, string deniedPermissionId, Guid siteId)
        {
            return CreateClientWithAllPermissions(serverFixture, userId, deniedPermissionId, RoleResourceType.Site, siteId);
        }


        private static HttpClient CreateClientWithAllPermissions(ServerFixture serverFixture, Guid? userId, string deniedPermissionId, RoleResourceType resourceType, Guid resourceId)
        {
            userId = userId ?? Guid.NewGuid();
            foreach(var permissionId in s_allPermissions)
            {
                if (permissionId == deniedPermissionId)
                {
                    SetupPermissionRequest(serverFixture.Arrange().GetDirectoryApi(), userId.Value, permissionId, resourceType, resourceId, false);
                }
                else
                {
                    SetupPermissionRequest(serverFixture.Arrange().GetDirectoryApi(), userId.Value, permissionId, resourceType, resourceId, true);
                }
            }
            return serverFixture.CreateClient(null, userId);
        }

        private static void SetupPermissionRequest(DependencyServiceHttpHandler directoryApiHttpHandler, Guid userId, string permissionId, RoleResourceType resourceType, Guid resourceId, bool isAuthorized)
        {
            var fixture = new Fixture();
            fixture.Customizations.Add(new RandomDateOnlySequenceGenerator());

            var url = $"users/{userId}/permissions/{permissionId}/eligibility";
            switch (resourceType)
            {
                case RoleResourceType.Customer:
                    url = QueryHelpers.AddQueryString(url, "customerId", resourceId.ToString());
                    break;
                case RoleResourceType.Portfolio:
                    url = QueryHelpers.AddQueryString(url, "portfolioId", resourceId.ToString());
                    break;
                case RoleResourceType.Site:
                    url = QueryHelpers.AddQueryString(url, "siteId", resourceId.ToString());
                    break;
                default:
                    throw new ArgumentException("Invalid resource type", nameof(resourceType)).WithData(new { resourceType });
            }

            directoryApiHttpHandler
                .SetupRequest(HttpMethod.Get, url)
                .ReturnsJson(new { IsAuthorized = isAuthorized });

            directoryApiHttpHandler.SetupRequest(HttpMethod.Get, $"users/{userId}/sites?permissionId={permissionId}")
                .ReturnsJson(isAuthorized
                ? fixture.Build<Platform.Models.Site>().With(x => x.Id, resourceId).CreateMany(1).ToList()
                : []);
        }
    }
}
