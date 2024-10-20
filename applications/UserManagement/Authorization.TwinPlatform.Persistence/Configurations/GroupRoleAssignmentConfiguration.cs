using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Class to Configure GroupRoleAssignment Entity Configuration
/// </summary>
internal class GroupRoleAssignmentConfiguration : IEntityTypeConfiguration<GroupRoleAssignment>
{
	/// <summary>
	/// Method to configure GroupRoleAssignment Entity Configuration
	/// </summary>
	/// <param name="builder">Group Role Assignment Entity Builder instance</param>
	public void Configure(EntityTypeBuilder<GroupRoleAssignment> builder)
	{
		builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();
		builder.HasIndex(x => new { x.RoleId, x.GroupId, x.Expression }).IsUnique();
	}
}

