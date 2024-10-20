namespace Authorization.TwinPlatform.Common.Abstracts;

/// <summary>
/// Abstract for Admin Service
/// </summary>
public interface IAdminService
{
    /// <summary>
    /// Get List of configured admin emails from permission api.
    /// </summary>
    /// <returns>List of emails.</returns>
    public Task<IEnumerable<string>> GetAdminEmails();
}
