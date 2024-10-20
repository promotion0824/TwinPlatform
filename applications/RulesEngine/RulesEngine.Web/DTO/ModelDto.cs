using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;

// Poco classes
#nullable disable

namespace RulesEngine.Web;

/// <summary>
/// A copy of DigitalTwinsModelData
/// </summary>
/// <remarks>
/// MSFT class doesn't have a public parameterless constructor so somewhat useless for deserialization
/// </remarks>
public class ModelDto
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ModelDto(ModelData model, IEnumerable<ModelData> successors = null, bool loadInheritedProperties = false)
    {
        this.Id = model.Id;
        this.Decommissioned = model.Decommissioned;
        this.LanguageDescriptions = model.LanguageDescriptions;
        this.LanguageDisplayNames = model.LanguageDisplayNames;

        IEnumerable<ModelData> models = new[] { model };

        if(successors is not null)
        {
            if (loadInheritedProperties)
            {
                models = models.Union(successors);
            }

            InheritedModelIds = successors.Select(v => v.Id).ToArray();
        }

        this.Properties = ModelPropertyDto.CreateProperties(models);
    }

    ///<summary>
    /// A language dictionary that contains the localized display names as specified
    /// in the model definition.
    ///</summary>
    public IReadOnlyDictionary<string, string> LanguageDisplayNames { get; set; }

    ///<summary>
    /// A language dictionary that contains the localized descriptions as specified in
    /// the model definition.
    ///</summary>
    public IReadOnlyDictionary<string, string> LanguageDescriptions { get; set; }

    /// <summary>
    /// Model ids that this id inherits from
    /// </summary>
    public string[] InheritedModelIds { get; set; } = [];

    /// <summary>
    /// The id of the model as specified in the model definition.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Indicates if the model is decommissioned. Decommissioned models cannot be referenced by newly created digital twins.
    /// </summary>
    public bool? Decommissioned { get; set; }

    /// <summary>
    /// Model properties 
    /// </summary>
    public ModelPropertyDto[] Properties { get; set; } = [];
}

/// <summary>
/// A model property
/// </summary>
public class ModelPropertyDto
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ModelPropertyDto(string modelId, Content property)
    {
        this.ModelId = modelId;
        this.PropertyName = property.name;
        this.PropertyKey = property.name;
        this.PropertyType = property.schema?.type ?? "";
    }

    /// <summary>
    /// From which model id
    /// </summary>
    public string ModelId { get; set; }

    /// <summary>
    /// Name of property
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Name of property
    /// </summary>
    public string PropertyKey { get; set; }

    /// <summary>
    /// Property type
    /// </summary>
    public string PropertyType { get; set; }

    /// <summary>
    /// Create properties from a list of models
    /// </summary>
    /// <returns></returns>
    public static ModelPropertyDto[] CreateProperties(IEnumerable<ModelData> models)
    {
        return models.SelectMany(m =>
            m.GetProperties()
            .Select(v => new ModelPropertyDto(m.Id, v)))
            .DistinctBy(v => v.PropertyName)
            .ToArray();
    }
}
