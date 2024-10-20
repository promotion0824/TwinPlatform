using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Willow.Rules.Configuration;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Sources;
using Willow.Rules.Test;

namespace WillowRules.Test;

public static class MockObjects
{
	private static Lazy<WillowEnvironment> willowEnvironment = new Lazy<WillowEnvironment>(() => CreateWillowEnvironment());

	public static WillowEnvironment WillowEnvironment => willowEnvironment.Value;

	public static IOptions<CustomerOptions> CustomerOptions
	{
		get
		{
			return Options.Create(new CustomerOptions()
			{
				Id = "test",
				Execution = new ExecutionOption()
				{
					RunFrequency = TimeSpan.FromMinutes(15),
					SettlingInterval = TimeSpan.FromMinutes(15),
				}
			});
		}
	}

	private static WillowEnvironment CreateWillowEnvironment()
	{
		return new WillowEnvironment(
			new CustomerOptions()
			{
				SQL = new SqlOption()
			});
	}

	private class LoggerMock<T> : ILogger<T>
	{
		private ILogger<T> logger;

		public LoggerMock(ILogger<T> logger)
		{
			this.logger = logger;
		}

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull
		{
			return logger.BeginScope(state);
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			return logger.IsEnabled(logLevel);
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		{
			if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
			{
				throw new Exception("Errors not expected: " + formatter(state, exception));
			}

			logger.Log(logLevel, eventId, state, exception, formatter);
		}
	}

	public static ILogger<T> GetLogger<T>(ILogger<T> logger)
	{
		return new LoggerMock<T>(logger);
	}

	public static ILogger<T> GetLogger<T>()
	{
		return GetLogger(Mock.Of<ILogger<T>>());
	}
}
