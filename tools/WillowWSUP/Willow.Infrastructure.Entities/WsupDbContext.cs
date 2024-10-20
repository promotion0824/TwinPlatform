#nullable disable

namespace Willow.Infrastructure.Entities
{
    using Microsoft.EntityFrameworkCore;

    public class WsupDbContext : DbContext
    {
        public WsupDbContext(DbContextOptions<WsupDbContext> options)
                        : base(options)
        {
        }

        public virtual DbSet<Application> Applications { get; set; }

        public virtual DbSet<Building> Buildings { get; set; }

        public virtual DbSet<BuildingConnector> BuildingConnectors { get; set; }

        public virtual DbSet<BuildingConnectorStatus> BuildingConnectorStatuses { get; set; }

        public virtual DbSet<Connector> Connectors { get; set; }

        public virtual DbSet<ConnectorStatus> ConnectorStatuses { get; set; }

        public virtual DbSet<ConnectorType> ConnectorTypes { get; set; }

        public virtual DbSet<Customer> Customers { get; set; }

        public virtual DbSet<CustomerInstance> CustomerInstances { get; set; }

        public virtual DbSet<CustomerInstanceApplication> CustomerInstanceApplications { get; set; }

        public virtual DbSet<CustomerInstanceStatus> CustomerInstanceStatuses { get; set; }

        public virtual DbSet<CustomerStatus> CustomerStatuses { get; set; }

        public virtual DbSet<Environment> Environments { get; set; }

        public virtual DbSet<Region> Regions { get; set; }

        public virtual DbSet<Stamp> Stamps { get; set; }

        public virtual DbSet<Team> Teams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Application>(entity =>
            {
                entity.ToTable("Applications", "wsup");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.ApplicationStatus)
                    .WithMany(p => p.Applications)
                    .HasForeignKey(d => d.ApplicationStatusId)
                    .HasConstraintName("FK_Application_ApplicationStatuses");

                entity.HasOne(d => d.Team)
                    .WithMany(p => p.Applications)
                    .HasForeignKey(d => d.TeamId)
                    .HasConstraintName("FK_Application_Team");
            });

            modelBuilder.Entity<ApplicationStatus>(entity =>
            {
                entity.ToTable("ApplicationStatuses", "wsup");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasData(
                    new ApplicationStatus { Id = (int)ApplicationStatusEnum.Inactive, Name = "Inactive", Description = "Application is not active." },
                    new ApplicationStatus { Id = (int)ApplicationStatusEnum.Active, Name = "Active", Description = "Application is active." },
                    new ApplicationStatus { Id = (int)ApplicationStatusEnum.Preview, Name = "Preview", Description = "Application is in Preview mode." });
            });

            modelBuilder.Entity<Building>(entity =>
            {
                entity.ToTable("Buildings", "wsup");
                entity.HasKey(e => new { e.Id, e.CustomerInstanceId });

                entity.Property(e => e.Id).HasMaxLength(50);
                entity.Property(e => e.Name).HasMaxLength(100);

                entity.HasOne(d => d.CustomerInstance)
                    .WithMany(p => p.Buildings)
                    .HasForeignKey(d => d.CustomerInstanceId)
                    .HasConstraintName("FK_Building_CustomerInstance");
            });

            modelBuilder.Entity<BuildingConnector>(entity =>
            {
                entity.ToTable("BuildingConnectors", "wsup");

                entity.HasKey(e => new { e.CustomerInstanceId, e.BuildingId, e.ConnectorId });
                entity.Property(e => e.BuildingId).HasMaxLength(50);
                entity.Property(e => e.ConnectorId).HasMaxLength(50);

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.BuildingConnectors)
                    .HasForeignKey(d => new { d.BuildingId, d.CustomerInstanceId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BuildingConnector_Building");

                entity.HasOne(d => d.Connector)
                    .WithMany(p => p.BuildingConnectors)
                    .HasForeignKey(d => new { d.ConnectorId, d.CustomerInstanceId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BuildingConnector_Connector");

                entity.HasOne(d => d.BuildingConnectorStatus)
                    .WithMany(p => p.BuildingConnectors)
                    .HasForeignKey(d => d.BuildingConnectorStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BuildingConnector_BuildingConnectorStatuses");
            });

            modelBuilder.Entity<BuildingConnectorStatus>(entity =>
            {
                entity.ToTable("BuildingConnectorStatuses", "wsup");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasData(
                    new BuildingConnectorStatus { Id = (int)BuildingConnectorStatusEnum.Commissioning, Name = "Commissioning", Description = "Connector is in the process of being deployed to this building." },
                    new BuildingConnectorStatus { Id = (int)BuildingConnectorStatusEnum.Disabled, Name = "Disabled", Description = "Connector is now disabled for this building." },
                    new BuildingConnectorStatus { Id = (int)BuildingConnectorStatusEnum.Offline, Name = "Offline", Description = "Connector is temporarily offline for this building." },
                    new BuildingConnectorStatus { Id = (int)BuildingConnectorStatusEnum.Active, Name = "Active", Description = "Building Connector is active." });
            });

            modelBuilder.Entity<Connector>(entity =>
            {
                entity.ToTable("Connectors", "wsup");
                entity.HasKey(e => new { e.Id, e.CustomerInstanceId });

                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Id).HasMaxLength(50);
                entity.Property(e => e.ConnectorTypeId).HasMaxLength(50);

                entity.HasOne(d => d.ConnectorStatus)
                    .WithMany(p => p.Connectors)
                    .HasForeignKey(d => d.ConnectorStatusId)
                    .HasConstraintName("FK_Connector_ConnectorStatuses");

                entity.HasOne(d => d.ConnectorType)
                    .WithMany(p => p.Connectors)
                    .HasForeignKey(d => d.ConnectorTypeId)
                    .HasConstraintName("FK_Connector_ConnectorTypes");

                entity.HasOne(d => d.CustomerInstance)
                    .WithMany(p => p.Connectors)
                    .HasForeignKey(d => d.CustomerInstanceId)
                    .HasConstraintName("FK_Connector_CustomerInstance");
            });

            modelBuilder.Entity<ConnectorType>(entity =>
            {
                entity.ToTable("ConnectorTypes", "wsup");
                entity.HasKey(e => new { e.Id });
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Id).HasMaxLength(50);
            });

            modelBuilder.Entity<ConnectorStatus>(entity =>
            {
                entity.ToTable("ConnectorStatuses", "wsup");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasData(
                    new ConnectorStatus { Id = (int)ConnectorStatusEnum.InDevelopment, Name = "In Development", Description = "Connector is in development." },
                    new ConnectorStatus { Id = (int)ConnectorStatusEnum.Inactive, Name = "Inactive", Description = "Connector is no longer active." },
                    new ConnectorStatus { Id = (int)ConnectorStatusEnum.Active, Name = "Active", Description = "Connector is active." });
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customers", "wsup");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.CustomerStatus)
                    .WithMany(p => p.Customers)
                    .HasForeignKey(d => d.CustomerStatusId)
                    .HasConstraintName("FK_Customer_CustomerStatuses");
            });

            modelBuilder.Entity<CustomerInstance>(entity =>
            {
                entity.ToTable("CustomerInstances", "wsup");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerInstances)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_CustomerInstance_Customer");

                entity.HasOne(d => d.CustomerInstanceStatus)
                    .WithMany(p => p.CustomerInstances)
                    .HasForeignKey(d => d.CustomerInstanceStatusId)
                    .HasConstraintName("FK_CustomerInstance_CustomerInstanceStatuses");

                entity.HasOne(d => d.Stamp)
                    .WithMany(p => p.CustomerInstances)
                    .HasForeignKey(d => d.StampId)
                    .HasConstraintName("FK_CustomerInstance_Stamp");
            });

            modelBuilder.Entity<CustomerInstanceApplication>(entity =>
            {
                entity.ToTable("CustomerInstanceApplications", "wsup");

                entity.HasKey(e => new { e.CustomerInstanceId, e.ApplicationId });

                entity.HasOne(d => d.CustomerInstance)
                    .WithMany(p => p.CustomerInstanceApplications)
                    .HasForeignKey(d => d.CustomerInstanceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CustomerInstanceApplication_CustomerInstances");

                entity.HasOne(d => d.Application)
                    .WithMany(p => p.CustomerInstanceApplications)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CustomerInstanceApplication_Applications");

                entity.HasOne(d => d.CustomerInstanceApplicationStatus)
                    .WithMany(p => p.CustomerInstanceApplications)
                    .HasForeignKey(d => d.CustomerInstanceApplicationStatusId)
                    .HasConstraintName("FK_CustomerInstanceApplication_CustomerInstanceApplicationStatuses");
            });

            modelBuilder.Entity<CustomerInstanceApplicationStatus>(entity =>
            {
                entity.ToTable("CustomerInstanceApplicationStatuses", "wsup");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasData(
                    new CustomerInstanceApplicationStatus { Id = (int)CustomerInstanceApplicationStatusEnum.Inactive, Name = "Inactive", Description = "Application is not active." },
                    new CustomerInstanceApplicationStatus { Id = (int)CustomerInstanceApplicationStatusEnum.Active, Name = "Active", Description = "Application is active." });
            });

            modelBuilder.Entity<CustomerInstanceStatus>(entity =>
            {
                entity.ToTable("CustomerInstanceStatuses", "wsup");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasData(
                    new CustomerInstanceStatus { Id = (int)CustomerInstanceStatusEnum.Commissioning, Name = "Commissioning", Description = "Customer instance is not yet active." },
                    new CustomerInstanceStatus { Id = (int)CustomerInstanceStatusEnum.Active, Name = "Active", Description = "Customer instance is active." },
                    new CustomerInstanceStatus { Id = (int)CustomerInstanceStatusEnum.Decommissioned, Name = "Decommissioned", Description = "Customer instance is no longer active." });
            });

            modelBuilder.Entity<CustomerStatus>(entity =>
            {
                entity.ToTable("CustomerStatuses", "wsup");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasData(
                    new CustomerStatus { Id = (int)CustomerStatusEnum.Inactive, Name = "Inactive", Description = "Customer is not active." },
                    new CustomerStatus { Id = (int)CustomerStatusEnum.Active, Name = "Active", Description = "Customer is active." });
            });

            modelBuilder.Entity<Environment>(entity =>
            {
                entity.ToTable("Environments", "wsup");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasKey(e => e.Name);

                entity.HasData(
                    new Environment { Name = "dev", Description = "Development and Test Environment." },
                    new Environment { Name = "prd", Description = "Production Environment." },
                    new Environment { Name = "sbx", Description = "Sandbox environment for developer testing." });
            });

            modelBuilder.Entity<Region>(entity =>
            {
                entity.ToTable("Regions", "wsup");

                entity.HasKey(e => e.ShortName);

                entity.Property(e => e.ShortName).HasMaxLength(10);

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasData(
                    new Region { Name = "australiaeast", ShortName = "aue", DisplayName = "Australia East" },
                    new Region { Name = "eastus", ShortName = "eus", DisplayName = "US East" },
                    new Region { Name = "eastus2", ShortName = "eus2", DisplayName = "US East 2" },
                    new Region { Name = "westeurope", ShortName = "weu", DisplayName = "West Europe" },
                    new Region { Name = "westus", ShortName = "wus", DisplayName = "West US" },
                    new Region { Name = "westus2", ShortName = "wus2", DisplayName = "West US 2" });
            });

            modelBuilder.Entity<Stamp>(entity =>
            {
                entity.ToTable("Stamps", "wsup");

                entity.HasIndex(e => new { e.EnvironmentName, e.RegionShortName, e.Name }).IsUnique();

                entity.Property(e => e.Name).HasMaxLength(10);
            });

            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("Teams", "wsup");

                entity.Property(e => e.Name).HasMaxLength(30);

                entity.HasData(
                    new Team { Id = (int)TeamEnum.CloudOps, Name = "CloudOps", Description = "Cloud Operations" },
                    new Team { Id = (int)TeamEnum.ActivateTechnology, Name = "Activate Technology", Description = "Activate Technology Team" },
                    new Team { Id = (int)TeamEnum.AdvancedAnalytics, Name = "Advanced Analytics", Description = "Advanced Analytics" },
                    new Team { Id = (int)TeamEnum.Connectors, Name = "Connectors", Description = "Connectors" },
                    new Team { Id = (int)TeamEnum.CoreServices, Name = "CoreServices", Description = "Core Services" },
                    new Team { Id = (int)TeamEnum.Dashboards, Name = "Dashboards", Description = "Dashboards" },
                    new Team { Id = (int)TeamEnum.InvestaExperience, Name = "InvestaExperience", Description = "Investa Experience" },
                    new Team { Id = (int)TeamEnum.IoTServices, Name = "IoTServices", Description = "IoT Services" },
                    new Team { Id = (int)TeamEnum.SearchAndExplore, Name = "SearchAndExplore", Description = "Search and Explore" },
                    new Team { Id = (int)TeamEnum.Security, Name = "Security", Description = "Security and Privacy" },
                    new Team { Id = (int)TeamEnum.Workflows, Name = "Workflows", Description = "Workflows" },
                    new Team { Id = (int)TeamEnum.Unknown, Name = "Unknown", Description = "Team is Unknown." }
                    );
            });
        }
    }
}
