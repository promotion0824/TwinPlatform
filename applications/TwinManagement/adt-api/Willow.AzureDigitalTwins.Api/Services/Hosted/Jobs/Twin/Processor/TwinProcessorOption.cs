namespace Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs.Twin.Processor;
public record TwinProcessorOption
{
    /// <summary>
    /// True to enable processor; false it not.
    /// </summary>
    public bool Enabled { get; set; }

	/// <summary>
	/// Processor Priority
	/// </summary>
	public int Priority { get; set; }
}
