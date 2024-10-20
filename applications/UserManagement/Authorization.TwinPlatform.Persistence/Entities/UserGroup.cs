using System.ComponentModel.DataAnnotations.Schema;

namespace Authorization.TwinPlatform.Persistence.Entities;

/// <summary>
/// User Group Entity mapped to UserGroup table
/// </summary>
public class UserGroup : IEntityBase
{
	/// <summary>
	/// Id of the UserGroup
	/// </summary>
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid Id { get; set; } = default;

	/// <summary>
	/// Id of the Group Entity
	/// </summary>
	public Guid GroupId { get; set; }

	/// <summary>
	/// Id of the User Entity
	/// </summary>
	public Guid UserId { get; set; }

	/// <summary>
	/// Group Navigation Property
	/// </summary>
	[ForeignKey(nameof(GroupId))]
	public Group Group { get; set; } = default!;

	/// <summary>
	/// User Navigation Property
	/// </summary>
	[ForeignKey(nameof(UserId))]
	public User User { get; set; } = default!;
}

