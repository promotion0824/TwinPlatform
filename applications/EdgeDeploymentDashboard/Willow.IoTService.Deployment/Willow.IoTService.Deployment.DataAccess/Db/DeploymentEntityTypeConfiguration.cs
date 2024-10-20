using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.IoTService.Deployment.DataAccess.Entities;

namespace Willow.IoTService.Deployment.DataAccess.Db;

public class DeploymentEntityTypeConfiguration : IEntityTypeConfiguration<DeploymentEntity>
{
    public void Configure(EntityTypeBuilder<DeploymentEntity> builder)
    {
        builder.Property(x => x.Id)
               .IsRequired();
        builder.Property(x => x.Name)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Name);
        builder.Property(x => x.Status)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Short);
        builder.Property(x => x.StatusMessage)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Description);
        builder.Property(x => x.DateTimeApplied)
               .IsRequired();
        builder.Property(x => x.Version)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Short);
        builder.Property(x => x.AssignedBy)
               .IsRequired()
               .HasMaxLength(VarCharPropertyLengths.Name);
        builder.ConfigureBaseEntity();

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.DateTimeApplied);
    }
}
