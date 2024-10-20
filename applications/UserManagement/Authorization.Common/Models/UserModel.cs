using Authorization.Common.Abstracts;

namespace Authorization.Common.Models;

/// <summary>
/// Model DTO that maps to the User Entity
/// </summary>
public class UserModel : IUser
{
    /// <summary>
    /// Unique Identifier of the user model.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// First name of the User.
    /// </summary>
    public string FirstName { get; set; } = default!;

    /// <summary>
    /// Last name of the User.
    /// </summary>
    public string LastName { get; set; } = default!;

    /// <summary>
    /// Full Name of the User.
    /// </summary>
    public string FullName
    {
        get {
            return $"{FirstName ?? string.Empty} {LastName ?? string.Empty}".Trim();
        }
    }

    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Define whether the Email address is verified or not.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Date time the user record is created.
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// Status of the User record. 0 - Active; 1 - Inactive.
    /// </summary>
    public UserStatus Status { get; set; }

    /// <summary>
    /// Get or Sets whether the user is super admin or not.
    /// </summary>
    public bool isAdmin { get; set; } = false;
}

public enum UserStatus
{
    Active = 0,
    Inactive = 1,
}
