using System;
using System.Collections.Generic;
using System.Net;

using Xunit;
using Moq;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

using Willow.Api.Exceptions;
using Willow.Common;
using Willow.Logging;

namespace Willow.Api.Exceptions.UnitTests
{
    public class GlobalExceptionFilterTests
    {
        private readonly Mock<ILogger<GlobalExceptionFilter>> _logger = new Mock<ILogger<GlobalExceptionFilter>>();
        private readonly GlobalExceptionFilter _filter;

        public GlobalExceptionFilterTests()
        {
            _filter = new GlobalExceptionFilter(_logger.Object);
        }

        [Fact]
        public void GlobalExceptionFilter_HandleException_ArgumentException()
        {
            var result = _filter.HandleException(new ArgumentException("myParam"));

            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
           // Assert.Equal("", result.Value);

            _logger.Verify( log=> log.Log(LogLevel.Warning,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Once);

            _logger.Verify( log=> log.Log(LogLevel.Error,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Never);

        }

        [Fact]
        public void GlobalExceptionFilter_HandleException_ArgumentNullException()
        {
            var result = _filter.HandleException(new ArgumentNullException("myParam"));

            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
           // Assert.Equal("", result.Value);

            _logger.Verify( log=> log.Log(LogLevel.Warning,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Once);

            _logger.Verify( log=> log.Log(LogLevel.Error,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Never);

        }

        [Fact]
        public void GlobalExceptionFilter_HandleException_NotFoundException()
        {
            var result = _filter.HandleException(new NotFoundException("myParam"));

            Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
           // Assert.Equal("", result.Value);

            _logger.Verify( log=> log.Log(LogLevel.Error,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Never);

            _logger.Verify( log=> log.Log(LogLevel.Warning,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Never);

        }

        [Fact]
        public void GlobalExceptionFilter_HandleException_UnauthorizedAccessException()
        {
            var result = _filter.HandleException(new UnauthorizedAccessException("myParam"));

            Assert.Equal((int)HttpStatusCode.Forbidden, result.StatusCode);
           // Assert.Equal("", result.Value);

            _logger.Verify( log=> log.Log(LogLevel.Warning,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Once);

            _logger.Verify( log=> log.Log(LogLevel.Error,
                                          It.IsAny<EventId>(),
                                          It.IsAny<It.IsAnyType>(),
                                          It.IsAny<Exception>(),
                                          (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), Times.Never);

        }
    }
}