
namespace Authorization.TwinPlatform.Common.Abstracts;

/// <summary>
/// Authorization Token Service Contract
/// </summary>
public interface IAuthorizationApiTokenService
{
	/// <summary>
	/// Method to get access tokem
	/// </summary>
	/// <returns>Access token string</returns>
	public Task<string> GetTokenAsync();

	/// <summary>
	/// Method add access token to the http client headers
	/// </summary>
	/// <param name="httpClient">Http Client instance</param>
	/// <returns>A task was returned</returns>
	public Task AuthorizeClient(HttpClient httpClient);
}
