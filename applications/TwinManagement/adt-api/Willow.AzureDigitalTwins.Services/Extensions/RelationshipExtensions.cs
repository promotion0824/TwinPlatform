using Azure.DigitalTwins.Core;

namespace Willow.AzureDigitalTwins.Services.Extensions;

public static class RelationshipExtensions
{
    public static bool Match(this BasicRelationship relationship, BasicRelationship other)
    {
        if (other == null)
            return false;

        if (relationship.Id == other.Id)
            return true;

        if (GetId(relationship) == GetId(other))
            return true;

        return false;
    }

    public static bool IsLoaded(this BasicRelationship relationship)
    {
        return relationship != null && !string.IsNullOrEmpty(relationship.Id);
    }

    public static void SetId(this BasicRelationship relationship)
    {
        if (!string.IsNullOrEmpty(relationship.Id))
            return;

        // Note that we could specify a callback to log or throw an exception as a 2nd param to GetId here -
        //   however, currently TLM only allows strings as rel prop vals.
        relationship.Id = GetId(relationship);
    }

    // For a relationship with properties, return true for value types that
    //    can be used for constructing safe relationship Ids 
    // A complex type would need to use escaped JSON serialization 
    private static bool IsSimpleType(object o) =>
         o is null || o is string || o.GetType().IsPrimitive || o is decimal;

    /// <summary>
    /// Create an relationship ID for the given relationship.
    /// This defines the uniqueness criteria for a relationship.
    /// The format is Source_RelName_Target.
    /// If there are also properties on the relationship, the keys and values
    ///   are also appended.
    ///   Examples:  T12_locatedIn_Room-42 or AHU-35_feeds_FCU-99_temp_cold
    /// Warning: that this means that if a property on a relationship even changes after it has
    ///   been created, then we may end up with duplicate relationships.
    /// </summary>
    /// <param name="relationship"></param>
    /// <returns>The id for the new relationship</returns>
    public static string GetId(BasicRelationship relationship, Action<KeyValuePair<string, object>> invalidCallback = null)
    {
        // TODO: If there are invalid chars in a rel prop val, we'd need to encode them or TLM needs to disallow
        var idKeys = new List<string> { relationship.SourceId, relationship.Name, relationship.TargetId };

        if (relationship.Properties != null)
        {
            foreach (var prop in relationship.Properties.OrderBy(x => x.Key))
            {
                if (invalidCallback != null && !IsSimpleType(prop.Value))
                    invalidCallback(prop);

                idKeys.Add(prop.Key);
                idKeys.Add(prop.Value.ToString());
            }
        }

        return string.Join("_", idKeys);
    }
}
