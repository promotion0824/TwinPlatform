using System;
using System.IO;
using System.Collections.Generic;
using Xunit;

using Microsoft.Extensions.Logging;

using Willow.Common;
using Willow.Logging;

namespace Willow.Logging.UnitTests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LoggerExtensions_LogError()
        {
            var logger = new FakeLogger();
            var ex = new Exception("This is also an error");

            ex.Data["Method"] = "GetFoo";

            logger.LogError("This is an error", ex, new { Class = "AssetCore" });

            Assert.Equal("This is an error", logger.Message);
            Assert.Equal("Error",            logger.Properties["Log Level"]);
            Assert.Equal("AssetCore",        logger.Properties["Class"]);
            Assert.Equal("GetFoo",           logger.Properties["Method"]);

            logger.LogInformation("This is an error", new { Class = "AssetCore" });

            Assert.Equal("This is an error", logger.Message);
            Assert.Equal("Information",      logger.Properties["Log Level"]);
            Assert.Equal("AssetCore",        logger.Properties["Class"]);
        }

        [Fact]
        public void LoggerExtensions_LogError_innerexception()
        {
            var logger = new FakeLogger();
            var exInner = new Exception("This is an inner exception");

            exInner.Data["Subject"] = "Email";

            var ex = new Exception("This is also an error", exInner);

            ex.Data["Method"] = "GetFoo";

            logger.LogError("This is an error", ex, new { Class = "AssetCore" });

            Assert.Equal("This is an error", logger.Message);
            Assert.Equal("Error",            logger.Properties["Log Level"]);
            Assert.Equal("AssetCore",        logger.Properties["Class"]);
            Assert.Equal("GetFoo",           logger.Properties["Method"]);
            Assert.Equal("Email",            logger.Properties["Subject"]);

            logger.LogInformation("This is an error", new { Class = "AssetCore" });

            Assert.Equal("This is an error", logger.Message);
            Assert.Equal("Information",      logger.Properties["Log Level"]);
            Assert.Equal("AssetCore",        logger.Properties["Class"]);
        }

        public class FakeLogger : ILogger
        {
            public string Message { get; set; } 
            public IDictionary<string, object> Properties { get; set; } 

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                throw new NotImplementedException();
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                this.Properties = state.ToDictionary();
                this.Message    = formatter(state, exception);
            }
        }
    }
}
