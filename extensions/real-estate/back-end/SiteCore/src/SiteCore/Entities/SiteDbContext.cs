using Microsoft.EntityFrameworkCore;
using SiteCore.Enums;

namespace SiteCore.Entities
{
    public class SiteDbContext : DbContext
    {
        public SiteDbContext(DbContextOptions<SiteDbContext> options)
            : base(options)
        {
            this.UseManagedIdentity();
        }

        public virtual DbSet<FloorEntity> Floors { get; set; }
        public virtual DbSet<LayerEntity> Layers { get; set; }
        public virtual DbSet<SiteEntity> Sites { get; set; }
        public virtual DbSet<LayerGroupEntity> LayerGroups { get; set; }
        public virtual DbSet<ZoneEntity> Zones { get; set; }
        public virtual DbSet<LayerEquipmentEntity> LayerEquipments { get; set; }
        public virtual DbSet<ModuleEntity> Modules { get; set; }
        public virtual DbSet<ModuleTypeEntity> ModuleTypes { get; set; }
        public virtual DbSet<ModuleGroupEntity> ModuleGroups { get; set; }
        public virtual DbSet<WidgetEntity> Widgets { get; set; }
        public virtual DbSet<ScopeWidgetEntity> ScopeWidgets { get; set; }
        public virtual DbSet<SiteWidgetEntity> SiteWidgets { get; set; }
        public virtual DbSet<PortfolioWidgetEntity> PortfolioWidgets { get; set; }
        public virtual DbSet<SitePreferencesEntity> SitePreferences { get; set; }
        public virtual DbSet<MetricEntity> Metrics { get; set; }
        public virtual DbSet<SiteMetricValueEntity> SiteMetricValues { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingFloorEntity(modelBuilder);

            OnModelCreatingLayerEntity(modelBuilder);

            OnModelCreatingSiteEntity(modelBuilder);

            OnModelCreatingLayerGroupEntity(modelBuilder);

            OnModelCreatingZoneEntity(modelBuilder);

            OnModelCreatingLayerEquipmentEntity(modelBuilder);

            OnModelCreatingSiteWidgetEntity(modelBuilder);

            OnModelCreatingPortfolioWidgetEntity(modelBuilder);

            OnModelCreatingWidgetEntity(modelBuilder);

            OnModelCreatingMetricEntity(modelBuilder);

            OnModelCreatingSiteMetricValueEntity(modelBuilder);

            OnModelCreatingSitePreferencesEntity(modelBuilder);
        }

        private static void OnModelCreatingSiteMetricValueEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SiteMetricValueEntity>(entity => {
                entity.Property(e => e.Id).IsRequired().ValueGeneratedNever();

                entity.Property(e => e.SiteId).IsRequired();
                entity.Property(e => e.TimeStamp).IsRequired();
                entity.Property(e => e.MetricId).IsRequired();
                entity.Property(e => e.Value).HasColumnType("DECIMAL(15,6)");

                entity.HasIndex("SiteId", "MetricId", "TimeStamp")
                    .IsUnique()
                    .HasDatabaseName("UX_SiteId_MetricId_TimeStamp");

                entity.HasIndex("TimeStamp")
                    .HasDatabaseName("IX_TimeStamp");

                entity.HasIndex("SiteId", "TimeStamp")
                    .HasDatabaseName("IX_SiteId_TimeStamp");
            });
        }

        private static void OnModelCreatingMetricEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MetricEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Key).IsRequired().HasMaxLength(64);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(128);
                entity.Property(e => e.FormatString).IsRequired().HasMaxLength(64);
                entity.Property(e => e.ErrorLimit).HasColumnType("DECIMAL(15,6)");
                entity.Property(e => e.WarningLimit).HasColumnType("DECIMAL(15,6)");

                entity.HasIndex("ParentId", "Key")
                    .IsUnique()
                    .HasDatabaseName("UX_ParentId_Key");
            });
        }

        private static void OnModelCreatingWidgetEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WidgetEntity>(entity =>
            {
                entity.HasMany(r => r.SiteWidgets)
                    .WithOne(sr => sr.Widget);
            });
        }

        private static void OnModelCreatingScopeWidgetEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ScopeWidgetEntity>(entity =>
            {
                entity.HasOne(sr => sr.Widget)
                    .WithMany(r => r.ScopeWidgets)
                    .HasForeignKey(sr => sr.WidgetId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ScopeWidgets_Widgets");
            });
        }

        private static void OnModelCreatingSiteWidgetEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SiteWidgetEntity>(entity =>
            {
                entity.HasKey(sw => new { sw.SiteId, sw.WidgetId });

                entity.HasOne(sr => sr.Site)
                    .WithMany()
                    .HasForeignKey(sr => sr.SiteId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_SiteWidgets_Sites");

                entity.HasOne(sr => sr.Widget)
                    .WithMany(r => r.SiteWidgets)
                    .HasForeignKey(sr => sr.WidgetId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_SiteWidgets_Widgets");
            });
        }

        private static void OnModelCreatingPortfolioWidgetEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PortfolioWidgetEntity>(entity =>
            {
                entity.HasKey(sw => new { sw.PortfolioId, sw.WidgetId });

                entity.HasOne(sr => sr.Widget)
                    .WithMany(r => r.PortfolioWidgets)
                    .HasForeignKey(sr => sr.WidgetId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_PortfolioWidgets_Widgets");
            });
        }

        private static void OnModelCreatingLayerEquipmentEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LayerEquipmentEntity>(entity =>
            {
                entity.HasKey(e => new { e.LayerGroupId, e.EquipmentId });

                entity.HasOne(le => le.LayerGroup)
                    .WithMany(lg => lg.LayerEquipments)
                    .HasForeignKey(le => le.LayerGroupId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LayerEquipment_LayerGroups");

                entity.HasOne(le => le.Zone)
                    .WithMany(z => z.LayerEquipments)
                    .HasForeignKey(le => le.ZoneId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_LayerEquipment_Zones");
            });
        }

        private static void OnModelCreatingZoneEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ZoneEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Zindex).HasColumnName("ZIndex");

                entity.HasOne(d => d.LayerGroup)
                    .WithMany(p => p.Zones)
                    .HasForeignKey(d => d.LayerGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Zones_ZoneGroups");
            });
        }

        private static void OnModelCreatingLayerGroupEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LayerGroupEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Zindex).HasColumnName("ZIndex");

                entity.HasOne(d => d.Floor)
                    .WithMany(p => p.LayerGroups)
                    .HasForeignKey(d => d.FloorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ZoneGroups_Floors");
            });
        }

        private static void OnModelCreatingSiteEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SiteEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Suburb)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Country)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Postcode)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.State)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Area)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.TimezoneId)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.SiteCode)
                    .HasMaxLength(20);

                entity.Property(e => e.SiteContactEmail)
                    .HasMaxLength(100);

                entity.Property(e => e.SiteContactName)
                    .HasMaxLength(100);

                entity.Property(e => e.SiteContactPhone)
                    .HasMaxLength(50);

                entity.Property(e => e.SiteContactTitle)
                    .HasMaxLength(50);

				entity.HasQueryFilter(e => e.Status != SiteStatus.Deleted);
            });
        }

        private static void OnModelCreatingLayerEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LayerEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.LayerGroup)
                    .WithMany(p => p.Layers)
                    .HasForeignKey(d => d.LayerGroupId)
                    .HasConstraintName("FK_Layers_ZoneGroups");
            });
        }

        private static void OnModelCreatingFloorEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FloorEntity>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Code).HasMaxLength(10);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(d => d.Site)
                    .WithMany(p => p.Floors)
                    .HasForeignKey(d => d.SiteId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Floors_Sites");
            });
        }

        private static void OnModelCreatingSitePreferencesEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SitePreferencesEntity>(entity =>
            {
                entity.HasKey(sp => new { sp.SiteId, sp.ScopeId });
            });
        }
    }
}
