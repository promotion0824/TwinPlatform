using Abodit.Graph;
using Abodit.Mutable;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Willow.Rules.Model;

namespace WillowRules.Extensions;

public static class TwinExtensions
{
	/// <summary>
	/// Retrieves property usage (left-join style) between a twin and it's model schemas (inherited)
	/// </summary>
	public static IEnumerable<(JObject? token, string propertyPath, Content property, bool used, ModelData model)> GetPropertyUsage(
	   this BasicDigitalTwinPoco twin,
	   Graph<ModelData, Abodit.Graph.Relation> ontology,
	   Dictionary<string, ModelData> modelLookup)
	{
		return GetPropertyUsage(JObject.FromObject(twin.Contents), "", twin.ModelId(), ontology, modelLookup);
	}

	private static IEnumerable<(JObject? token, string propertyPath, Content property, bool used, ModelData model)> GetPropertyUsage(
	   JObject? token,
	   string propertyPath,
	   string modelId,
	   Graph<ModelData, Abodit.Graph.Relation> ontology,
	   Dictionary<string, ModelData> modelLookup)
	{
		if (modelLookup.TryGetValue(modelId, out var modelNode))
		{
			var successors = ontology.Successors<ModelData>(modelNode, Relation.RDFSType);

			foreach (var model in successors)
			{
				foreach (var content in model.GetProperties())
				{
					var property = content.name;

					JToken? propertyToken = null;

					bool isUsed = token is not null && token.TryGetValue(property, out propertyToken);


					if (content.type.Contains("Component"))
					{
						string subPath = string.IsNullOrEmpty(propertyPath) ? property : $"{propertyPath}.{property}";

						foreach (var item in GetPropertyUsage(propertyToken as JObject, subPath, content.schema.type, ontology, modelLookup))
						{
							yield return item;
						}
					}

					yield return (token, propertyPath, content, isUsed, model);
				}
			}
		}
	}
}
