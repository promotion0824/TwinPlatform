using System.Text.Json.Serialization;

namespace Willow.Model.Adt
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EntityType
    {
        Twins,
        Relationships,
        Models,
        Unknown = 99
    }
}
