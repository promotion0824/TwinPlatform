namespace Willow.CommandAndControl.Application.Models;

/// <summary>
/// Set value request.
/// </summary>
/// <param name="Value">The value.</param>
/// <param name="TimeoutSecs">The timeout in seconds.</param>
public record SetValueRequest(string Value, int TimeoutSecs);
