using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Willow.ExceptionHandling.Exceptions;

namespace Willow.ExceptionHandling;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public virtual void OnException(ExceptionContext context)
    {
        var statusCode = StatusCodes.Status500InternalServerError;

        switch (context.Exception)
        {
            case UnauthorizedAccessException:
            case ForbiddenException:
                statusCode = StatusCodes.Status403Forbidden;
                break;

            case ArgumentException:
            case BadRequestException:
                statusCode = StatusCodes.Status400BadRequest;
                break;

            case NotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                break;

            case RequestFailedException requestFailedException:
                _logger.LogError(context.Exception, "{Message}", context.Exception.Message);
                statusCode = requestFailedException.Status;
                break;

            case HttpRequestException httpRequestException:
                if (httpRequestException.StatusCode.HasValue)
                    statusCode = (int)httpRequestException.StatusCode;
                break;

            case FileParseException:
            case FileContentException:
            case GitContentException:
            case KeyNotFoundException:
                statusCode = StatusCodes.Status422UnprocessableEntity;
                break;

            case BadResponseException:
                statusCode = StatusCodes.Status424FailedDependency;
                break;

            case DependencyServiceFailureException dependencyServiceFailure:
                _logger.LogWarning(dependencyServiceFailure,
                    "{Name} caught a DependencyServiceFailureException from {ServiceName} service.",
                    nameof(GlobalExceptionFilter), dependencyServiceFailure.ServiceName);
                statusCode = (int)dependencyServiceFailure.ServiceStatusCode;
                break;

            default:
                _logger.LogError(context.Exception, "GlobalExceptionFilter caught an unknown exception.");
                break;
        }

        WriteErrorResponse(context, statusCode);
    }

    protected static void WriteErrorResponse(ExceptionContext context, int statusCode)
    {
        var response = new ErrorResponse
        {
            StatusCode = statusCode,
            Message = context.Exception.Message,
            Data = context.Exception.Data
        };

        context.Result = new ObjectResult(response)
        {
            StatusCode = response.StatusCode,
            ContentTypes = new MediaTypeCollection
            {
                "application/problem+json"
            },
            DeclaredType = typeof(ErrorResponse)
        };
    }
}
