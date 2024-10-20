using Polly;
using Polly.Retry;
using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.Alert.Resolver.ResolutionHandlers.Enumerations;
using Willow.Alert.Resolver.ResolutionHandlers.Extensions;
using Willow.IoTService.Monitoring.Contracts;
using Willow.IoTService.Monitoring.Enums;
using Willow.IoTService.Monitoring.Services.AppInsights;

namespace Willow.Alert.Resolver.ResolutionHandlers.Implementation;

internal abstract class RetryableResolutionHandlerBase<TRequest> : IResolutionHandler<TRequest> where TRequest : ResolutionRequest
{
    private readonly ILogger<IResolutionHandler<TRequest>> _logger;
    private readonly IMonitorEventTracker? _monitorEventTracker;

    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly int _retryAttempt = 3;
    private readonly int _exponentialBackoffSeconds = 5;
    protected RetryableResolutionHandlerBase(ILogger<IResolutionHandler<TRequest>> logger,
                                             IConfiguration configuration,
                                             IMonitorEventTracker? monitorEventTracker = null)
    {
        _logger = logger;
        _monitorEventTracker = monitorEventTracker;
        _retryAttempt = configuration.GetValue("ResolutionSettings:RetryAttempts", _retryAttempt);
        _exponentialBackoffSeconds = configuration.GetValue("ResolutionSettings:ExponentialBackoffSeconds", _exponentialBackoffSeconds);
        _retryPolicy = BuildRetryPolicy();
    }

    private AsyncRetryPolicy BuildRetryPolicy()
    {

        return Policy.Handle<ResolutionException>()
            .WaitAndRetryAsync(_retryAttempt, retryAttempt => TimeSpan.FromSeconds(Math.Pow(_exponentialBackoffSeconds, retryAttempt)),
        (exception, timeSpan, retryAttempt, context) =>
        {
            _logger.LogInformation("{operation}: Retry number {retryNo} within {timespan}ms due to {exception}",
                                   this.GetType().Name,
                                   retryAttempt,
                                   timeSpan.TotalSeconds,
                                   exception.Message);
        }
        );
    }

    public abstract Task<bool> RunAsync(TRequest request, IResolutionContext context, CancellationToken cancellationToken = default);
    public IResolutionHandler<TRequest>? Next { get; set; }
    public void SetNext(IResolutionHandler<TRequest> resolverStep)
    {
        Next = resolverStep;
    }
    public async Task<bool> RunAsChainAsync(TRequest request, IResolutionContext context, CancellationToken cancellationToken = default)
    {

        if (cancellationToken.IsCancellationRequested) return false;
        var result = false;

        try
        {
            result = await _retryPolicy.ExecuteAsync(async () => await RunInternalAsync(request, context, cancellationToken));
            context.AddResponse(this, result.GetResolutionStatus());
            _logger.LogInformation($"{this.GetType().Name} has been executed successfully");
        }
        catch (Exception ex)
        {
            context.AddResponse(this, false.GetResolutionStatus(), errorMessage: ex.Message);
            _logger.LogError(ex, $"{this.GetType().Name} has failed with exception {ex.Message}");
        }
        finally
        {
            if (_monitorEventTracker is not null) SendMonitorEvent(request, context);
        }

        if (Next != null)
        {
            return await Next.RunAsChainAsync(request, context, cancellationToken);
        }
        return result;
    }

    private void SendMonitorEvent(TRequest request, IResolutionContext context)
    {
        var metrics = new Dictionary<string, double>(context.Metrics);
        foreach (var response in context.Responses)
        {
            metrics.Add(response.Key, response.Value.Status == ResolutionStatus.Success ? 1 : 0);
        }

        var properties = new Dictionary<string, string>(context.CustomProperties);

        var monitorEvent = new MonitorEvent
        {
            DeviceId = request.DeviceId,
            ConnectorId = Guid.Parse(request.ConnectorId),
            ConnectorName = request.ConnectorName,
            ConnectorType = request.ConnectorType,
            MonitorSource = MonitorSource.AlertResolver,
            CustomProperties = properties,
            Metrics = metrics
        };
        _monitorEventTracker?.Execute(monitorEvent);
    }

    private async Task<bool> RunInternalAsync(TRequest request, IResolutionContext context, CancellationToken cancellationToken)
    {
        try
        {
            return await RunAsync(request, context, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ResolutionException(this.GetType().Name, ex.Message);
        }
    }
}
