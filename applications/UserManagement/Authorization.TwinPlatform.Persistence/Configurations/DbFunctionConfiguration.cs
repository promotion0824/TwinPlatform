using Authorization.TwinPlatform.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Configuration Class for configuring Custom User Defined function on the Model Builder
/// </summary>
internal class DbFunctionConfiguration
{
	/// <summary>
	/// Method to configure DB functions
	/// </summary>
	/// <param name="modelBuilder"></param>
	/// <exception cref="NotImplementedException"></exception>
	public static void ApplyConfiguration(ModelBuilder modelBuilder)
	{
		var getUserWithGroupMethodInfo = typeof(TwinPlatformAuthContext).GetMethod(nameof(TwinPlatformAuthContext.GetUsersWithinGroup), new[] { typeof(string) });
		_ = getUserWithGroupMethodInfo != null ? modelBuilder.HasDbFunction(getUserWithGroupMethodInfo) : throw new NotImplementedException();

		var getRolesByUserAssignmentMethodInfo = typeof(TwinPlatformAuthContext).GetMethod(nameof(TwinPlatformAuthContext.GetRoleAssignmentsByUser), new[] { typeof(string) });
		_ = getRolesByUserAssignmentMethodInfo != null ? modelBuilder.HasDbFunction(getRolesByUserAssignmentMethodInfo) : throw new NotImplementedException();
	}
}

