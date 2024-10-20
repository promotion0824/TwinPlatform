namespace Authorization.Common.Abstracts;

/// <summary>
/// Interface for identifying user models
/// </summary>
public interface IUser
{
    public string FullName { get; }

    /// <summary>
    /// Email address of the user.
    /// </summary>
    public string Email { get; set; }
}
