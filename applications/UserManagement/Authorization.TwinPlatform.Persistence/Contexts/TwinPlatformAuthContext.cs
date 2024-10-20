using Authorization.TwinPlatform.Persistence.Configurations;
using Authorization.TwinPlatform.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authorization.TwinPlatform.Persistence.Contexts;

/// <summary>
/// Authorization Twin Platform Database Context
/// </summary>
public class TwinPlatformAuthContext : DbContext
{
	public DbSet<User> Users { get; set; } = null!;
	public DbSet<Role> Roles { get; set; } = null!;
	public DbSet<Permission> Permissions { get; set; } = null!;
	public DbSet<Application> Applications { get; set; } = null!;
    public DbSet<ApplicationClient> ApplicationClients { get; set; } = null!;
	public DbSet<RolePermission> RolePermissions { get; set; } = null!;
	public DbSet<RoleAssignment> RoleAssignments { get; set; } = null!;
	public DbSet<GroupType> GroupTypes { get; set; } = null!;
	public DbSet<Group> Groups { get; set; } = null!;
	public DbSet<UserGroup> UserGroups { get; set; } = null!;
	public DbSet<GroupRoleAssignment> GroupRoleAssignments { get; set; } = null!;
	public DbSet<ClientAssignment> ClientAssignments { get; set; } = null!;
	public DbSet<ClientAssignmentPermission> ClientAssignmentPermissions { get; set; } = null!;

	public TwinPlatformAuthContext(DbContextOptions options) : base(options)
	{     
    }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
		modelBuilder.ApplyConfiguration(new RoleAssignmentConfiguration());
		modelBuilder.ApplyConfiguration(new PermissionConfiguration());
		modelBuilder.ApplyConfiguration(new RoleConfiguration());
		modelBuilder.ApplyConfiguration(new UserConfiguration());
		modelBuilder.ApplyConfiguration(new GroupTypeConfiguration());
		modelBuilder.ApplyConfiguration(new GroupConfiguration());
        modelBuilder.ApplyConfiguration(new UserGroupConfiguration());
		modelBuilder.ApplyConfiguration(new GroupRoleAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationClientConfiguration());

        modelBuilder.ApplyConfiguration(new ClientAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new ClientAssignmentPermissionConfiguration());

		//Map User Defined functions here
		DbFunctionConfiguration.ApplyConfiguration(modelBuilder);
	}

	#region Table Valued Functions
	public IQueryable<User> GetUsersWithinGroup(string name)
		=> FromExpression(() => GetUsersWithinGroup(name));

	public IQueryable<RoleAssignment> GetRoleAssignmentsByUser(string userEmail)
	=> FromExpression(() => GetRoleAssignmentsByUser(userEmail));
	#endregion
}
