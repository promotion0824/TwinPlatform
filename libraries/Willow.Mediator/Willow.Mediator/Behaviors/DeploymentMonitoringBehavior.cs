namespace Willow.Mediator.Behaviors;

using MediatR;
using Microsoft.Extensions.Logging;

/// <summary>
/// A base class for MediatR pipeline behaviors that monitor deployments.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public abstract class DeploymentMonitoringBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeploymentMonitoringBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">An ILogger implementation.</param>
    protected DeploymentMonitoringBehavior(ILogger<IPipelineBehavior<TRequest, TResponse>> logger)
    {
        Logger = logger;
    }

    /// <summary>
    /// Gets the logger.
    /// </summary>
    protected ILogger<IPipelineBehavior<TRequest, TResponse>> Logger { get; }

    /// <summary>
    /// Handle the request.
    /// </summary>
    /// <param name="request">The incoming request.</param>
    /// <param name="next">The next call in the stack.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An asynchronus task.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Request Delegate named next is common practice in .net.")]
    public abstract Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
