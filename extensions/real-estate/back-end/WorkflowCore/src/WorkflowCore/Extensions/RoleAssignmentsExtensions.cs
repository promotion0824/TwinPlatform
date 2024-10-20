using System.Collections.Generic;
using System.Linq;
using Willow.Directory.Models;

namespace WorkflowCore.Extensions
{
    public static class RoleAssignmentsExtensions
    {
        public static bool IsCustomerAdmin(this IEnumerable<RoleAssignment> roleAssignments) =>
            roleAssignments.Any(ra => ra.ResourceType == RoleResourceType.Customer &&
                                      ra.RoleId == WellKnownRoleIds.CustomerAdmin);
    }
}
