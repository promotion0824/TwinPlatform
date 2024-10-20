using System.Text.Json.Serialization;
using DTDLParser.Models;
using Willow.Model.Responses;

namespace Willow.TwinLifecycleManagement.Web.Models;

/// <summary>
/// ModelsTwinInfo Record.
/// </summary>
public record ModelsTwinInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelsTwinInfo"/> class.
    /// </summary>
    /// <param name="modelResponse">Model Response.</param>
    public ModelsTwinInfo(ModelResponse modelResponse)
    {
        Id = modelResponse.Id;
        Name = modelResponse.DisplayName.GetValueOrDefault("en");
        ExactCount = modelResponse.TwinCount?.Exact ?? 0;
        TotalCount = modelResponse.TwinCount?.Total ?? 0;
        Description = modelResponse.Description.GetValueOrDefault("en");
        UploadTime = modelResponse.UploadTime;
        Model = modelResponse.Model;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelsTwinInfo"/> class.
    /// </summary>
    public ModelsTwinInfo()
    {
    }

    /// <summary>
    /// Gets or sets the Id.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the exact count.
    /// </summary>
    public int ExactCount { get; set; }

    /// <summary>
    /// Gets or sets the total count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the upload time.
    /// </summary>
    public DateTimeOffset? UploadTime { get; set; }

    /// <summary>
    /// Gets or sets the model.
    /// </summary>
    public string Model { get; set; }
}

/// <summary>
/// Interface Twins Info.
/// </summary>
public record InterfaceTwinsInfo : ModelsTwinInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InterfaceTwinsInfo"/> class.
    /// </summary>
    public InterfaceTwinsInfo()
        : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InterfaceTwinsInfo"/> class.
    /// </summary>
    /// <param name="modelResponse">Model Response.</param>
    /// <param name="entityInfo">Entity Info.</param>
    public InterfaceTwinsInfo(ModelResponse modelResponse, DTEntityInfo entityInfo)
        : base(modelResponse)
    {
        EntityInfo = entityInfo as DTInterfaceInfo;
    }

    /// <summary>
    /// Gets the Entity Info.
    /// </summary>
    [JsonIgnore]
    public DTInterfaceInfo EntityInfo { get; internal set; }
}
