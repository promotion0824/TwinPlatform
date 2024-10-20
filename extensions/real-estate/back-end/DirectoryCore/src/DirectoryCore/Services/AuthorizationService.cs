using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectoryCore.Domain;
using DirectoryCore.Dto;
using DirectoryCore.Entities;
using DirectoryCore.Entities.Permission;
using DirectoryCore.Enums;
using Microsoft.EntityFrameworkCore;
using Willow.Common;
using Willow.Database;
using Willow.Directory.Models;
using Willow.Infrastructure.Exceptions;

namespace DirectoryCore.Services
{
    public interface IAuthorizationService
    {
        Task<bool> CreateUserAssignment(
            Guid userId,
            Guid roleId,
            RoleResourceType resourceType,
            Guid resourceId
        );
        Task<bool> CreateUserAssignments(Guid userId, IList<RoleAssignment> roles);
        Task<AuthorizationInfo> CheckPermissionOnCustomer(
            Guid userId,
            string permissionId,
            Guid customerId
        );
        Task<AuthorizationInfo> CheckPermissionOnPortfolio(
            Guid userId,
            string permissionId,
            Guid portfolioId
        );
        Task<AuthorizationInfo> CheckPermissionOnSite(
            Guid userId,
            string permissionId,
            Guid siteId
        );
        Task<bool> UpdateUserAssignment(
            Guid userId,
            Guid roleId,
            RoleResourceType resourceType,
            Guid resourceId
        );
        Task DeleteAssignmentsByResource(RoleResourceType resourceType, Guid resourceId);
        Task<IList<RoleAssignmentDto>> GetRoleAssignments(
            Guid userId,
            Guid? customerId,
            Guid? portfolioId,
            Guid? siteId
        );
        Task DeleteUserAssignmentsByResource(Guid userId, Guid resourceId);
        Task<List<Site>> GetUserSites(Guid customerId, Guid userId, string permissionId);
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly DirectoryDbContext _dbContext;
        private readonly IDatabase _database;
        private readonly ISitesService _sitesService;

        public AuthorizationService(
            DirectoryDbContext directoryContext,
            IDatabase database,
            ISitesService sitesService
        )
        {
            _dbContext = directoryContext;
            _database = database;
            _sitesService = sitesService;
        }

        public async Task<bool> CreateUserAssignment(
            Guid userId,
            Guid roleId,
            RoleResourceType resourceType,
            Guid resourceId
        )
        {
            var exist = await _dbContext.Assignments.AnyAsync(
                x => x.PrincipalId == userId && x.RoleId == roleId && x.ResourceId == resourceId
            );
            if (exist)
            {
                return false;
            }

            var assignment = new AssignmentEntity
            {
                PrincipalId = userId,
                PrincipalType = PrincipalType.User,
                RoleId = roleId,
                ResourceId = resourceId,
                ResourceType = resourceType
            };
            _dbContext.Add(assignment);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Replaces all current role assignments with the given list
        /// </summary>
        /// <param name="userId">User whose roles will be updated</param>
        /// <param name="roles">List of new roles</param>
        public async Task<bool> CreateUserAssignments(Guid userId, IList<RoleAssignment> roles)
        {
            var entities = roles.Select(
                r =>
                    new AssignmentEntity
                    {
                        PrincipalId = userId,
                        PrincipalType = PrincipalType.User,
                        RoleId = r.RoleId,
                        ResourceId = r.ResourceId,
                        ResourceType = r.ResourceType
                    }
            );

            // Get the list of all existing assignments
            var currentAssignments = _dbContext
                .Assignments.Where(
                    a => a.PrincipalId == userId && a.PrincipalType == PrincipalType.User
                )
                .ToList();

            // ...and delete them
            if (!currentAssignments.Empty())
                _dbContext.Assignments.RemoveRange(currentAssignments.ToArray());

            // Add all the new ones
            foreach (var entity in entities)
                _dbContext.Add(entity);

            // Commit changes
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<AuthorizationInfo> CheckPermissionOnCustomer(
            Guid userId,
            string permissionId,
            Guid customerId
        )
        {
            return new AuthorizationInfo
            {
                IsAuthorized = await IsPermissionAssigned(userId, permissionId, customerId)
            };
        }

        public async Task<AuthorizationInfo> CheckPermissionOnPortfolio(
            Guid userId,
            string permissionId,
            Guid portfolioId
        )
        {
            var portfolio = await _dbContext.Portfolios.FirstOrDefaultAsync(
                p => p.Id == portfolioId
            );
            if (portfolio == null)
            {
                throw new ResourceNotFoundException("portfolio", portfolioId);
            }

            return new AuthorizationInfo
            {
                IsAuthorized = await IsPermissionAssigned(
                    userId,
                    permissionId,
                    portfolio.CustomerId,
                    portfolio.Id
                )
            };
        }

        public async Task<AuthorizationInfo> CheckPermissionOnSite(
            Guid userId,
            string permissionId,
            Guid siteId
        )
        {
            var site = await _sitesService.GetSite(siteId);
            if (site == null)
            {
                throw new ResourceNotFoundException("site", siteId);
            }
            return new AuthorizationInfo
            {
                IsAuthorized = await IsPermissionAssigned(
                    userId,
                    permissionId,
                    site.CustomerId,
                    site.PortfolioId.Value,
                    site.Id
                )
            };
        }

        private async Task<bool> IsPermissionAssigned(
            Guid userId,
            string permissionId,
            params Guid[] resourceIds
        )
        {
            var roleIds = _dbContext
                .RolePermission.Where(r => r.PermissionId == permissionId)
                .Select(r => r.RoleId);

            var result = await _dbContext.Assignments.AnyAsync(
                a =>
                    a.PrincipalId == userId
                    && roleIds.Contains(a.RoleId)
                    && resourceIds.Contains(a.ResourceId)
            );
            return result;
        }

        public async Task<bool> UpdateUserAssignment(
            Guid userId,
            Guid roleId,
            RoleResourceType resourceType,
            Guid resourceId
        )
        {
            var existsAssignment = await _dbContext.Assignments.FirstOrDefaultAsync(
                x =>
                    x.PrincipalId == userId
                    && x.ResourceType == resourceType
                    && x.ResourceId == resourceId
            );
            if (existsAssignment != null)
            {
                _dbContext.Remove(existsAssignment);
            }
            var assignment = new AssignmentEntity
            {
                PrincipalId = userId,
                PrincipalType = PrincipalType.User,
                RoleId = roleId,
                ResourceId = resourceId,
                ResourceType = resourceType
            };
            _dbContext.Assignments.Add(assignment);

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task DeleteAssignmentsByResource(
            RoleResourceType resourceType,
            Guid resourceId
        )
        {
            var assignments = await _dbContext
                .Assignments.AsTracking()
                .Where(x => x.ResourceType == resourceType && x.ResourceId == resourceId)
                .ToListAsync();
            _dbContext.Assignments.RemoveRange(assignments);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IList<RoleAssignmentDto>> GetRoleAssignments(
            Guid userId,
            Guid? customerId,
            Guid? portfolioId,
            Guid? siteId
        )
        {
            Guid[] resourceIds = null;

            if (customerId.HasValue)
            {
                resourceIds = new[] { customerId.Value };
            }
            else if (portfolioId.HasValue)
            {
                var portfolio = await _dbContext.Portfolios.FirstOrDefaultAsync(
                    p => p.Id == portfolioId
                );
                if (portfolio == null)
                {
                    throw new ResourceNotFoundException("portfolio", portfolioId.Value);
                }
                resourceIds = new[] { portfolio.CustomerId, portfolio.Id };
            }
            else if (siteId.HasValue)
            {
                var site = await _sitesService.GetSite(siteId.Value);
                if (site == null)
                {
                    throw new ResourceNotFoundException("site", siteId.Value);
                }
                resourceIds = new[] { site.CustomerId, site.PortfolioId.Value, site.Id };
            }

            var sql = $"SELECT * FROM dbo.vRoleAssignments WHERE PrincipalId = @PrincipalId";
            var sqlParam = new Dictionary<string, object> { { "@PrincipalId", userId } };
            if (resourceIds != null && resourceIds.Any())
            {
                sql += " AND ResourceId IN @ResourceIds";
                sqlParam.Add("ResourceIds", resourceIds);
            }

            var result = await _database.QueryList<RoleAssignmentEntity>(sql, sqlParam);
            var siteAssignments = result
                .Where(a => a.ResourceType == RoleResourceType.Site)
                .ToList();

            if (siteAssignments.Any())
            {
                var assignedSiteIds = siteAssignments.Select(a => a.ResourceId.ToString()).ToList();
                var sites = await _sitesService.GetSitesBySiteIds(assignedSiteIds);
                var invalidSiteId = new List<Guid>();

                foreach (var siteAssignment in siteAssignments)
                {
                    var site = sites.FirstOrDefault(s => s.Id == siteAssignment.ResourceId);
                    if (site != null)
                    {
                        siteAssignment.CustomerId = site.CustomerId;
                        siteAssignment.PortfolioId = site.PortfolioId.Value;
                    }
                    else
                    {
                        invalidSiteId.Add(siteAssignment.ResourceId);
                    }
                }

                // Remove all the assignments that their assigned Site Id does not exist in siteCore DB
                if (invalidSiteId.Any())
                {
                    var validSiteRoleAssignments = result.ToList();
                    validSiteRoleAssignments.RemoveAll(
                        c =>
                            c.ResourceType == RoleResourceType.Site
                            && invalidSiteId.Contains(c.ResourceId)
                    );

                    return validSiteRoleAssignments.ToRoleAssignments();
                }
            }

            return result.ToRoleAssignments();
        }

        public async Task DeleteUserAssignmentsByResource(Guid userId, Guid resourceId)
        {
            var assignments = await _dbContext
                .Assignments.AsTracking()
                .Where(x => x.PrincipalId == userId && x.ResourceId == resourceId)
                .ToListAsync();
            _dbContext.Assignments.RemoveRange(assignments);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Site>> GetUserSites(
            Guid customerId,
            Guid userId,
            string permissionId
        )
        {
            var roleIds = _dbContext
                .RolePermission.Where(r => r.PermissionId == permissionId)
                .Select(r => r.RoleId);

            var assignments = _dbContext.Assignments.Where(
                a => a.PrincipalId == userId && roleIds.Contains(a.RoleId)
            );

            var sites = await _sitesService.GetSitesByCustomer(customerId);
            sites = sites
                .Where(
                    s =>
                        assignments.Any(
                            a =>
                                a.ResourceId == s.CustomerId
                                || a.ResourceId == s.PortfolioId
                                || a.ResourceId == s.Id
                        )
                )
                .ToList();

            return sites;
        }
    }
}
