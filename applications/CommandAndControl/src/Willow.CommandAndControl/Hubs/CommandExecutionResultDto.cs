namespace Willow.CommandAndControl.Hubs;

/// <summary>
/// Command execution result DTO.
/// </summary>
public record CommandExecutionResultDto
{
    /// <summary>
    /// Gets or sets the status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the message.
    /// </summary>
    public string Message { get; set; } = default!;

    /// <summary>
    /// Gets or sets the RequestBody.
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// Gets or sets the ResponseBody.
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// Gets or sets the command ID.
    /// </summary>
    public string CommandId { get; set; } = default!;
}

/// <summary>
/// Send command request DTO.
/// </summary>
public record SendCommandRequestDto
{
    /// <summary>
    /// Gets or sets the command ID.
    /// </summary>
    public required string CommandId { get; set; }

    /// <summary>
    /// Gets or sets the external ID.
    /// </summary>
    public required string ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public required double Value { get; set; }
}
