
using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Group Type Entity Configuration
/// </summary>
public class GroupTypeConfiguration : IEntityTypeConfiguration<GroupType>
{
    /// <summary>
    /// Configure Group Type Entity Configuration
    /// </summary>
    /// <param name="builder">Group Entity Builder instance</param>
    public void Configure(EntityTypeBuilder<GroupType> builder)
    {
        builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
