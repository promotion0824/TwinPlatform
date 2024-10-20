namespace Authorization.TwinPlatform.Services.Hosted.Request;

/// <summary>
/// Abstract for background request.
/// </summary>
public interface IBackgroundRequest
{
    /// <summary>
    /// Get the unique identifier for the request
    /// </summary>
    /// <returns>Identifier as string</returns>
    string GetIdentifier();
}
