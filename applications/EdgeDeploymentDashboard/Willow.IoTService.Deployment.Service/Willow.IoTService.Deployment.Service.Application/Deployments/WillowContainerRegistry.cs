namespace Willow.IoTService.Deployment.Service.Application.Deployments;

using JetBrains.Annotations;

/// <summary>
///     Willow Container Registry record.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public record WillowContainerRegistry
{
    /// <summary>
    ///     Gets username for the ACR scoped token.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    ///     Gets username for the ACR scoped token.
    /// </summary>
    public string Username2 { get; init; } = string.Empty;

    /// <summary>
    ///     Gets password for the ACR scoped token.
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    ///     Gets password for the ACR scoped token.
    /// </summary>
    public string Password2 { get; init; } = string.Empty;

    /// <summary>
    ///     Gets address for the ACR containing the connector images.
    /// </summary>
    public string Address { get; init; } = string.Empty;

    /// <summary>
    ///     Gets address for the ACR containing the connector images.
    /// </summary>
    public string Address2 { get; init; } = string.Empty;

    /// <summary>
    ///     Deconstructs the record into its components.
    /// </summary>
    /// <param name="username">User Name.</param>
    /// <param name="username2">User Name 2.</param>
    /// <param name="password">Password.</param>
    /// <param name="password2">Password 2.</param>
    /// <param name="address">ACR address.</param>
    /// <param name="address2">ACR address 2.</param>
    public void Deconstruct(
        out string username,
        out string username2,
        out string password,
        out string password2,
        out string address,
        out string address2)
    {
        username = this.Username;
        username2 = this.Username2;
        password = this.Password;
        password2 = this.Password2;
        address = this.Address;
        address2 = this.Address2;
    }
}
