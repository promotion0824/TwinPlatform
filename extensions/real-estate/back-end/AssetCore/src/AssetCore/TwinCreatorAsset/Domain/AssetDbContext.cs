using AssetCoreTwinCreator.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace AssetCoreTwinCreator.Domain
{
    public class AssetDbContext : DbContext
    {
        public AssetDbContext() { }
        public AssetDbContext(DbContextOptions<AssetDbContext> options) : base(options)
        {
            this.UseManagedIdentity();
            ChangeTracker.LazyLoadingEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public virtual Task<IDbContextTransaction> GetDbContextTransaction()
        {
            return Database.BeginTransactionAsync();
        }

        public virtual DbSet<Asset> Assets { get; set; }
        public virtual DbSet<AssetChangeLog> AssetChangeLogs { get; set; }
        public virtual DbSet<Building> Buildings { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<CategoryColumn> CategoryColumns { get; set; }
        public virtual DbSet<Floor> Floors { get; set; }
        public virtual DbSet<Group> Groups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Asset>()
            .ToTable("TES_Asset_Register")
            .HasKey(x => x.Id);

            modelBuilder.Entity<AssetChangeLog>()
            .ToTable("TES_Asset_ChangeLog");

            modelBuilder.Entity<Building>()
            .ToTable("TES_Building");

            modelBuilder.Entity<Category>()
            .ToTable("TES_Category");

            modelBuilder.Entity<Category>()
            .HasQueryFilter(x => x.Archived == false)   //Global Query filters can be ignored by consumers: https://docs.microsoft.com/en-us/ef/core/querying/filters
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.ChildCategories)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CategoryColumn>()
            .ToTable("TES_Category_Column");

            modelBuilder.Entity<CategoryGroup>()
            .ToTable("TES_Category_Group")
            .HasKey(x => new { x.CategoryId, x.GroupId });

            modelBuilder.Entity<CategoryGroup>()
            .HasOne(x => x.Category)
            .WithMany(x => x.CategoryGroups)
            .HasForeignKey(x => x.CategoryId);

            modelBuilder.Entity<CategoryGroup>()
            .HasOne(x => x.Group)
            .WithMany(x => x.CategoryGroups)
            .HasForeignKey(x => x.GroupId);

            modelBuilder.Entity<Floor>()
            .ToTable("TES_Floor");

            modelBuilder.Entity<Group>()
            .ToTable("TES_Group");

            modelBuilder.Entity<Building>()
            .HasMany(x => x.Floors);

            modelBuilder.Entity<Floor>()
            .HasOne(x => x.Building)
            .WithMany(x => x.Floors)
            .HasForeignKey(x => x.BuildingId);
        }

    }
}