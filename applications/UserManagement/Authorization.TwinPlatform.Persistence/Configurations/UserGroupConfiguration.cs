using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

public class UserGroupConfiguration : IEntityTypeConfiguration<UserGroup>
{
    /// <summary>
    /// Method to configure UserGroup Entity Configuration
    /// </summary>
    /// <param name="builder">UserGroup Entity Builder instance</param>
    public void Configure(EntityTypeBuilder<UserGroup> builder)
	{
		builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();

		builder.HasKey(x => new { x.GroupId, x.UserId });

		builder.HasOne(o => o.Group).WithMany(m => m.UserGroups);
		builder.HasOne(o => o.User).WithMany(m => m.UserGroups);
	}
}

