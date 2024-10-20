using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;

using Willow.Common;
using Willow.Logging;

namespace Willow.Api.Exceptions
{
   public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public virtual void OnException(ExceptionContext context)
        {
            var result = HandleException(context.Exception);
            if (result is not null)
            {
                context.Result = result;
            }
        }

        public virtual ObjectResult HandleException(Exception exception)
        {
			HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

            if(exception is AuthenticationException)
            { 
    			statusCode = HttpStatusCode.Unauthorized;
            }
            else if(exception is UnauthorizedAccessException)
            { 
    		    statusCode = HttpStatusCode.Forbidden;

                _logger.LogWarning(exception.Message, exception, null);
            }
            else if(exception is ArgumentException)
            { 
			    statusCode = HttpStatusCode.BadRequest;

                _logger.LogWarning(exception.Message, exception, null);
            }
            else if(exception is NotFoundException)
            { 
		        statusCode = HttpStatusCode.NotFound;
            }
            else if(exception is FileNotFoundException fex)
            { 
		        statusCode = HttpStatusCode.NotFound;

                exception.Data.Add("FileName", fex.FileName);

                _logger.LogWarning(fex.Message, exception, null);
            }
            else
            {
                _logger.LogError(exception.Message, exception, null);
                // return null and let the exception middleware handle it
                return null;
            }

            return HandleException(exception, statusCode);
        }

        protected void HandleException(ExceptionContext context, HttpStatusCode statusCode)
        {
            context.Result = HandleException(context.Exception, statusCode);
        }

        protected ObjectResult HandleException(Exception exception, HttpStatusCode statusCode)
        {
			var response = new ErrorResponse
            {
                StatusCode	= (int)statusCode,
                Message		= exception.Message,
                Data		= exception.Data
            };

            return new ObjectResult(response)
            {
                StatusCode   = response.StatusCode,
                ContentTypes = new MediaTypeCollection { "application/problem+json" },
                DeclaredType = typeof(ErrorResponse)
            };
        }
    }
}
