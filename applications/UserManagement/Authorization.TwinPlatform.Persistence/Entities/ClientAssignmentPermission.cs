using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.TwinPlatform.Persistence.Entities;

/// <summary>
/// Client Assignment Permission.
/// </summary>
public class ClientAssignmentPermission: IEntityBase
{
    /// <summary>
    /// Id of the Client Assignment Permission.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Client Assignment Id.
    /// </summary>
    [Required]
    public Guid ClientAssignmentId { get; set; }

    /// <summary>
    /// Permission Id.
    /// </summary>
    [Required]
    public Guid PermissionId { get; set; }

    [ForeignKey(nameof(ClientAssignmentId))]
    public ClientAssignment ClientAssignment { get; set; } = default!;

    [ForeignKey(nameof(PermissionId))]
    public Permission Permission { get; set; } = default!;
}
