using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Class to configure RoleAssignment Entity Configuration
/// </summary>
internal class RoleAssignmentConfiguration : IEntityTypeConfiguration<RoleAssignment>
{
	/// <summary>
	/// Method to configure RoleAssignment Entity Configuration
	/// </summary>
	/// <param name="builder">RoleAssignment Entity Builder instance</param>
	public void Configure(EntityTypeBuilder<RoleAssignment> builder)
	{
		builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();
		builder.HasIndex(x => new { x.RoleId, x.UserId, x.Expression }).IsUnique();
	}
}
