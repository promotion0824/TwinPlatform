using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Class to Configure Group Entity Configuration
/// </summary>
internal class GroupConfiguration : IEntityTypeConfiguration<Group>
{
	/// <summary>
	/// Method to Configure Group Entity Configuration
	/// </summary>
	/// <param name="builder">Group Entity Builder instance</param>
	public void Configure(EntityTypeBuilder<Group> builder)
	{
		builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();
		builder.HasIndex(x => x.Name).IsUnique();
	}
}
