using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Authorization.TwinPlatform.Persistence.Entities;
public class Application : IEntityBase
{
    /// <summary>
    /// Id of the Willow Application.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the Willow Application.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Description for the Application.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Description { get; set; } = default!;

    /// <summary>
    /// Defines whether the application should support Client Credential Auth (Service-to-Service Authentication).
    /// </summary>
    public bool SupportClientAuthentication { get; set; }

    public ICollection<Permission> Permissions { get; set; } = [];

    public ICollection<ApplicationClient> Clients { get; set; } = [];
}
