using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

namespace Willow.Infrastructure.Exceptions
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            ErrorResponse response;
            switch (context.Exception)
            {
                case ApiException apiException:
                    response = new ErrorResponse
                    {
                        StatusCode = (int)apiException.StatusCode,
                        Message = apiException.Message,
                        Data = apiException.AdditionalData
                    };
                    _logger.LogWarning(
                        apiException,
                        "GlobalExceptionFilter caught a known exception. {ErrorResponse}",
                        JsonSerializerExtensions.Serialize(response)
                    );
                    break;

                default:
                    var exception = context.Exception;
                    _logger.LogError(exception, "GlobalExceptionFilter caught an exception.");
                    response = new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        Message = exception.Message,
                        Data = new { ExceptionType = exception.GetType().ToString() },
                        CallStack = exception
                            .ToString()
                            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    };
                    break;
            }
            context.Result = new ObjectResult(response)
            {
                StatusCode = response.StatusCode,
                ContentTypes = new MediaTypeCollection { "application/problem+json" },
                DeclaredType = typeof(ErrorResponse)
            };
        }
    }
}
