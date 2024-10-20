namespace Willow.CommandAndControl.Application.Models;

using System.ComponentModel.DataAnnotations.Schema;
using DTDLParser;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
/// Represents a location twin.
/// </summary>
public record LocationTwin
{
    /// <summary>
    /// Gets the twin ID.
    /// </summary>
    /// <remarks>
    /// EF Core gets upset if we suffix properties with ID and don't define a primary key.
    /// </remarks>
    [JsonPropertyName("TwinId")]
    public required string TwinName { get; init; }

    /// <summary>
    /// Gets the location model ID.
    /// </summary>
    /// <remarks>
    /// EF Core gets upset if we suffix properties with ID and don't define a primary key.
    /// </remarks>
    [JsonPropertyName("LocationModelId")]
    public required string LocationModelName { get; init; }

    /// <summary>
    /// Gets the location twin ID.
    /// </summary>
    /// <remarks>
    /// EF Core gets upset if we suffix properties with ID and don't define a primary key.
    /// </remarks>
    [JsonPropertyName("LocationTwinId")]
    public required string LocationTwinName { get; init; }

    /// <summary>
    /// Gets the location order.
    /// </summary>
    public required int Order { get; init; }

    /// <summary>
    /// Gets the last part of the model name.
    /// </summary>
    [NotMapped]
    public string Model => string.IsNullOrEmpty(LocationModelName) ? LocationModelName : new Dtmi(LocationModelName.ToString(), true).Labels.Last();
}

internal class StringToDtmi : ValueConverter<Dtmi, string>
{
    public StringToDtmi(Expression<Func<Dtmi, string>> convertToProviderExpression, Expression<Func<string, Dtmi>> convertFromProviderExpression, ConverterMappingHints? mappingHints = null)
        : base(convertToProviderExpression, convertFromProviderExpression, mappingHints)
    {
    }
}
