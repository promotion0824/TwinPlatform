namespace Willow.Model.Responses;

/// <summary>
/// Twin Validation Exception
/// </summary>
public class TwinValidationException : Exception
{
    public string TwinId { get; }

    public TwinValidationException(string twinId, List<string> errorMessages)
        : base(message: string.Join(Environment.NewLine, errorMessages))
    {
        TwinId = twinId;
    }
}
