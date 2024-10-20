using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Db;

public static class EntityBuilderExtensions
{
    public static void ConfigureBaseEntity<T>(this EntityTypeBuilder<T> builder)
        where T : BaseEntity
    {
        builder.Property(x => x.CreatedBy)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Name);
        builder.Property(x => x.UpdatedBy)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Name);
        builder.Property(x => x.CreatedOn)
               .IsRequired();
        builder.Property(x => x.UpdatedOn)
               .IsRequired();
    }
}
