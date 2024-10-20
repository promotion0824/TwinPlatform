using Authorization.TwinPlatform.Services.Hosted.Request;
using System.Threading.Channels;

namespace Authorization.TwinPlatform.Services.Hosted;

/// <summary>
/// Contract for background channel receiver.
/// </summary>
/// <typeparam name="T">Request type.</typeparam>
public interface IBackgroundChannelReceiver<T>
{
    /// <summary>
    /// Set the status of the channel.
    /// </summary>
    /// <param name="status">True to active; False to inactive.</param>
    /// <returns>Current status of the channel.</returns>
    public bool SetStatus(bool status);


    /// <summary>
    /// Read data from the channel asynchronously
    /// </summary>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>IAsyncEnumerable</returns>
    public IAsyncEnumerable<T> ReadEnumerableAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Contract for background channel sender.
/// </summary>
/// <typeparam name="T">Request type.</typeparam>
public interface IBackgroundChannelSender<T>
{

    /// <summary>
    /// Returns the status of the channel.
    /// </summary>
    /// <returns>Status as boolean.</returns>
    public bool GetStatus();

    /// <summary>
    /// Enqueue new request in to the background channel.
    /// </summary>
    /// <param name="request">Instance of <typeparamref name="T"/></param>
    /// <returns>True if added; false if not.</returns>
    public bool Write(T request);
}

public class BackgroundChannel<T> : IBackgroundChannelSender<T>, IBackgroundChannelReceiver<T> where T : IBackgroundRequest
{
    private readonly Channel<T> _channel;
    private bool _activeStatus;

    public BackgroundChannel(Channel<T> channel)
    {
        _channel = channel;
        _activeStatus = false; // set channel initial status to false
    }


    /// <summary>
    /// Enqueue new request in to the background channel.
    /// </summary>
    /// <param name="request">Instance of <typeparamref name="T"/></param>
    /// <returns>True if added; false if not.</returns>
    public bool Write(T request)
    {
        if(!_activeStatus)
            return false;

        return _channel.Writer.TryWrite(request);
    }


    /// <summary>
    /// Enqueue new request in to the background channel.
    /// </summary>
    /// <param name="request">Instance of <typeparamref name="T"/></param>
    /// <returns>True if added; false if not.</returns>
    public IAsyncEnumerable<T> ReadEnumerableAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }

    /// <summary>
    /// Returns the status of the channel.
    /// </summary>
    /// <returns>Status as boolean.</returns>
    public bool GetStatus() => _activeStatus;

    public bool SetStatus(bool status)
    {
        _activeStatus = status;
        return status;
    }
}
