using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.TwinPlatform.Persistence.Entities;

/// <summary>
/// Client Assignment Entity.
/// </summary>
public class ClientAssignment : IEntityBase
{
    /// <summary>
    /// Id of the Client Assignment.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid ApplicationClientId { get; set; }

    /// <summary>
    /// Expression string that defines the scope of resources.
    /// </summary>
    [StringLength(1000)]
    public string? Expression { get; set; }

    /// <summary>
    /// Boolean condition that dynamically sets the state of the assignment.
    /// Expression supports Willow Expression Language.
    /// String.Empty or null will be considered as an active assignment.
    /// </summary>
    [StringLength(400)]
    public string? Condition { get; set; }

    /// <summary>
    /// Application Client.
    /// </summary>
    [ForeignKey(nameof(ApplicationClientId))]
    public ApplicationClient ApplicationClient { get; set; } = default!;

    public ICollection<ClientAssignmentPermission> ClientAssignmentPermissions { get; set; } = [];
}
