using System;
using Polly;
using Polly.Retry;

namespace Willow.Rules.Services;

/// <summary>
/// Polly retry policies
/// </summary>
public interface IRetryPolicies
{
	/// <summary>
	/// Retry policy for ADT requests
	/// </summary>
	AsyncRetryPolicy ADTRetryPolicy { get; }
}

/// <summary>
/// Polly retry policies
/// </summary>
public class RetryPolicies : IRetryPolicies
{
	/// <inheritdoc />
	public AsyncRetryPolicy ADTRetryPolicy => Policy
		.Handle<Azure.RequestFailedException>()
		//.Handle<Exception>()  // TODO: Specific ADT Exception
		.WaitAndRetryAsync(3, retryAttempt =>
			TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
		);
}