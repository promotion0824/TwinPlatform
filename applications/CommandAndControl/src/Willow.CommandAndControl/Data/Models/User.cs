namespace Willow.CommandAndControl.Data.Models;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Application User.
/// </summary>
[ComplexType]
public class User
{
    /// <summary>
    /// Gets an empty user.
    /// </summary>
    public static User Empty => new();

    /// <summary>
    /// Gets or sets the full name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string? Email { get; set; }
}
