namespace Willow.CommandAndControl.Application.Requests.GetUserPermissions
{
    /// <summary>
    /// User Management Authorization Response .
    /// </summary>
    public class UserPermissionsResponseDto
    {
        /// <summary>
        /// Gets or sets the list of user's permissions.
        /// </summary>
        public IEnumerable<string> Permissions { get; set; } = null!;

        /// <summary>
        /// Gets or sets a value indicating whether the user has admin privileges.
        /// </summary>
        public bool IsAdminUser { get; set; }
    }
}
