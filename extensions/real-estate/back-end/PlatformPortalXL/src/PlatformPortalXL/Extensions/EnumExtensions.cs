using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PlatformPortalXL.Dto;

namespace PlatformPortalXL.Extensions;

public static class EnumExtensions
{
    public static List<EnumKeyValueDto> ToEnumKeyValueDto(this Type enumType)
    {
        if (!enumType.IsEnum)
        {
            throw new ArgumentException("T must be an enumerator type.");
        }

        return Enum.GetValues(enumType)
            .Cast<int>()
            .Select(x => new EnumKeyValueDto(key: x, value: Enum.GetName(enumType, x)))
            .ToList();
    }

    public static string GetDescription(this Enum enumValue)
    {
        var description = enumValue.ToString();
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

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
