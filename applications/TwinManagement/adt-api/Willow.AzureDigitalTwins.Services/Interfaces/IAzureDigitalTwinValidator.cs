using Azure.DigitalTwins.Core;

namespace Willow.AzureDigitalTwins.Services.Interfaces;
public interface IAzureDigitalTwinValidator
{
    /// <summary>
    /// Validate Twin Content
    /// </summary>
    /// <param name="basicDigitalTwin">Instance of  <see cref="BasicDigitalTwin"/></param>
    /// <param name="errors">List of validation error messages.</param>
    /// <returns>Awaitable Task.returns>
    Task<bool> ValidateTwinAsync(BasicDigitalTwin basicDigitalTwin, out List<string> errors);
}
