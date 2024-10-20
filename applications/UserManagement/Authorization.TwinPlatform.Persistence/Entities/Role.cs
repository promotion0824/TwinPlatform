using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.TwinPlatform.Persistence.Entities;

/// <summary>
/// Role entity class mapped to Role table
/// </summary>
public class Role : IEntityBase
{
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[Key]
	public Guid Id { get; set; }

	[Required]
	[StringLength(100)]
	public string Name { get; set; } = default!;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = default!;

    public ICollection<RolePermission> RolePermission { get; set; } = new List<RolePermission>();
}
