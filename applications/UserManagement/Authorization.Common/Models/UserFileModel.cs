using Authorization.Common.Abstracts;

namespace Authorization.Common.Models;

/// <summary>
/// User File Model used for User Import Export action.
/// </summary>
public class UserFileModel : BaseFileImportModel, IUser
{
    /// <summary>
    /// Name of the Record Type
    /// </summary>
    public const string Type = "User";

    /// <summary>
    /// First name of the User.
    /// </summary>
    public string FirstName { get; set; } = default!;

    /// <summary>
    /// Last name of the User.
    /// </summary>
    public string LastName { get; set; } = default!;

    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; set; } = default!;

    /// <summary>
    /// Full Name of the User.
    /// </summary>
    public string FullName
    {
        get
        {
            return $"{FirstName ?? string.Empty} {LastName ?? string.Empty}".Trim();
        }
    }
}
