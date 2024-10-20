using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace WillowRules.Test.Bugs;

public class ConsoleScope<TState> : IDisposable
{
	private readonly TState state;

	public ConsoleScope(TState state)
	{
		this.state = state;
	}

	internal string DumpState()
	{
		if (state is Dictionary<string, object> dict)
		{
			return string.Join(",", dict.Select(x => $"{x.Key}={x.Value}"));
		}
		else
		{
			return state?.ToString() ?? "";
		}
	}

	public void Dispose()
	{
		//Console.WriteLine($"End scope {DumpState()}");
	}
}


/// <summary>
/// Console logger for unit tests
/// </summary>
/// <typeparam name="T"></typeparam>
public class ConsoleLogger<T> : ILogger<T>
{
	public int Invocations { get; private set; }

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull
	{
		var scope = new ConsoleScope<TState>(state);
		//Console.WriteLine($"Begin scope {scope.DumpState()}");
		return scope;
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		return true;
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		Invocations++;
		if (exception is Exception)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(exception.Message);
			Console.WriteLine(exception.StackTrace);
			Console.ForegroundColor = ConsoleColor.White;
		}
		Console.WriteLine(formatter(state, exception));
	}
}
