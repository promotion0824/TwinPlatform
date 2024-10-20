namespace WillowRules.Configuration;

#pragma warning disable CS8618 // Nullable fields in DTO

/// <summary>
/// A single willow service that we want to call across to
/// </summary>
/// <remarks>
/// This is likely in the same K8s namespace like http://foo-svc but may be somewhere else
/// on a VNet
/// </remarks>
public class WillowService
{
	/// <summary>
	/// The base Uri for the service, e.g. http://foo-svc
	/// </summary>
	public string BaseUri { get; set; }

	/// <summary>
	/// The audience for authenticated calls to other services
	/// </summary>
	/// <example>
	/// ebb53e69-b5be-454d-928e-a2e69cdcdfc7
	/// </example>
	public string TokenAudience { get; set; }
}

#pragma warning restore CS8618 // Nullable fields in DTO
