using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Willow.Api.Client;
using Willow.ExceptionHandling;
using Willow.ExceptionHandling.Exceptions;

namespace Willow.Infrastructure.Exceptions
{

    public class RestExceptionFilter : GlobalExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public RestExceptionFilter(ILogger<GlobalExceptionFilter> logger) : base(logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is RestException restEx)
            {
                WriteErrorResponse(context, (int)restEx.StatusCode);
                return;
            }

            if (context.Exception is NotFoundException)
                _logger.LogWarning(null, context.Exception, null);

            base.OnException(context);
        }
    }
}
