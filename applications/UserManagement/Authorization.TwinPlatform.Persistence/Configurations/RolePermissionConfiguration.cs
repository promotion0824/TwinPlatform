using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Method to configure RolePermission Entity Configuration
/// </summary>
internal class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
	/// <summary>
	/// Method to configure RolePermission Entity Configuration
	/// </summary>
	/// <param name="builder">Role Permission Entity Builder instance</param>
	public void Configure(EntityTypeBuilder<RolePermission> builder)
	{
		builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();

		builder.HasKey(x => new { x.PermissionId, x.RoleId });

		builder.HasOne(x => x.Permission).WithMany(x => x.RolePermission);
		builder.HasOne(x => x.Role).WithMany(x => x.RolePermission);
	}
}
