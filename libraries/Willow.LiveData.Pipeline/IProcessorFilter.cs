namespace Willow.LiveData.Pipeline;

using Azure.Messaging.EventHubs;

/// <summary>
/// A filter that when data is received.
/// </summary>
public interface IProcessorFilter
{
    /// <summary>
    /// Execute the filter when data is received.
    /// </summary>
    /// <param name="eventData">Event data.</param>
    /// <returns>Return true if filter passes.</returns>
    public bool OnDataReceived(EventData eventData);
}
