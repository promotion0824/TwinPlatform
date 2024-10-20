using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Logging;

namespace Willow.Rules.Test;

[TestClass]
public class LoggerEveryTests
{
	[TestMethod]
	public void CanUseLoggerEvery()
	{
		var logger = new Moq.Mock<ILogger>();

		var logEvery = new LoggerEvery(logger.Object, TimeSpan.FromMilliseconds(100));

		DateTimeOffset start = DateTimeOffset.Now;
		int count = 0;
		while (DateTimeOffset.Now < start.AddSeconds(5))
		{
			count++;
			logEvery.LogDebug("{date}", DateTimeOffset.Now.Second);
		}

		logger.Invocations.Count.Should().BeGreaterOrEqualTo(48);
		logger.Invocations.Count.Should().BeLessOrEqualTo(50);
		count.Should().BeGreaterThan(1000);  // 6465052
	}

	[TestMethod]
	public void CanUseTimingBlock()
	{
		var logger = new ConsoleLogger();

		using (var timedlog = logger.TimeOperation("Test log {id}", "someid"))
		{
			Thread.Sleep(100);
		}

		logger.Invocations.Should().Be(2);  // One for starting, one for completion
	}
}

public class ConsoleLogger : ILogger
{
	public int Invocations { get; private set; }

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
	{
		throw new NotImplementedException();
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return true;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		Invocations++;
		Debug.WriteLine(formatter(state, exception));
	}
}
