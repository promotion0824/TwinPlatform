using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Types;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Authorization.TwinPlatform.Services;

/// <summary>
/// Service for interacting with Microsoft Graph API and UM Authorization Database.
/// </summary>
public class AuthorizationGraphService(ILogger<AuthorizationGraphService> logger,
    TwinPlatformAuthContext authContext,
    IMapper mapper,
    IGraphApplicationService graphApplicationService,
    IEnumerable<IGraphApplicationClientService> graphApplicationClients,
    IWillowExpressionService expressionService) : IAuthorizationGraphService
{

    /// <summary>
    /// Get Users all UM permission based on AD security groups by mail address
    /// </summary>
    /// <param name="email">Mail address of the active directory users</param>
    /// <returns>Enumerable of conditional permission model.</returns>
    public async Task<IEnumerable<ConditionalPermissionModel>> GetAllPermissionByEmail(string email)
    {
        try
        {
            // Get all non Application type groups from DB 
            var allADGroupsWithAssignments = await authContext.Groups.Include(t => t.GroupType)
                                                .Where(w => w.GroupType.Name != GroupTypeNames.Application.ToString() && w.GroupRoleAssignments.Any())
                                                .GroupBy(g => g.GroupType.Name)
                                                .AsNoTracking()
                                                .ToDictionaryAsync(grp => grp.Key, v => v.ToList());

            ConcurrentBag<(string type, List<Persistence.Entities.Group> groups)> memberGroupsByType = new();
            async Task FindGroupsUserIsMemberOf(IGraphApplicationClientService graphClient)
            {
                // Get Entity groups; Type not found just return
                if (!allADGroupsWithAssignments!.TryGetValue(graphClient.GraphConfiguration.Type.ToString(), out var entityGroups))
                    return;

                var memberGroups = await graphApplicationService.FilterGroupsOfUserByEmailAsync(graphClient, entityGroups.Select(s => s.Name), email, true);
                if (memberGroups is not null)
                {
                    var memberEntityGroups = entityGroups.Where(w => memberGroups.Contains(w.Name)).ToList();
                    memberGroupsByType.Add((graphClient.GraphConfiguration.Type.ToString(), memberEntityGroups));
                }
            }

            // Filter to only the groups the user is memberOf for each client
            var findGroupsTask = graphApplicationClients.Select(client => FindGroupsUserIsMemberOf(client));
            await Task.WhenAll(findGroupsTask);

            var memberGroupIds = memberGroupsByType.SelectMany(m => m.groups).Select(s => s.Id).ToList();
            var memberGroupAssignments = await authContext.GroupRoleAssignments.Include(i => i.Role)
                                            .ThenInclude(r => r.RolePermission)
                                            .ThenInclude(rp => rp.Permission)
                                            .ThenInclude(i=>i.Application)
                                            .Where(w => memberGroupIds.Contains(w.GroupId))
                                            .ToListAsync();

            List<ConditionalPermissionModel> result = [];
            var willowExpressionEnv  = expressionService.GetUMDefaultEnvironment();
            foreach (var groupAssignment in memberGroupAssignments)
            {
                // If Condition expression is not empty and evaluate to false (inactive), skip to next iteration.
                if(!string.IsNullOrWhiteSpace(groupAssignment.Condition) && !expressionService.Evaluate<bool>(groupAssignment.Condition,willowExpressionEnv,out _))
                {
                    continue;
                }

                var conditionals = groupAssignment.Role.RolePermission.Select(s =>new ConditionalPermissionModel(mapper.Map<Persistence.Entities.Permission, PermissionModel>(s.Permission),
                                                                                                           groupAssignment.Expression ?? string.Empty,
                                                                                                           groupAssignment.Condition ?? string.Empty));
                result.AddRange(conditionals);
            }

            // Get the Distinct Permission + Resource + Condition
            result = result.DistinctBy(d => new { d.Permission.FullName, d.Expression, d.Condition }).ToList();
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while getting permission based on AD Groups.");
        }
        return Enumerable.Empty<ConditionalPermissionModel>();
    }
}
