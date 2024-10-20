using System.Net;

namespace Willow.IoTService.WebApiErrorHandling.Contracts;

/// <summary>
///     Use this exception when doing delayed property checks outside of validators.
/// </summary>
public class RequestInvalidException(string? message) : RequestBaseException(message, (int)HttpStatusCode.BadRequest);
