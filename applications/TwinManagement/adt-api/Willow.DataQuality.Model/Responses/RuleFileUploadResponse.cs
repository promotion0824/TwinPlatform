namespace Willow.DataQuality.Model.Responses;


/// <summary>
/// A dictionary where is each key is the fileName that was uploaded,
/// and the value is null is there was no error, or a string error message
/// </summary>
public class RuleFileUploadResponse
{
    // TODO: rename this
    public IDictionary<string, string> FileUploaded { get; init; } = new Dictionary<string, string>();

}
