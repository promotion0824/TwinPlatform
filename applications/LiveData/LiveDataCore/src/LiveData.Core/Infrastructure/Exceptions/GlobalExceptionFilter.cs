namespace Willow.Infrastructure.Exceptions
{
    using System;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    internal class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void OnException(ExceptionContext context)
        {
            ErrorResponse response;
            switch (context.Exception)
            {
                case BadRequestException badRequest:
                    response = new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.BadRequest,
                        Message = badRequest.Message,
                    };
                    break;

                case ResourceNotFoundException resourceNotFound:
                    response = new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.NotFound,
                        Message = resourceNotFound.Message,
                        Data = new { resourceNotFound.ResourceType, resourceNotFound.ResourceId },
                    };
                    break;

                case DependencyServiceFailureException dependencyServiceFailure:
                    response = new ErrorResponse
                    {
                        StatusCode = (int)dependencyServiceFailure.ServiceStatusCode,
                        Message = dependencyServiceFailure.Message,
                        Data = new { dependencyServiceFailure.ServiceName },
                    };
                    break;

                default:
                    var exception = context.Exception;
                    response = new ErrorResponse
                    {
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        Message = exception.Message,
                        Data = new { ExceptionType = exception.GetType().ToString() },
                        CallStack = exception.ToString().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries),
                    };
                    break;
            }

            logger.LogWarning(context.Exception, "GlobalExceptionFilter caught an exception. {errorResponse}", JsonConvert.SerializeObject(response));
            context.Result = new ObjectResult(response)
            {
                StatusCode = response.StatusCode,
                ContentTypes = new MediaTypeCollection() { "application/problem+json" },
                DeclaredType = typeof(ErrorResponse),
            };
        }
    }
}
