using PlatformPortalXL.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Management;
using Willow.Platform.Models;

namespace PlatformPortalXL.Extensions
{
    public static class ManagementExtensions
    {
        public static ManagedSiteDto MapToManagedSiteDto(this Site site, IEnumerable<RoleAssignmentDto> assignments, IImageUrlHelper helper)
        {
            var assignment = assignments.CheckAssignmentFor(site.Id);

             return assignment.Exists ? new ManagedSiteDto
            {
                SiteId = site.Id,
                SiteName = site.Name,
                Role = assignment.RoleName,
                LogoUrl = site.LogoId.HasValue ? helper.GetSiteLogoUrl(site.LogoPath, site.LogoId.Value) : null,
                LogoOriginalSizeUrl = site.LogoId.HasValue
                    ? helper.GetSiteLogoOriginalSizeUrl(site.LogoPath, site.LogoId.Value)
                    : null
            } : null;
        }

        public static List<ManagedSiteDto> MapToManagedSiteDto(this IEnumerable<Site> sites, IEnumerable<RoleAssignmentDto> assignments, IImageUrlHelper helper)
        {
            return sites?.Select(x => x.MapToManagedSiteDto(assignments, helper)).Where(x => x != null).ToList() ?? new List<ManagedSiteDto>();
        }

        public static ManagedPortfolioDto MapToManagedPortfolioDto(this Portfolio portfolio, IEnumerable<RoleAssignmentDto> assignments, IImageUrlHelper helper)
        {
            var assignment = assignments.CheckAssignmentFor(portfolio.Id);

            var sites = MapToManagedSiteDto(portfolio.Sites, assignments, helper);

            return (assignment.Exists || sites.Any()) ? new ManagedPortfolioDto
            {
                PortfolioId = portfolio.Id,
                PortfolioName = portfolio.Name,
                Role = assignment.RoleName,
                Features = PortfolioFeaturesDto.MapFrom(portfolio.Features),
                Sites = sites
            } : null;
        }

        public static List<ManagedPortfolioDto> MapToManagedPortfolioDto(this IEnumerable<Portfolio> portfolios, IEnumerable<RoleAssignmentDto> assignments, IImageUrlHelper helper)
        {
            return portfolios?.Select(x => x.MapToManagedPortfolioDto(assignments, helper)).Where(x => x != null).ToList() ?? new List<ManagedPortfolioDto>();
        }

        private const string AdminRoleName = "Admin";
        private const string ViewerRoleName = "Viewer";

        private static (bool Exists, string RoleName) CheckAssignmentFor(this IEnumerable<RoleAssignmentDto> assignments, Guid resourceId)
        {
            var roleName = "";

            if (assignments.IsPortfolioViewer(resourceId) || assignments.IsSiteViewer(resourceId))
            {
                roleName = ViewerRoleName;
            }
            else if (assignments.IsAdmin() || assignments.IsPortfolioAdmin(resourceId) || assignments.IsSiteAdmin(resourceId))
            {
                roleName = AdminRoleName;
            }

            return (!string.IsNullOrEmpty(roleName), roleName);
        }
    }
}
