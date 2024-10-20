
using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Class to configure Permission Entity Configuration
/// </summary>
internal class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
	/// <summary>
	/// Method to configure Permission Entity Configuration
	/// </summary>
	/// <param name="builder">Permission Entity Builder instance</param>
	public void Configure(EntityTypeBuilder<Permission> builder)
	{
		builder.HasIndex(x => new { x.Name, x.ApplicationId }).IsUnique();
		builder.HasIndex(x => x.ApplicationId);
		builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();
	}
}
