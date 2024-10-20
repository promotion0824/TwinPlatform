namespace ConnectorCore.Common.Extensions
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Reflection;

    internal static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            if (field == null)
            {
                return string.Empty;
            }

            var attribs = field.GetCustomAttributes(typeof(DisplayAttribute), true).ToList();
            if (attribs.Any())
            {
                return ((DisplayAttribute)attribs.First()).GetName();
            }

            return value.ToString();
        }
    }
}
