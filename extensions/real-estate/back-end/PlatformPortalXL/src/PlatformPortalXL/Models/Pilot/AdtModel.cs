using PlatformPortalXL.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlatformPortalXL.Pilot
{
    public class AdtModel
    {
        [JsonPropertyName("displayName")]
        public ModelDisplayNameDto DisplayName { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("description")]
        public ModelDescriptionDto Description { get; set; }

        [JsonPropertyName("uploadTime")]
        public DateTimeOffset? UploadTime { get; set; }

        [JsonPropertyName("decommissioned")]
        public bool? Decommissioned { get; set; }

        [JsonPropertyName("model")]
        public ModelDefinitionDto ModelDefinition { get; set; }
    }

    public class ModelDisplayNameDto
    {
        [JsonPropertyName("en")]
        public string En { get; set; }
    }

    public class ModelDescriptionDto
    {
        [JsonPropertyName("en")]
        public string En { get; set; }
    }

    public class ModelDefinitionDto
    {
        private JsonElement? displayNameElement;
        private JsonElement? descriptionElement;
        private JsonElement? extendModelIdsElement;
        private JsonElement? contentsElement;

        [JsonPropertyName("@id")]
        public string Id { get; set; }

        [JsonPropertyName("@type")]
        public ModelDefinitionTypeDto Type { get; set; }

        [JsonPropertyName("@context")]
        public JsonElement ContextElement { get; set; }

        [JsonPropertyName("displayName")]
        public JsonElement? DisplayNameElement
        {
            get => displayNameElement;
            set 
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    displayNameElement = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("description")]
        public JsonElement? DescriptionElement {
            get => descriptionElement;
            set 
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    descriptionElement = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("extends")]
        public JsonElement? ExtendModelIdsElement { 
            get => extendModelIdsElement;
            set
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    extendModelIdsElement = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("contents")]
        public JsonElement? ContentsElement { 
            get => contentsElement;
            set
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    contentsElement = value.Value.Clone();
                }
            }
        }

        [JsonIgnore]
        public List<string> ExtendModelIds
        {
            get
            {
                if (ExtendModelIdsElement.HasValue)
                {
                    if (ExtendModelIdsElement.Value.ValueKind == JsonValueKind.String)
                    {
                        return new List<string> { ExtendModelIdsElement.Value.GetString() };
                    }
                    else if (ExtendModelIdsElement.Value.ValueKind == JsonValueKind.Array)
                    {
                        var output = new List<string>();
                        foreach (JsonElement extendElement in ExtendModelIdsElement.Value.EnumerateArray())
                        {
                            output.Add(extendElement.GetString());
                        }
                        return output;
                    }
                }
                return null;
            }
        }


        [JsonIgnore]
        public List<ModelDefinitionContentDto> Contents
        {
            get
            {
                var output = new List<ModelDefinitionContentDto>();
                if (ContentsElement.HasValue)
                {
                    if (ContentsElement.Value.ValueKind == JsonValueKind.Object)
                    {
                        output.Add(JsonSerializerHelper.Deserialize<ModelDefinitionContentDto>(ContentsElement.ToString()));
                    }
                    else if (ContentsElement.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement relationship in ContentsElement.Value.EnumerateArray())
                        {
                            output.Add(JsonSerializerHelper.Deserialize<ModelDefinitionContentDto>(relationship.ToString()));
                        }
                    }
                }

                return output;
            }
        }

        [JsonIgnore]
        public ModelDisplayNameDto DisplayName
        {
            get
            {
                if (DisplayNameElement.HasValue)
                {
                    if (DisplayNameElement.Value.ValueKind == JsonValueKind.String)
                    {
                        return new ModelDisplayNameDto { En = DisplayNameElement.Value.GetString() };
                    }
                    else if (DisplayNameElement.Value.ValueKind == JsonValueKind.Object)
                    {
                        return new ModelDisplayNameDto { En = DisplayNameElement.Value.GetProperty("en").GetString() };
                    }
                }
                return null;
            }
        }

        [JsonIgnore]
        public ModelDescriptionDto Description
        {
            get
            {
                if (DescriptionElement.HasValue)
                {
                    if (DescriptionElement.Value.ValueKind == JsonValueKind.String)
                    {
                        return new ModelDescriptionDto { En = DescriptionElement.Value.GetString() };
                    }
                    else if (DisplayNameElement.Value.ValueKind == JsonValueKind.Object)
                    {
                        return new ModelDescriptionDto { En = DescriptionElement.Value.GetProperty("en").GetString() };
                    }
                }
                return null;
            }
        }

        [JsonIgnore]
        public string Context
        {
            get
            {
                if (ContextElement.ValueKind == JsonValueKind.String)
                {
                    return ContextElement.GetString();
                }
                else if (ContextElement.ValueKind == JsonValueKind.Array)
                {
                    return ContextElement.EnumerateArray().FirstOrDefault().ToString();
                }
                return string.Empty;
            }
        }

        public static ModelDefinitionDto MapFromModelData(string model)
        {
            if(model == null)
            {
                return null;
            }

            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return JsonSerializer.Deserialize<ModelDefinitionDto>(model, options);
        }
    }

    public enum ModelDefinitionTypeDto
    {
        Interface
    }

    public class ModelDefinitionContentDto
    {
        private JsonElement? type;
        private JsonElement? displayNameElement;
        private JsonElement? descriptionElement;
        private JsonElement? schema;

        [JsonPropertyName("@type")]
        public JsonElement? Type
        {
            get => type;
            set
            {
                if (value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    type = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("target")]
        public string Target { get; set; }

        [JsonPropertyName("displayName")]
        public JsonElement? DisplayNameElement
        {
            get => displayNameElement;
            set
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    displayNameElement = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("description")]
        public JsonElement? DescriptionElement
        {
            get => descriptionElement;
            set
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    descriptionElement = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("schema")]
        public JsonElement? Schema
        {
            get => schema;
            set
            {
                if (value.HasValue && value.Value.ValueKind != JsonValueKind.Undefined)
                {
                    schema = value.Value.Clone();
                }
            }
        }

        [JsonPropertyName("unit")]
        public string Unit { get; set; }

        public bool HasType(ModelDefinitionContentTypeDto type)
        {
            if (this.type.HasValue)
            {
                if (this.type.Value.ValueKind == JsonValueKind.String)
                {
                    Enum.TryParse(this.Type.Value.GetString(), true, out ModelDefinitionContentTypeDto parsedType);
                    return parsedType == type;
                }
                else if (this.Type.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in this.Type.Value.EnumerateArray())
                    {
                        Enum.TryParse(item.GetString(), true, out ModelDefinitionContentTypeDto parsedType);
                        if (parsedType == type)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                throw new ArgumentException($"Unknown content type: {Type.Value.ValueKind} {Type.Value}");
            }
            throw new ArgumentNullException($"Unknown content type: Type is null");
        }

        [JsonIgnore]
        public ModelDisplayNameDto DisplayName
        {
            get
            {
                if (DisplayNameElement.HasValue)
                {
                    if (DisplayNameElement.Value.ValueKind == JsonValueKind.String)
                    {
                        return new ModelDisplayNameDto { En = DisplayNameElement.Value.GetString() };
                    }
                    else if (DisplayNameElement.Value.ValueKind == JsonValueKind.Object)
                    {
                        return new ModelDisplayNameDto { En = DisplayNameElement.Value.GetProperty("en").GetString() };
                    }
                }
                return null;
            }
        }

        [JsonIgnore]
        public ModelDescriptionDto Description
        {
            get
            {
                if (DescriptionElement.HasValue)
                {
                    if (DescriptionElement.Value.ValueKind == JsonValueKind.String)
                    {
                        return new ModelDescriptionDto { En = DescriptionElement.Value.GetString() };
                    }
                    else if (DisplayNameElement.Value.ValueKind == JsonValueKind.Object)
                    {
                        return new ModelDescriptionDto { En = DescriptionElement.Value.GetProperty("en").GetString() };
                    }
                }
                return null;
            }
        }
    }

    public enum ModelDefinitionContentTypeDto
    {
        Telemetry,
        Property,
        Command,
        Relationship,
        Component
    }
}
