namespace Authorization.TwinPlatform.Common.Model;

/// <summary>
/// Model DTO that maps to the User Entity
/// </summary>
public class UserModel
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
        get
        {
            return $"{FirstName} {LastName}";
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
    /// Date time in UTC the user record is created.
    /// </summary>
    public DateTimeOffset CreatedDate { get; set; }

    /// <summary>
    /// Status of the User record. 0 - Active; 1 - Inactive.
    /// </summary>
    public UserStatus Status { get; set; }

}

public enum UserStatus
{
    Active = 0,
    Inactive = 1,
}

