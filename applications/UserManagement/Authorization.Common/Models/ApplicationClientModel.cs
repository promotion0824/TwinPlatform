
namespace Authorization.Common.Models;
public class ApplicationClientModel
{
    /// <summary>
    /// Id of the Client. This will be same as the Object Id of the App Registration
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Display name of the Client.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Description of the client.
    /// </summary>
    public string Description { get; set; } = default!;

    /// <summary>
    /// Application (Client) Id from the App Registration
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Willow Application
    /// </summary>
    public ApplicationModel Application { get; set; } = default!;
}
