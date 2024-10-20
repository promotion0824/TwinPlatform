using Authorization.TwinPlatform.Persistence.Entities;
using Authorization.TwinPlatform.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Configurations;

/// <summary>
/// Client Assignment Permission Table Configuration.
/// </summary>
internal class ClientAssignmentPermissionConfiguration : IEntityTypeConfiguration<ClientAssignmentPermission>
{
    /// <summary>
    /// Configure Client Assignment Permission Entity Configuration
    /// </summary>
    /// <param name="builder">Entity Builder instance</param>
    public void Configure(EntityTypeBuilder<ClientAssignmentPermission> builder)
    {
        builder.Property<Guid>(x => x.Id).UseNewSeqIdasDefault();
        builder.HasIndex(x => new { x.PermissionId, x.ClientAssignmentId }).IsUnique();
        builder.HasOne(p => p.Permission);
    }
}
