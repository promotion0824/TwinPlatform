namespace Authorization.TwinPlatform.Persistence.Entities;

/// <summary>
/// Interface with access to common fields used in the DbSet
/// </summary>
public interface IEntityBase
{
	public Guid Id { get; set; }
}