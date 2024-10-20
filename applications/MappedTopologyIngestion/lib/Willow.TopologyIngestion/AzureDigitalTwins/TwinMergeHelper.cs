// -----------------------------------------------------------------------
// <copyright file="TwinMergeHelper.cs" Company="Willow">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.TopologyIngestion.AzureDigitalTwins
{
    using System.Text.Json;
    using Azure.DigitalTwins.Core;
    using Microsoft.AspNetCore.JsonPatch;

    /// <summary>
    /// Helper class doing twin merging/manipulation for <see cref="AdtApiGraphManager{TOptions}"/>.
    /// </summary>
    internal class TwinMergeHelper
    {
        /// <summary>
        /// Tries to generate a JSON Patch document translating an existing digital twin object to a target representation.
        /// </summary>
        /// <param name="existingDigitalTwin">The source twin.</param>
        /// <param name="newTwin">The target twin.</param>
        /// <param name="jsonPatchDocument">Generated JSON Patch document.</param>
        /// <returns><c>true</c> if a JSON Patch could be created, else <c>false</c>.</returns>
        internal static bool TryCreatePatchDocument(BasicDigitalTwin existingDigitalTwin, BasicDigitalTwin newTwin, out JsonPatchDocument jsonPatchDocument)
        {
            jsonPatchDocument = new JsonPatchDocument();

            // If the models don't match, then replace the metadata for the model id
            if (existingDigitalTwin.Metadata.ModelId != newTwin.Metadata.ModelId)
            {
                jsonPatchDocument.Replace("/$metadata/$model", newTwin.Metadata.ModelId);
            }

            foreach (var propertyName in newTwin.Contents.Keys)
            {
                var newPropertyJson = JsonSerializer.SerializeToElement(newTwin.Contents[propertyName]);

                // See if the existing twin has the property
                if (existingDigitalTwin.Contents.TryGetValue(propertyName, out var existingPropertyValue))
                {
                    var existingPropertyJson = JsonSerializer.SerializeToElement(existingPropertyValue);

                    GetUpdates(jsonPatchDocument, "/" + propertyName, newPropertyJson, existingPropertyJson);
                }
                else
                {
                    // This is a new property
                    jsonPatchDocument.Add("/" + propertyName, newPropertyJson);
                }
            }

            return jsonPatchDocument.Operations.Any();
        }

        /// <summary>
        /// Tries to generate a JSON Patch document translating an existing relationship object to a target representation.
        /// </summary>
        /// <param name="existingRelationship">The source relationship.</param>
        /// <param name="newRelationship">The target relationship.</param>
        /// <param name="jsonPatchDocument">Generated JSON Patch document.</param>
        /// <returns><c>true</c> if a JSON Patch could be created, else <c>false</c>.</returns>
        internal static bool TryCreatePatchDocument(BasicRelationship existingRelationship, BasicRelationship newRelationship, out JsonPatchDocument jsonPatchDocument)
        {
            jsonPatchDocument = new JsonPatchDocument();

            foreach (var newProperty in newRelationship.Properties)
            {
                var newPropertyName = newProperty.Key;
                var newPropertyValue = newProperty.Value;

                // See if the existing relationship has the property
                if (existingRelationship.Properties.TryGetValue(newPropertyName, out var existingPropertyValue))
                {
                    if (existingPropertyValue != null && existingPropertyValue != newPropertyValue)
                    {
                        jsonPatchDocument.Replace("/" + newPropertyName, newPropertyValue);
                    }
                }
                else
                {
                    jsonPatchDocument.Add("/" + newPropertyName, newPropertyValue);
                }
            }

            return jsonPatchDocument.Operations.Any();
        }

        private static void GetUpdates(JsonPatchDocument jsonPatchDocument, string propertyName, JsonElement newPropertyJson, JsonElement existingPropertyJson)
        {
            // If the kinds are the same, keep checking to see if there are other equalities
            if (newPropertyJson.ValueKind == existingPropertyJson.ValueKind)
            {
                switch (newPropertyJson.ValueKind)
                {
                    case JsonValueKind.String:
                        {
                            // Verify that the property value has changed
                            if (string.Compare(existingPropertyJson.ToString(), newPropertyJson.ToString(), StringComparison.OrdinalIgnoreCase) != 0)
                            {
                                jsonPatchDocument.Replace(propertyName, newPropertyJson.ToString());
                            }

                            break;
                        }

                    case JsonValueKind.Number:
                        {
                            // Verify that the property value has changed
                            if (existingPropertyJson.ToString() != newPropertyJson.ToString())
                            {
                                // See why we always try decimal first, here: https://stackoverflow.com/questions/72597489/what-is-the-idiomatic-way-to-find-the-underlying-type-of-a-jsonelement-value
                                if (newPropertyJson.TryGetDecimal(out var result))
                                {
                                    jsonPatchDocument.Replace(propertyName, result);
                                }
                                else
                                {
                                    jsonPatchDocument.Replace(propertyName, newPropertyJson.ToString());
                                }
                            }

                            break;
                        }

                    case JsonValueKind.Object:
                        {
                            foreach (var prop in newPropertyJson.EnumerateObject())
                            {
                                // Skip the metadata property
                                if (prop.Name != "$metadata")
                                {
                                    if (existingPropertyJson.TryGetProperty(prop.Name, out var outProp))
                                    {
                                        GetUpdates(jsonPatchDocument, propertyName + "/" + prop.Name, prop.Value, outProp);
                                    }
                                }
                            }
                        }

                        break;

                    case JsonValueKind.Array:
                        {
                            var replaceArray = false;

                            // This is a really simplified implementation, assuming all proprties in the array are strings and that if a record is not found in the
                            // existing array, replace the entire array. This is not a good implementation for large arrays.
                            // Loop through the collection of records in the new list
                            foreach (var newArrayItem in newPropertyJson.EnumerateArray())
                            {
                                var newHash = string.Empty;

                                // We don't know what the key is in the array, so we can only add to the existing array if any fields have changed.
                                foreach (var property in newArrayItem.EnumerateObject())
                                {
                                    newHash += property.Value.ToString();
                                }

                                var recordFound = false;

                                foreach (var existingArrayItem in existingPropertyJson.EnumerateArray())
                                {
                                    var existingHash = string.Empty;

                                    foreach (var property in existingArrayItem.EnumerateObject())
                                    {
                                        existingHash += property.Value.ToString();
                                    }

                                    // If the hashes match, the record already exists in the array. No need to add the new one
                                    if (existingHash == newHash)
                                    {
                                        recordFound = true;
                                        break;
                                    }
                                }

                                if (recordFound)
                                {
                                    // Go to next entry in the array
                                    continue;
                                }

                                // If any new record was not found, we need to replace the array
                                replaceArray = true;
                                break;
                            }

                            if (replaceArray)
                            {
                                jsonPatchDocument.Remove(propertyName);
                                jsonPatchDocument.Add(propertyName, newPropertyJson);
                            }
                        }

                        break;
                }
            }
            else if (newPropertyJson.ValueKind != JsonValueKind.Null && existingPropertyJson.ValueKind == JsonValueKind.Null)
            {
                switch (newPropertyJson.ValueKind)
                {
                    case JsonValueKind.String:
                        {
                            jsonPatchDocument.Replace(propertyName, newPropertyJson.ToString());
                            break;
                        }

                    case JsonValueKind.Number:
                        {
                            // See why we always try decimal first, here: https://stackoverflow.com/questions/72597489/what-is-the-idiomatic-way-to-find-the-underlying-type-of-a-jsonelement-value
                            if (newPropertyJson.TryGetDecimal(out var result))
                            {
                                jsonPatchDocument.Replace(propertyName, result);
                            }
                            else
                            {
                                jsonPatchDocument.Replace(propertyName, newPropertyJson.ToString());
                            }

                            break;
                        }

                    case JsonValueKind.Object:
                        {
                            foreach (var prop in newPropertyJson.EnumerateObject())
                            {
                                // Skip the metadata property
                                if (prop.Name != "$metadata")
                                {
                                    GetUpdates(jsonPatchDocument, propertyName + "/" + prop.Name, prop.Value, existingPropertyJson);
                                }
                            }
                        }

                        break;

                    case JsonValueKind.Array:
                        {
                            jsonPatchDocument.Remove(propertyName);
                            jsonPatchDocument.Add(propertyName, newPropertyJson);
                        }

                        break;
                }
            }
            else if (newPropertyJson.ValueKind == JsonValueKind.Null && existingPropertyJson.ValueKind != JsonValueKind.Null)
            {
                jsonPatchDocument.Remove(propertyName);
            }
            else
            {
                // The kind has changed. Do a replace
                jsonPatchDocument.Replace(propertyName, newPropertyJson);
            }
        }
    }
}
