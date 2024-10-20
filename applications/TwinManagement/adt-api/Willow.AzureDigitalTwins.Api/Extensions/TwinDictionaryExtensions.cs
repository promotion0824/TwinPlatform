using System;
using System.Collections;
using System.Collections.Generic;

namespace Willow.AzureDigitalTwins.Api.Extensions;

public static class TwinDictionaryExtensions
{
    /// <summary>
    /// Get a value from a nested dictionary using a key path.
    /// 
    /// This could be extended to use reflection to get data from any nested object, not just dictionaries.
    ///
    /// Note also that ideally we'd have versions of these functions returning an Expression from the path,
    /// so that rather than parsing/interpreting the string every time,
    /// we could cache the string->Expr(T) and Evaluate it as many times as we wanted to.
    /// </summary>
    /// <typeparam name="T">Property value type</typeparam>
    /// <param name="dict">The dictionary to get value from</param>
    /// <param name="keyPath">The path to access the property value. Example: "customProperties.copilot.llm_summary"</param>
    /// <param name="defaultValue">The default value if property is not found</param>
    /// <returns></returns>
    public static T GetValue<T>(this Dictionary<string, object> dict, string keyPath, T defaultValue = default)
    {
        if (dict == null || string.IsNullOrWhiteSpace(keyPath))
        {
            return defaultValue;
        }

        var keys = keyPath.Split('.');
        object current = dict;

        foreach (var key in keys)
        {
            if (current is IDictionary<string, object> nestedDict)
            {
                if (nestedDict.TryGetValue(key, out var value))
                {
                    current = value;
                }
                else
                {
                    return defaultValue; // Key not found
                }
            }
            else if (current is IEnumerable enumerable && !(current is string))
            {
                if (int.TryParse(key.Trim('[', ']'), out int index))
                {
                    var list = (IList)current;
                    if (index >= 0 && index < list.Count)
                    {
                        current = list[index];
                    }
                    else
                    {
                        return defaultValue; // Index out of range
                    }
                }
                else
                {
                    return defaultValue; // Invalid index format
                }
            }
            else
            {
                return defaultValue; // Not a dictionary or list
            }
        }

        // Return the value cast to the expected type, or default if cast fails
        try
        {
            return (T)Convert.ChangeType(current, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Set a value in a nested dictionary using a key path.
    /// </summary>
    /// <param name="dict">The dictionary to set value</param>
    /// <param name="keyPath">The path to access the property value. Example: "customProperties.copilot.llm_summary"</param>
    /// <param name="value">The value to set for the property in key path</param>
    /// <returns></returns>
    public static bool SetValue(this Dictionary<string, object> dict, string keyPath, object value)
    {
        if (dict == null || string.IsNullOrWhiteSpace(keyPath))
        {
            return false;
        }

        var keys = keyPath.Split('.');
        object current = dict;

        for (int i = 0; i < keys.Length - 1; i++)
        {
            var key = keys[i];
            if (current is IDictionary<string, object> nestedDict)
            {
                //creates key if it does not already exist,
                if (!nestedDict.TryGetValue(key, out var next))
                {
                    next = new Dictionary<string, object>();
                    nestedDict[key] = next;
                }
                current = next;
            }
            else if (current is IList list)
            {
                if (int.TryParse(key.Trim('[', ']'), out int index))
                {
                    if (index >= 0 && index < list.Count)
                    {
                        current = list[index];
                    }
                    else
                    {
                        return false; // Index out of range
                    }
                }
                else
                {
                    return false; // Invalid index format
                }
            }
            else
            {
                return false; // Current object is not a dictionary or list
            }
        }

        var finalKey = keys[^1];

        if (current is IDictionary<string, object> finalDict)
        {
            finalDict[finalKey] = value;
            return true;
        }
        else if (current is IList finalList)
        {
            if (int.TryParse(finalKey.Trim('[', ']'), out int index))
            {
                if (index >= 0 && index < finalList.Count)
                {
                    finalList[index] = value;
                    return true;
                }
                else
                {
                    return false; // Index out of range
                }
            }
            else
            {
                return false; // Invalid index format
            }
        }
        else
        {
            return false; // Cannot set value (current object is not a dictionary or list)
        }
    }
}
