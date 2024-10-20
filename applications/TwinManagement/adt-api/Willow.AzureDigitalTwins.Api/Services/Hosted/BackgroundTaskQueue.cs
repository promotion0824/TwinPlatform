using System.Collections.Concurrent;

namespace Willow.AzureDigitalTwins.Api.Services.Hosted;

/// <summary>
/// Background task queue interface
/// </summary>
public interface IBackgroundTaskQueue<T>
{
    /// <summary>
    /// Queue a job to process
    /// </summary>
    /// <param name="job">Instance of T</param>
    /// <returns>Return true if queued successfully; else false.</returns>
    public bool TryQueueJob(T job);

    /// <summary>
    /// Dequeue a job from the queue
    /// </summary>
    /// <param name="job">Instance of T</param>
    /// <returns>Return true if dequeue successfully; else false.</returns>
    public bool TryDequeueJob(out T job);

    /// <summary>
    /// Peek in to a job from the queue without dequeuing 
    /// </summary>
    /// <param name="job">Instance of T</param>
    /// <returns>Return true if peek was successful; else false.</returns>
    public bool TryPeekJob(out T job);
}

/// <summary>
/// Background task queue class implementation
/// </summary>
public class BackgroundTaskQueue<T> : IBackgroundTaskQueue<T>
{
    const int MaximumTwinJobInAQueue = 10;
    private readonly ConcurrentQueue<T> jobsQueue = new();

    /// <summary>
    /// Queue a job to process
    /// </summary>
    /// <param name="job">Instance of T</param>
    /// <returns>Return true if queued successfully; else false.</returns>
    public bool TryQueueJob(T job)
    {
        if (jobsQueue.Count < MaximumTwinJobInAQueue)
        {
            jobsQueue.Enqueue(job);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Dequeue a job from the queue
    /// </summary>
    /// <param name="job">Instance of T</param>
    /// <returns>Return true if dequeue successfully; else false.</returns>
    public bool TryDequeueJob(out T job)
    {
        return jobsQueue.TryDequeue(out job);
    }

    /// <summary>
    /// Peek in to a job from the queue without dequeuing 
    /// </summary>
    /// <param name="job">Instance of T</param>
    /// <returns>Return true if peek was successful; else false.</returns>
    public bool TryPeekJob(out T job)
    {
        return jobsQueue.TryDequeue(out job);
    }
}
