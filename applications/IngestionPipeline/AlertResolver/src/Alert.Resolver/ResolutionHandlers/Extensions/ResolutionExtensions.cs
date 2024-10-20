using Ardalis.GuardClauses;
using Willow.Alert.Resolver.ResolutionHandlers.Abstractions;
using Willow.Alert.Resolver.ResolutionHandlers.Enumerations;
using Willow.Alert.Resolver.ResolutionHandlers.Implementation;

namespace Willow.Alert.Resolver.ResolutionHandlers.Extensions;

internal static class ResolutionExtensions
{
    public static void AddResponse<TRequest>(this IResolutionContext context, IResolutionHandler<TRequest> handler,
        ResolutionStatus status, string? message = null, string? errorMessage = null)
    {
        Guard.Against.Null(context);
        Guard.Against.Null(context.Responses);
        Guard.Against.Null(handler);
        if (!context.Responses.ContainsKey(handler.GetHandlerName()))
        {
            context.Responses.Add(handler.GetHandlerName(),
                new ResolutionResponse(status, message ?? GetMessage(status)));
        }
    }

    public static void AddProperty(this IResolutionContext context, string key, string value)
    {
        Guard.Against.Null(context);
        Guard.Against.Null(context.CustomProperties);
        Guard.Against.Null(key);
        Guard.Against.Null(value);
        if (!context.CustomProperties.ContainsKey(key))
        {
            context.CustomProperties.Add(key, value);
        }
        else
        {
            context.CustomProperties[key] = value;
        }
    }

    public static void AddMetric(this IResolutionContext context, string key, double value)
    {
        Guard.Against.Null(context);
        Guard.Against.Null(context.Metrics);
        Guard.Against.Null(key);
        if (!context.Metrics.ContainsKey(key))
        {
            context.Metrics.Add(key, value);
        }
        else
        {
            context.Metrics[key] = value;
        }
    }

    private static string GetMessage(ResolutionStatus status) =>
        status switch
        {
            ResolutionStatus.Success => "Action has run successfully",
            ResolutionStatus.Failed => "Action has failed",
            ResolutionStatus.Skipped => "Action has been skipped",
            _ => throw new ArgumentOutOfRangeException(nameof(status)),
        };

    public static ResolutionStatus GetResolutionStatus(this bool result)
    {
        return result ? ResolutionStatus.Success : ResolutionStatus.Failed;
    }

    public static string GetHandlerName<TRequest>(this IResolutionHandler<TRequest> handler)
    {
        return handler.GetType().Name.Replace("Handler", string.Empty);
    }
}
