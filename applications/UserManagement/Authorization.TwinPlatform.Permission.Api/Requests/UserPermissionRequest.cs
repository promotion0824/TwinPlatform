using System.ComponentModel.DataAnnotations;

namespace Authorization.TwinPlatform.Permission.Api.Requests;

/// <summary>
/// Class that represents list user permission request payload
/// </summary>
public class UserPermissionRequest
{
	[EmailAddress]
	[Required]
	public string UserEmail { get; set; }

	// Note: Extension name is required for all incoming request,
	// as we don't want calling (extension) to see user's permission outside its scope
	[Required]
	public string Extension { get; set; }

    public string GetUMCacheKey => string.Format("{0}_UM", UserEmail);
    public string GetADCacheKey => string.Format("{0}_AD", UserEmail);

}
