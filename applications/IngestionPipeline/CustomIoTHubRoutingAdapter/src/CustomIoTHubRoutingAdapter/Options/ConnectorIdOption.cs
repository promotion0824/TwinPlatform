namespace Willow.CustomIoTHubRoutingAdapter.Options;

internal sealed record ConnectorIdOption
{
    public const string Section = "ConnectorIdList";

    public IEnumerable<string> ConnectorIdList { get; set; } = [];
}
