using System.Reflection;

namespace Authorization.Common.Models;
public record AuditLog
{
    public static string Format(string entityType, RecordAction action, string? entityIdentifier, string? details = null)
    {
        return $"[Type=\"{entityType}\" Action=\"{action}\" Identifier=\"{entityIdentifier}\" {(string.IsNullOrWhiteSpace(details) ? "" : $"Details=\"{details}\"")}]";
    }

    public static string Summarize<T>(T OldObject, T newObject) => string.Join(',', ListChanges(OldObject, newObject, 0));

    private static List<string> ListChanges<T>(T OldObject, T newObject, int depth)
    {
        if (++depth > 2)
            return [];

        List<string> changes = [];
        try
        {
            if (OldObject == null || newObject == null)
            {
                return [];
            }

            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object? oldValue = property.GetValue(OldObject);
                object? newValue = property.GetValue(newObject);

                if (property.PropertyType.IsPrimitive ||
                    property.PropertyType == typeof(string) ||
                    property.PropertyType == typeof(decimal))
                {
                    if (!Equals(oldValue, newValue))
                    {
                        changes.Add($"{property.Name} has changed: {oldValue} -> {newValue}");
                    }
                }
                else if (property.PropertyType.IsArray)
                {
                    int? OldCount = (oldValue as Array)?.Length;
                    int? newCount = (newValue as Array)?.Length;
                    if (newCount != OldCount)
                    {
                        changes.Add($"{property.Name} collection is modified.");
                    }
                }
                else // could be a complex object at this point
                {
                    changes.AddRange(ListChanges(oldValue, newValue, depth));
                }
            }
        }
        catch (Exception)
        {
            changes.Add($"Unable to compute changes for {nameof(OldObject)}.");
        }

        return changes;
    }
}
