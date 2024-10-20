using WorkflowCore.Entities.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Data;
using WorkflowCore.Models;
using System;

namespace WorkflowCore.Entities
{
    public class WorkflowContext : DbContext
    {
        public WorkflowContext(DbContextOptions<WorkflowContext> options) : base(options)
        {
            this.UseManagedIdentity();
            ChangeTracker.LazyLoadingEnabled = false;
        }

        public virtual DbSet<SchemaEntity> Schemas { get; set; }
        public virtual DbSet<SchemaColumnEntity> SchemaColumns { get; set; }
        public virtual DbSet<TicketCategoryEntity> TicketCategories { get; set; }
        public virtual DbSet<TicketEntity> Tickets { get; set; }
        public virtual DbSet<TicketTemplateEntity> TicketTemplates { get; set; }
        public virtual DbSet<TicketNextNumberEntity> TicketNextNumbers { get; set; }
        public virtual DbSet<TicketNextNumberEntity> TicketTemplateNextNumbers { get; set; }
        public virtual DbSet<AttachmentEntity> Attachments { get; set; }
        public virtual DbSet<CommentEntity> Comments { get; set; }
        public virtual DbSet<ReporterEntity> Reporters { get; set; }
        public virtual DbSet<NotificationReceiverEntity> NotificationReceivers { get; set; }
        public virtual DbSet<WorkgroupEntity> Workgroups { get; set; }
        public virtual DbSet<WorkgroupMemberEntity> WorkgroupMembers { get; set; }
        public virtual DbSet<ZoneEntity> Zones { get; set; }
        public virtual DbSet<InspectionEntity> Inspections { get; set; }
        public virtual DbSet<CheckEntity> Checks { get; set; }
        public virtual DbSet<InspectionRecordEntity> InspectionRecords { get; set; }
        public virtual DbSet<CheckRecordEntity> CheckRecords { get; set; }
        public virtual DbSet<SiteExtensionEntity> SiteExtensions { get; set; }
        public virtual DbSet<TicketTaskEntity> TicketTasks { get; set; }
        public virtual DbSet<ScheduleEntity> Schedules { get; set; }
        public virtual DbSet<TicketNextNumberEntity> TicketSequenceNumbers { get; set; }
        public virtual DbSet<TicketStatusEntity> TicketStatuses { get; set; }

		public virtual DbSet<AuditTrailEntity> AuditTrails { get; set; }
        public virtual DbSet<ExternalProfileEntity> ExternalProfiles { get; set; }

        public virtual DbSet<ServiceNeededEntity> ServiceNeeded { get; set; }
        public virtual DbSet<ServiceNeededSpaceTwinEntity> ServiceNeededSpaceTwin { get; set; }
        public virtual DbSet<JobTypeEntity> JobTypes { get; set; }
        public virtual DbSet<TicketInsightEntity> TicketInsights { get; set; }
        public virtual DbSet<TicketStatusTransitionsEntity> TicketStatusTransitions { get; set; }
        public virtual DbSet<TicketSubStatusEntity> TicketSubStatus { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // https://github.com/aspnet/EntityFrameworkCore/issues/11295#issuecomment-373852015
            modelBuilder.HasDbFunction(typeof(JsonExtensions).GetMethod(nameof(JsonExtensions.JsonValue)))
                    .HasName("JSON_VALUE") // function name in server
                    .HasSchema(""); // empty string since in built functions has no schema

            modelBuilder.Entity<SchemaEntity>().HasMany(x => x.SchemaColumns).WithOne(x => x.Schema).HasForeignKey(x => x.SchemaId);

            modelBuilder.Entity<TicketEntity>().HasMany(x => x.Comments).WithOne(x => x.Ticket).HasForeignKey(x => x.TicketId);
            modelBuilder.Entity<TicketEntity>().HasMany(x => x.Attachments).WithOne(x => x.Ticket).HasForeignKey(x => x.TicketId);
            modelBuilder.Entity<TicketEntity>().HasMany(x => x.Tasks).WithOne(x => x.Ticket).HasForeignKey(x => x.TicketId);

            modelBuilder.Entity<TicketEntity>().Property(x => x.Latitude).HasPrecision(9, 6);
            modelBuilder.Entity<TicketEntity>().Property(x => x.Longitude).HasPrecision(9, 6);

            modelBuilder.Entity<TicketEntity>().Property(x => x.ComputedCreatedDate).ValueGeneratedOnAddOrUpdate();
            modelBuilder.Entity<TicketEntity>().Property(x => x.ComputedCreatedDate).HasComputedColumnSql("IIF(LastUpdatedByExternalSource = 1, ISNULL(ExternalCreatedDate, CreatedDate), CreatedDate)");
            modelBuilder.Entity<TicketEntity>().Property(x => x.ComputedCreatedDate).ValueGeneratedOnAddOrUpdate();
            modelBuilder.Entity<TicketEntity>().Property(x => x.ComputedUpdatedDate).HasComputedColumnSql("IIF(LastUpdatedByExternalSource = 1, ISNULL(ExternalUpdatedDate, UpdatedDate), UpdatedDate)");

            modelBuilder.Entity<TicketCategoryEntity>().HasMany(x => x.Tickets).WithOne(x => x.Category).HasForeignKey(x => x.CategoryId);

            modelBuilder.Entity<TicketNextNumberEntity>().HasKey(c => c.Prefix);

            modelBuilder.Entity<NotificationReceiverEntity>().HasKey(c => new { c.SiteId, c.UserId });

            modelBuilder.Entity<WorkgroupMemberEntity>()
                .HasKey(wm => new { wm.WorkgroupId, wm.MemberId });

            modelBuilder.Entity<InspectionEntity>()
                .HasMany(x => x.Checks)
                .WithOne()
                .HasForeignKey(x => x.InspectionId);

            modelBuilder.Entity<SiteExtensionEntity>().HasKey(x => x.SiteId);

			modelBuilder
				.Entity<InspectionEntity>()
				.Property(d => d.FrequencyUnit)
				.HasConversion(
					v => v.ToString(),
					v => (SchedulingUnit)Enum.Parse(typeof(SchedulingUnit), v));

            modelBuilder.Entity<TicketStatusEntity>().HasKey(s => new { s.CustomerId, s.Status });
        }
    }
}
