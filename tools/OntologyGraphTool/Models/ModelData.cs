using Azure.DigitalTwins.Core;
using Newtonsoft.Json;
using System;
using System.Diagnostics;

// Poco classes
#nullable disable

namespace OntologyGraphTool.Models;

/// <summary>
/// A copy of DigitalTwinsModelData
/// </summary>
/// <remarks>
/// MSFT class doesn't have a public parameterless constructor so somewhat useless for deserialization
/// </remarks>
[DebuggerDisplay("{Id}")]
public class ModelData : IEquatable<ModelData>
{
    public ModelData(DigitalTwinsModelData value)
    {
        this.Id = value.Id;
        this.Decommissioned = value.Decommissioned;
        this.UploadedOn = value.UploadedOn;
        this.LanguageDescriptions = value.LanguageDescriptions;
        this.LanguageDisplayNames = value.LanguageDisplayNames;
        this.DtdlModel = JsonConvert.DeserializeObject<DtdlModel>(value.DtdlModel);
    }

    /// <summary>
    /// For deserialization
    /// </summary>
    public ModelData()
    {
    }

    //
    // Summary:
    //     The model definition that conforms to Digital Twins Definition Language (DTDL)
    //     v2.
    public DtdlModel DtdlModel { get; set; }
    //
    // Summary:
    //     The date and time the model was uploaded to the service.
    public DateTimeOffset? UploadedOn { get; set; }

    ///<summary>
    ///     A language dictionary that contains the localized display names as specified
    ///     in the model definition.
    ///</summary>
    public IReadOnlyDictionary<string, string> LanguageDisplayNames { get; set; }

    ///<summary>
    ///     A language dictionary that contains the localized descriptions as specified in
    ///     the model definition.
    ///</summary>
    public IReadOnlyDictionary<string, string> LanguageDescriptions { get; set; }

    //
    // Summary:
    //     The id of the model as specified in the model definition.
    [JsonProperty("id")]
    public string Id { get; set; }
    //
    // Summary:
    //     Indicates if the model is decommissioned. Decommissioned models cannot be referenced
    //     by newly created digital twins.
    public bool? Decommissioned { get; set; }

    /// <summary>
    /// IEquatable Equals implementation
    /// </summary>
    public bool Equals(ModelData other) => other is not null && this.Id.Equals(other.Id);

    /// <summary>
    /// Computed label using one of the display names, preferably english
    /// </summary>
    [JsonIgnore]
    public string Label =>
            this.DtdlModel.displayName?.en ??
                this.LanguageDisplayNames.Select(x => x.Value).FirstOrDefault() ??
                this.DtdlModel?.description?.en ??
                this.DtdlModel?.id ??
                $"Missing name ({this!.Id})";

    public override bool Equals(object obj)
    {
        return Equals(obj as ModelData);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Id);
    }
}

public class Schema
{
    public string type { get; set; }
    public Mapkey mapKey { get; set; }
    public Mapvalue mapValue { get; set; }

    public static implicit operator Schema(string text)
        => new Schema { type = text, mapKey = null, mapValue = null };

    public override string ToString() => String.IsNullOrEmpty(type) ?
        (String.IsNullOrEmpty(mapKey?.name) ? "null" :
        $"{mapKey?.name} : {mapValue.name}") : type;
}

public class Mapkey { public string name { get; set; } public string schema { get; set; } }
public class Mapvalue { public string name { get; set; } public Schema schema { get; set; } }

/// <summary>
/// Sometimes it's a list, sometimes it's a value
/// </summary>
public class StringList : List<string>
{
    public StringList() { }
    public StringList(params string[] values)
    {
        this.AddRange(values);
    }

    public static implicit operator StringList(string text) => new StringList(text);
    public static implicit operator StringList(string[] text) => new StringList(text);

    public override string ToString() => $"[{string.Join(",", this)}]";
}

public class TextLang
{
    public string en { get; set; }

    public static implicit operator TextLang(string text) => new TextLang { en = text };

    public override string ToString() => this.en;
}
