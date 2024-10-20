using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.TwinPlatform.Persistence.Entities;

/// <summary>
/// Permission entity class mapped to Permission table
/// </summary>
public class Permission : IEntityBase
{	
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[Key]
	public Guid Id { get; set; }

	[Required]
	[StringLength(100)]
	public string Name { get; set; } = default!;

	[Required]
	[StringLength(200)]
	public string Description { get; set; } = default!;

    public Guid? ApplicationId { get; set; }

    /// <summary>
    /// Application Navigation Property
    /// </summary>
    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = default!;

    public ICollection<RolePermission> RolePermission { get; set; } = new List<RolePermission>();
}
