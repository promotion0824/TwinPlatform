using Microsoft.EntityFrameworkCore;
using NotificationCore.Infrastructure.Extensions;

namespace NotificationCore.Entities;

public class NotificationDbContext: DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
        this.UseManagedIdentity();
        ChangeTracker.LazyLoadingEnabled = false;
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<NotificationTriggerEntity> NotificationTriggers { get; set; }
    public DbSet<NotificationTriggerTwinEntity> NotificationTriggerTwins { get; set; }
    public DbSet<NotificationTriggerSkillEntity> NotificationTriggerSkills { get; set; }
    public DbSet<NotificationTriggerSkillCategoryEntity> NotificationTriggerSkillCategories { get; set; }
    public DbSet<NotificationTriggerTwinCategoryEntity> NotificationTriggerTwinCategories { get; set; }
    public DbSet<NotificationSubscriptionOverrideEntity> NotificationSubscriptionOverrides { get; set; }
    public DbSet<WorkgroupSubscriptionEntity> WorkgroupSubscriptions { get; set; }
    public DbSet<LocationEntity> Locations { get; set; }
    public DbSet<NotificationEntity> Notifications { get; set; }
    public DbSet<NotificationUserEntity> NotificationsUsers { get; set; }

}
