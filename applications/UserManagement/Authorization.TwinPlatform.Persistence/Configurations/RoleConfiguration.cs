using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Method to configure Role Entity Configuration
/// </summary>
internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
	/// <summary>
	/// Method to configure Role Entity Configuration
	/// </summary>
	/// <param name="builder">Role Entity Builder instance</param>
	public void Configure(EntityTypeBuilder<Role> builder)
	{
		builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();
		builder.HasIndex(x => new { x.Name }).IsUnique();
	}
}
