using DirectoryCore.Entities.Permission;
using DirectoryCore.Enums;
using Microsoft.EntityFrameworkCore;

namespace DirectoryCore.Entities
{
    public class DirectoryDbContext : DbContext
    {
        public DirectoryDbContext(DbContextOptions<DirectoryDbContext> options)
            : base(options)
        {
            this.UseManagedIdentity();
            ChangeTracker.LazyLoadingEnabled = false;
            this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public DbSet<CustomerEntity> Customers { get; set; }

        public DbSet<UserEntity> Users { get; set; }

        public DbSet<SupervisorEntity> Supervisors { get; set; }

        public DbSet<PortfolioEntity> Portfolios { get; set; }

        public DbSet<RoleEntity> Roles { get; set; }

        public DbSet<PermissionEntity> Permissions { get; set; }

        public DbSet<RolePermissionEntity> RolePermission { get; set; }

        public DbSet<AssignmentEntity> Assignments { get; set; }
        public DbSet<CustomerUserPreferencesEntity> CustomerUserPreferences { get; set; }

        public DbSet<CustomerUserTimeSeriesEntity> CustomerUserTimeSeries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User's email should be unique
            modelBuilder.Entity<UserEntity>().HasIndex(u => new { u.Email }).IsUnique();

            modelBuilder
                .Entity<UserEntity>()
                .HasOne(x => x.Preferences)
                .WithOne(x => x.User)
                .HasForeignKey<CustomerUserPreferencesEntity>(x => x.CustomerUserId);

            modelBuilder
                .Entity<UserEntity>()
                .HasOne(x => x.TimeSeries)
                .WithOne(x => x.User)
                .HasForeignKey<CustomerUserTimeSeriesEntity>(x => x.CustomerUserId);

            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<AssignmentEntity>()
                .HasKey(
                    a =>
                        new
                        {
                            a.PrincipalId,
                            a.RoleId,
                            a.ResourceId
                        }
                );

            modelBuilder
                .Entity<RolePermissionEntity>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<CustomerUserPreferencesEntity>().HasKey(p => p.CustomerUserId);

            modelBuilder.Entity<CustomerUserTimeSeriesEntity>().HasKey(p => p.CustomerUserId);
        }
    }
}
