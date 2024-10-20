using Authorization.TwinPlatform.Services.Hosted.Request;
using System.Collections.Concurrent;

namespace Authorization.TwinPlatform.Services.Hosted;

/// <summary>
/// Contract for background queue receiver.
/// </summary>
/// <typeparam name="T">Request type.</typeparam>
public interface IBackgroundQueueReceiver<T>
{
    /// <summary>
    /// Set the status of the queue.
    /// </summary>
    /// <param name="status">True to active; False to inactive.</param>
    /// <returns>Current status of the queue.</returns>
    public bool SetStatus(bool status);

    /// <summary>
    /// Get all request from the background queue.
    /// </summary>
    /// <returns>List of <typeparamref name="T"/></returns>
    public List<T> GetAll();

    /// <summary>
    /// Dequeue the item based on the key value.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>True if removed; false if not.</returns>
    public bool Dequeue(string key);
}

/// <summary>
/// Contract for background queue sender.
/// </summary>
/// <typeparam name="T">Request type.</typeparam>
public interface IBackgroundQueueSender<T>
{

    /// <summary>
    /// Returns the status of the queue.
    /// </summary>
    /// <returns>Status as boolean.</returns>
    public bool GetStatus();

    /// <summary>
    /// Enqueue new request in to the background queue if not already exist.
    /// </summary>
    /// <param name="request">Instance of <typeparamref name="T"/></param>
    /// <returns>True if added; false if not.</returns>
    public bool Enqueue(T request);
}

/// <summary>
/// Background queue implementation
/// </summary>
/// <typeparam name="T">Request instance type.</typeparam>
public class BackgroundQueue<T> : ConcurrentDictionary<string, T>, IBackgroundQueueSender<T>, IBackgroundQueueReceiver <T> where T : IBackgroundRequest
{
    private bool _isActive = false;

    /// <summary>
    /// Set the status of the queue.
    /// </summary>
    /// <param name="status">True to active; False to inactive.</param>
    /// <returns>Current status of the queue.</returns>
    public bool SetStatus(bool status)
    {
        _isActive = status;
        return _isActive;
    }

    /// <summary>
    /// Returns the status of the queue.
    /// </summary>
    /// <returns>Status as boolean.</returns>
    public bool GetStatus() => _isActive;

    /// <summary>
    /// Enqueue new request in to the background queue if not already exist.
    /// </summary>
    /// <param name="request">Instance of <typeparamref name="T"/></param>
    /// <returns>True if added; false if not.</returns>
    public bool Enqueue(T request)
    {
        if (!_isActive) return _isActive;

        return this.TryAdd(request.GetIdentifier(), request);
    }

    /// <summary>
    /// Get all request from the background queue.
    /// </summary>
    /// <returns>List of <typeparamref name="T"/></returns>
    public List<T> GetAll()
    {
        return this.Values.ToList();
    }

    /// <summary>
    /// Dequeue the item based on the key value.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>True if removed; false if not.</returns>
    public bool Dequeue(string key)
    {
        if (!_isActive) return _isActive;

        return this.TryRemove(key, out _);
    }

}
