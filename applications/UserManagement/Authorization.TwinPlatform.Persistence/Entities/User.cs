using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.TwinPlatform.Persistence.Entities;

public class User : IEntityBase
{
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[Key]
	public Guid Id { get; set; }

	[StringLength(100)]
	public string FirstName { get; set; } = default!;

	[StringLength(150)]
	public string LastName { get; set; } = default!;

	[Required]
	[MaxLength(100)]
	public string Email { get; set; } = default!;

	[Required]
	[MaxLength(100)]
	public string EmailConfirmationToken { get; set; } = default!;

	public DateTime EmailConfirmationTokenExpiry { get; set; }

	public bool EmailConfirmed { get; set; }

	public DateTimeOffset CreatedDate { get; set; }

	public int Status { get; set; }

	public ICollection<RoleAssignment> RoleAssignments { get; set; } = new List<RoleAssignment>();
	public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}
