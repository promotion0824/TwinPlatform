using Willow.Rules.Repository;

// POCO class, serialized to DB
#nullable disable

namespace Willow.Rules.Model;

/// <summary>
/// A container object for ml model binary 
/// </summary>
public class MLModel : IId
{
	/// <summary>
	/// Constructor
	/// </summary>
	public MLModel(
		string id,
		string fullName,
		string modelName,
		string modelVersion,
		string description,
		byte[] modelData)
	{
		Id = id;
		FullName = fullName;
		ModelName = modelName;
		ModelVersion = modelVersion;
		Description = description;
		ModelData = modelData;
	}

	/// <summary>
	/// Constructor for serialization
	/// </summary>
	public MLModel()
	{

	}
	/// <summary>
	/// The id for the model
	/// </summary>
	public string Id { get; init; }

	/// <summary>
	/// The name of the model
	/// </summary>
	public string ModelName { get; set; } = "";

	/// <summary>
	/// The description of the model
	/// </summary>
	public string Description { get; set; } = "";

	/// <summary>
	/// The version of the model
	/// </summary>
	public string ModelVersion { get; set; } = "";

	/// <summary>
	/// The full name of the model
	/// </summary>
	public string FullName { get; set; } = "";

	/// <summary>
	/// The model binary
	/// </summary>
	public byte[] ModelData { get; init; }

	/// <summary>
	/// Additional metadata
	/// </summary>
	public MLModelExtensionData ExtensionData { get; init; } = new MLModelExtensionData();
}

/// <summary>
/// Additional metdata for <see cref="MLModel"/>
/// </summary>
public class MLModelExtensionData
{
	/// <summary>
	/// Input params for model
	/// </summary>
	public MLModelParam[] InputParams { get; set; } = new MLModelParam[0];

	/// <summary>
	/// Output params for model
	/// </summary>
	public MLModelParam[] OutputParams { get; set; } = new MLModelParam[0];
}

/// <summary>
/// A input or output parameter for a <see cref="MLModel"/>
/// </summary>
public class MLModelParam
{
	/// <summary>
	/// The param name
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// The param type
	/// </summary>
	public string Unit { get; set; }

	/// <summary>
	/// The number of inputs for the paramater
	/// </summary>
	public int Size { get; set; } = 1;

	/// <summary>
	/// Param description
	/// </summary>
	public string Description { get; set; }
}
