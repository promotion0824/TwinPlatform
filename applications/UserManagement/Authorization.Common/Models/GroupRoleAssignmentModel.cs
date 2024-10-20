
using Authorization.Common.Abstracts;

namespace Authorization.Common.Models
{
	/// <summary>
	/// DTO Model class that map to GroupRoleAssignment Entity
	/// </summary>
	public class GroupRoleAssignmentModel: RoleAssignmentModel, IGroupRoleAssignment
	{
		/// <summary>
		/// Assigned Group
		/// </summary>
		public GroupModel Group { get; set; } = null!;

        /// <summary>
        /// Returns a short version of the assignment information.
        /// </summary>
        /// <returns>String.</returns>
        public string GetRowName()
        {
            return $"User: {Group.Name} | Role:{Role.Name} | Expression:{Expression} | Condition: {Condition}";
        }
    }
}
