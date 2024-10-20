using Willow.Rules.Model;
using Willow.Rules.Repository;

// POCO class, serialized to DB
#nullable disable

namespace RulesEngine.Web;

/// <summary>
/// A container object for ml model binary 
/// </summary>
public class MLModelDto : IId
{
	/// <summary>
	/// Constructor
	/// </summary>
	public MLModelDto(MLModel model, string error)
	{
		Id = model.Id;
		FullName = model.FullName;
		ModelName = model.ModelName;
		ModelVersion = model.ModelVersion;
		Description = model.Description;
        InputParams = model.ExtensionData.InputParams ?? new MLModelParam[0];
        OutputParams = model.ExtensionData.OutputParams ?? new MLModelParam[0];
        Error = error;
    }

	/// <summary>
	/// Constructor for serialization
	/// </summary>
	public MLModelDto()
	{

	}

	/// <summary>
	/// The id for the model
	/// </summary>
	public string Id { get; init; }

	/// <summary>
	/// The name of the model
	/// </summary>
	public string ModelName { get; set; }

	/// <summary>
	/// The description of the model
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// The version of the model
	/// </summary>
	public string ModelVersion { get; set; }

	/// <summary>
	/// The full name of the model
	/// </summary>
	public string FullName { get; set; }

    /// <summary>
    /// Input params for model
    /// </summary>
    public MLModelParam[] InputParams { get; set; }

    /// <summary>
    /// Output params for model
    /// </summary>
    public MLModelParam[] OutputParams { get; set; }

    /// <summary>
    /// Model load errors if any
    /// </summary>
    public string Error { get; set; }
}
