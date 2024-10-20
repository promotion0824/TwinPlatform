using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Directory.Models;

namespace Willow.Management
{
    public static class RoleAssignmentsExtensions
    {
        public static bool IsAdmin(this IEnumerable<RoleAssignment> assignments) =>
            assignments.Any(x =>
                x.ResourceType == RoleResourceType.Customer && 
                x.RoleId == WellKnownRoleIds.CustomerAdmin);

        public static bool IsCustomerAdmin(this IEnumerable<RoleAssignment> assignments, Guid customerId) =>
            assignments.Any(x =>
                x.ResourceType == RoleResourceType.Customer && x.ResourceId == customerId &&
                x.RoleId == WellKnownRoleIds.CustomerAdmin);

        public static bool IsPortfolioAdmin(this IEnumerable<RoleAssignment> assignments, Guid portfolioId) =>
            assignments.Any(x =>
                x.ResourceType == RoleResourceType.Portfolio && x.ResourceId == portfolioId &&
                x.RoleId == WellKnownRoleIds.PortfolioAdmin);
        
        public static bool IsPortfolioViewer(this IEnumerable<RoleAssignment> assignments, Guid portfolioId) =>
            assignments.Any(x =>
                x.ResourceType == RoleResourceType.Portfolio && x.ResourceId == portfolioId &&
                x.RoleId == WellKnownRoleIds.PortfolioViewer);
        
        public static bool IsSiteAdmin(this IEnumerable<RoleAssignment> assignments, Guid siteId) =>
            assignments.Any(x =>
                x.ResourceType == RoleResourceType.Site && x.ResourceId == siteId &&
                x.RoleId == WellKnownRoleIds.SiteAdmin);

        public static bool IsSiteViewer(this IEnumerable<RoleAssignment> assignments, Guid siteId) =>
            assignments.Any(x =>
                x.ResourceType == RoleResourceType.Site && x.ResourceId == siteId &&
                x.RoleId == WellKnownRoleIds.SiteViewer);
    }
}
