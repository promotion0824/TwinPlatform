using System.ComponentModel;

namespace Willow.AzureDataExplorer.Helpers;

public static class EnumHelper
{
    public static string? GetDescription<T>(this T enumValue) where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
        {
            return null;
        }

        var description = enumValue.ToString();

        if (description == null)
        {
            return null;
        }

        var fieldInfo = enumValue.GetType().GetField(description);

        if (fieldInfo != null)
        {
            var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                description = ((DescriptionAttribute)attrs[0]).Description;
            }
        }

        return description;
    }
}
