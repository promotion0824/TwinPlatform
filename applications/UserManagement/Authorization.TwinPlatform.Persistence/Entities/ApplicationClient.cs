using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Authorization.TwinPlatform.Persistence.Entities;
public class ApplicationClient : IEntityBase
{
    /// <summary>
    /// Id of the Client.
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the Client.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// Description of the client.
    /// </summary>
    [StringLength(200)]
    public string Description { get; set; } = default!;

    /// <summary>
    /// Application (Client) Id from the App Registration
    /// </summary>
    [Required]
    public Guid ClientId { get; set; }

    public Guid ApplicationId { get; set; }

    /// <summary>
    /// Application Navigation Property
    /// </summary>
    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = default!;
}
