// #pragma warning disable CS8618 // Nullable fields in DTO

// using System.Collections.Generic;

// namespace WillowRules.DTO;

// /// <summary>
// /// Inheritance graph to hand back to UI
// /// </summary>
// public class InheritanceGraphDto
// {
// 	public ModelDto[] Nodes { get; set; }

// 	public InheritanceRelationshipDto[] Relationships { get; set; }
// }

// /// <summary>
// /// Model as part of inheritance graph
// /// </summary>
// public class ModelDto
// {
// 	/// <summary>
// 	/// Model id
// 	/// </summary>
// 	public string Id { get; set; }

// 	//
// 	// Summary:
// 	//     A language dictionary that contains the localized display names as specified
// 	//     in the model definition.
// 	public IReadOnlyDictionary<string, string> LanguageDisplayNames { get; set; }
// 	//
// 	// Summary:
// 	//     A language dictionary that contains the localized descriptions as specified in
// 	//     the model definition.
// 	public IReadOnlyDictionary<string, string> LanguageDescriptions { get; set; }

// 	/// <summary>
// 	/// Count without any inheritance
// 	/// </summary>
// 	public int CountActual { get; set; }

// 	/// <summary>
// 	/// Counts instances of model and all descendant models that inherit from it
// 	/// </summary>
// 	public int CountInherited { get; set; }

// 	/// <summary>
// 	/// List of ancestor models
// 	/// </summary>
// 	public string[] Ancestors { get; set; }
// }

// /// <summary>
// /// An inheritance relationship between two models
// /// </summary>
// public class InheritanceRelationshipDto
// {
// 	/// <summary>
// 	/// Id of start node
// 	/// </summary>
// 	public string StartId { get; set; }

// 	/// <summary>
// 	/// Id of end node
// 	/// </summary>
// 	public string EndId { get; set; }

// 	/// <summary>
// 	/// Name of relationship
// 	/// </summary>
// 	public string Relationship { get; set; }
// }

// #pragma warning restore CS8618 // Nullable fields in DTO
