using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Common;
using Willow.Data;
using Willow.Directory.Models;
using Willow.ExceptionHandling.Exceptions;
using Willow.Platform.Models;

namespace Willow.Management
{
    public interface IManagedUserRequestValidator
    {
        Task<IList<RoleAssignment>> Validate(ManagedUserRequest request, Guid customerId, IList<RoleAssignmentDto> currentUserAssignments, IList<RoleAssignmentDto> managedUserAssignments, bool allowNone);
    }

    public class ManagedUserRequestValidator : IManagedUserRequestValidator
    {
        private readonly IReadRepository<Guid, Site> _siteRepo;

        public ManagedUserRequestValidator(IReadRepository<Guid, Site> siteRepo)
        {
            _siteRepo = siteRepo ?? throw new ArgumentNullException(nameof(siteRepo));
        }

        #region IManagedUserRequestValidator

        public async Task<IList<RoleAssignment>> Validate(ManagedUserRequest request, Guid customerId, IList<RoleAssignmentDto> currentUserAssignments, IList<RoleAssignmentDto> managedUserAssignments, bool allowNone)
        {
            currentUserAssignments = currentUserAssignments ?? throw new ArgumentNullException(nameof(currentUserAssignments));

            if(currentUserAssignments.Empty())
                throw new UnauthorizedAccessException("Current user has no roles").WithData( new { CustomerId = customerId } );

            // Customer admins can do anything
            var customerAdmin = currentUserAssignments.IsCustomerAdmin(customerId);

            if(request.IsCustomerAdmin.Value)
            { 
                if(customerAdmin)
                    return new List<RoleAssignment> { new RoleAssignment { RoleId = WellKnownRoleIds.CustomerAdmin, ResourceType = RoleResourceType.Customer, ResourceId = customerId }  };

                // Request to create user as customer admin is denied
                throw new UnauthorizedAccessException("Only customer admins can create customer admins").WithData( new { CustomerId = customerId } );
            }

            var assignments = new List<RoleAssignment>();

            // Copy current assignments to output list
            if(managedUserAssignments != null)
            { 
                foreach(var assignment in managedUserAssignments)
                    assignments.Add(new RoleAssignment
                    { 
                        PrincipalId  = assignment.PrincipalId,
                        RoleId       = assignment.RoleId,
                        ResourceType = assignment.ResourceType,
                        ResourceId   = assignment.ResourceId
                    });
            }

            // Ensure we remove pre-existing admin role, if any
            assignments.Remove(RoleResourceType.Customer, customerId);

            // If no portfolios specified than no role assignments to create
            if(request.Portfolios.Empty())
                throw new ArgumentException("No portfolios or sites specified").WithData( new { CustomerId = customerId });

            foreach (var portfolio in request.Portfolios)
            {
                var isPortfolioAdmin = currentUserAssignments.IsPortfolioAdmin(portfolio.PortfolioId);
                var isPortfolioChange = assignments.IsChange(portfolio);

                if(isPortfolioChange)
                { 
                    if(!string.IsNullOrWhiteSpace(portfolio.Role))
                    {
                        if(portfolio.Role != "Viewer" && portfolio.Role != "Admin")
                            throw new ArgumentException("Invalid role for portfolio").WithData( new { CustomerId = customerId, PortfolioId = portfolio.PortfolioId, Role = portfolio.Role } );

                        if(!customerAdmin && !isPortfolioAdmin)
                            throw new UnauthorizedAccessException("Only customer and portfolio admins can create portfolio users").WithData( new { CustomerId = customerId, PortfolioId = portfolio.PortfolioId } );

                        assignments.AddOrReplace(new RoleAssignment { RoleId = MapRoleNameToRoleId(portfolio.Role, true), ResourceType = RoleResourceType.Portfolio, ResourceId = portfolio.PortfolioId }, portfolio.Role);
                    }
                    else if(customerAdmin || isPortfolioAdmin)
                        assignments.Remove(RoleResourceType.Portfolio, portfolio.PortfolioId);
                    else
                        throw new UnauthorizedAccessException("Only customer and portfolio admins can remove portfolio roles").WithData( new { CustomerId = customerId, PortfolioId = portfolio.PortfolioId } );
                }

                if(portfolio.Sites != null)
                { 
                    var userIsPortfolioAdmin = assignments.IsPortfolioAdmin(portfolio.PortfolioId);
                    var userIsPortfolioViewer = assignments.IsPortfolioViewer(portfolio.PortfolioId);

                    foreach(var newSiteRole in portfolio.Sites)
                    {
                        Site site = null;
                                
                        try
                        {   
                            site = await _siteRepo.Get<Site>(newSiteRole.SiteId);

                            if(site.CustomerId != customerId)
                                throw new ArgumentException("Site does not belong to this customer").WithData( new { CustomerId = customerId, PortfolioId = portfolio.PortfolioId, SiteId = newSiteRole.SiteId } );
                        }
                        catch(NotFoundException)
                        {
                            assignments.Remove(RoleResourceType.Site, newSiteRole.SiteId);
                            continue;
                        }

                        if(!string.IsNullOrWhiteSpace(newSiteRole.Role))
                        {
                            if(newSiteRole.Role != "Viewer" && newSiteRole.Role != "Admin")
                                throw new ArgumentException("Invalid role for site").WithData( new { CustomerId = customerId, SiteId = newSiteRole.SiteId, Role = portfolio.Role } );

                            if(!customerAdmin && !isPortfolioAdmin && !currentUserAssignments.IsSiteAdmin(newSiteRole.SiteId))
                                throw new UnauthorizedAccessException("Only customer, portfolio or site admins can create site users").WithData( new { CustomerId = customerId, SiteId = newSiteRole.SiteId } );

                            assignments.AddOrReplace(new RoleAssignment { RoleId = MapRoleNameToRoleId(newSiteRole.Role, false), ResourceType = RoleResourceType.Site, ResourceId = newSiteRole.SiteId }, newSiteRole.Role);
                        }
                        else
                            assignments.Remove(RoleResourceType.Site, newSiteRole.SiteId);
                    }
                }
            }

            if(!allowNone && assignments.Empty())
                throw new ArgumentException("No portfolio or site roles specified").WithData( new { CustomerId = customerId });

            return assignments;
        }

        #endregion 

        #region Private

        internal static Guid MapRoleNameToRoleId(string roleName, bool isPortfolio) =>
            (roleName, isPortfolio) switch
            {
                ("Admin", true)   => WellKnownRoleIds.PortfolioAdmin,
                ("Admin", false)  => WellKnownRoleIds.SiteAdmin,
                ("Viewer", true)  => WellKnownRoleIds.PortfolioViewer,
                ("Viewer", false) => WellKnownRoleIds.SiteViewer,
                                _ => throw new ArgumentException("Unknown role type")
            };

        #endregion
    }

    internal static class RoleAssignmentExtensions
    {
        internal static void AddOrReplace(this IList<RoleAssignment> assignments, RoleAssignment newAssignment, string newRole)
        {
            var result = assignments.Where( a=> a.ResourceType == newAssignment.ResourceType && a.ResourceId == newAssignment.ResourceId).FirstOrDefault();

            if(result != null)
                result.RoleId = ManagedUserRequestValidator.MapRoleNameToRoleId(newRole, newAssignment.ResourceType == RoleResourceType.Portfolio);
            else
                assignments.Add(newAssignment);
        }

        internal static void Remove(this IList<RoleAssignment> assignments, RoleResourceType type, Guid resourceId)
        {
            var result = assignments.Where( a=> a.ResourceType == type && a.ResourceId == resourceId).FirstOrDefault();

            if(result != null)
                assignments.Remove(result);
        }

        internal static void Remove(this IList<RoleAssignment> assignments, IList<ManagedSiteDto> sites)
        {
            foreach(var site in sites)
                assignments.Remove(RoleResourceType.Site, site.SiteId);
        }

        internal static bool IsChange(this IList<RoleAssignment> assignments, ManagedPortfolioDto portfolio)
        {
            var result = assignments.Where( a=> a.ResourceType == RoleResourceType.Portfolio && a.ResourceId == portfolio.PortfolioId).FirstOrDefault();

            if(string.IsNullOrWhiteSpace(portfolio.Role))
                return result != null; 

            if(result == null)
                return true;

            // Upgrading
            if(portfolio.Role == "Admin")
                return result.RoleId != WellKnownRoleIds.PortfolioAdmin;

            // Downgrading
            return result.RoleId != WellKnownRoleIds.PortfolioViewer;
        }        
    }
}
