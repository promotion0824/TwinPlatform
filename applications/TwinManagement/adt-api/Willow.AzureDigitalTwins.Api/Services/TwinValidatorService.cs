using Azure.DigitalTwins.Core;
using DTDLParser;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Services.Interfaces;

namespace Willow.AzureDigitalTwins.Api.Services;

/// <summary>
/// Implementation of Azure Digital Twin Validator Service.
/// </summary>
public partial class TwinValidatorService() : IAzureDigitalTwinValidator
{
    private static string FormatErrorMessage(string fieldName, string value, string message) => $"Field : {fieldName} Value: {value} Message: {message}";

    private static readonly string[] twinIDFields = ["siteID", "floorID", "uniqueID", "geometryViewerID", "trendID", "connectorID"];

    /// <summary>
    /// Validate Twin Data
    /// </summary>
    /// <param name="basicDigitalTwin">Instance of Basic Digital Twin.</param>
    /// <param name="errors">List of validation errors.</param>
    /// <returns>True if valid, false if not.</returns>
    public Task<bool> ValidateTwinAsync(BasicDigitalTwin basicDigitalTwin, out List<string> errors)
    {
        errors = [];
  
        // Validate Model Id Format
        if (!Dtmi.TryCreateDtmi(basicDigitalTwin.Metadata.ModelId, out _))
        {
            errors.Add(FormatErrorMessage(nameof(basicDigitalTwin.Metadata.ModelId), basicDigitalTwin.Metadata.ModelId, "Invalid Model Id Format."));
        }

        // Validate Id Fields
        foreach (var field in twinIDFields)
        {
            if (basicDigitalTwin.Contents.TryGetValue(field, out var value)) // has the field value
            {
                // check if the field is in GUID format
                if (value is not null && !Guid.TryParse(value.ToString(), out Guid result))
                {
                    errors.Add(FormatErrorMessage(field, value.ToString(), "Invalid GUID Format."));
                }
            }
        }

        return Task.FromResult(errors.Count > 0);
    }

}
