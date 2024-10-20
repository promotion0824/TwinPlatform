using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Polly.Registry;

namespace PlatformPortalXL.Services;

public static class ResiliencePipelineName
{
    public const string Retry = "retry-pipeline";
}

/// <summary>
/// A resilience pipeline service is responsible for executing tasks with resilience policies such as retries.
/// </summary>
/// <remarks>
/// The resilience pipeline service is a facade that abstracts the underlying Polly resilience policies and provides a
/// simple interface for executing tasks with resilience policies.
/// </remarks>
public interface IResiliencePipelineService
{
    ValueTask<TResult> ExecuteAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state);

    ValueTask<TResult> ExecuteAsync<TResult>(Func<ResilienceContext, ValueTask<TResult>> callback, ResilienceContext context);
    ValueTask<TResult> ExecuteAsync<TResult, TState>(Func<TState, CancellationToken, ValueTask<TResult>> callback, TState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the specified callback.
    /// </summary>
    /// <param name="callback">The user-provided callback.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> associated with the callback.</param>
    /// <returns>The instance of <see cref="ValueTask"/> that represents an asynchronous callback.</returns>
    ValueTask<TResult> ExecuteAsync<TResult>(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken cancellationToken = default);
}

public class ResiliencePipelineService : IResiliencePipelineService
{
    private readonly ResiliencePipelineProvider<string> _pipelineProvider;

    public ResiliencePipelineService(ResiliencePipelineProvider<string> pipelineProvider)
    {
        _pipelineProvider = pipelineProvider;
    }

    public async ValueTask<TResult> ExecuteAsync<TResult, TState>(Func<ResilienceContext, TState, ValueTask<TResult>> callback, ResilienceContext context, TState state)
    {
        return await _pipelineProvider.GetPipeline(ResiliencePipelineName.Retry).ExecuteAsync(callback, context, state);
    }

    public async ValueTask<TResult> ExecuteAsync<TResult>(Func<ResilienceContext, ValueTask<TResult>> callback, ResilienceContext context)
    {
        return await _pipelineProvider.GetPipeline(ResiliencePipelineName.Retry).ExecuteAsync(callback, context);
    }

    public async ValueTask<TResult> ExecuteAsync<TResult, TState>(Func<TState, CancellationToken, ValueTask<TResult>> callback, TState state, CancellationToken cancellationToken = default)
    {
        return await _pipelineProvider.GetPipeline(ResiliencePipelineName.Retry).ExecuteAsync(callback, state, cancellationToken);
    }

    public async ValueTask<TResult> ExecuteAsync<TResult>(Func<CancellationToken, ValueTask<TResult>> callback, CancellationToken cancellationToken = default)
    {
        return await _pipelineProvider.GetPipeline(ResiliencePipelineName.Retry).ExecuteAsync(callback, cancellationToken);
    }
}
