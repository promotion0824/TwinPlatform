using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Authorization.TwinPlatform.Persistence.Entities;

/// <summary>
/// Group Type Entity to distinguish the type of groups in User Management
/// </summary>
public class GroupType : IEntityBase
{
    /// <summary>
    /// Id of the Group
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the Group Type. Possible Values [ Application, AzureB2C, AzureAD ]
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = default!;
}
