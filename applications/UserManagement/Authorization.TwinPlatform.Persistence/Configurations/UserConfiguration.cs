using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Method to configure User Entity Configuration
/// </summary>
internal class UserConfiguration : IEntityTypeConfiguration<User>
{
	/// <summary>
	/// Method to configure User Entity Configuration
	/// </summary>
	/// <param name="builder">User Entity Builder instance</param>
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();
		builder.HasIndex(i=>i.Email).IsUnique();
	}
}
