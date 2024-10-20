using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Entities.Permission;
using DirectoryCore.Enums;
using Willow.Directory.Models;

namespace DirectoryCore.Dto
{
    public static class RoleAssignmentExtensions
    {
        public static RoleAssignment ToRoleAssignment(this AssignmentEntity assignment)
        {
            if (assignment == null)
            {
                return null;
            }

            var roleAssignmentDto = new RoleAssignment
            {
                PrincipalId = assignment.PrincipalId,
                RoleId = assignment.RoleId,
                ResourceId = assignment.ResourceId,
                ResourceType = assignment.ResourceType
            };
            return roleAssignmentDto;
        }

        public static RoleAssignmentDto ToRoleAssignment(this RoleAssignmentEntity assignment)
        {
            if (assignment == null)
                return null;

            var roleAssignmentDto = new RoleAssignmentDto
            {
                PrincipalId = assignment.PrincipalId,
                RoleId = assignment.RoleId,
                ResourceId = assignment.ResourceId,
                ResourceType = assignment.ResourceType,
                CustomerId = assignment.CustomerId,
                PortfolioId = assignment.PortfolioId
            };

            return roleAssignmentDto;
        }

        public static IList<RoleAssignment> ToRoleAssignments(
            this IEnumerable<AssignmentEntity> assignments
        )
        {
            return assignments.Select(s => s.ToRoleAssignment()).ToList();
        }

        public static IList<RoleAssignmentDto> ToRoleAssignments(
            this IEnumerable<RoleAssignmentEntity> assignments
        )
        {
            return assignments.Select(s => s.ToRoleAssignment()).ToList();
        }
    }
}
