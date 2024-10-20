namespace ConnectorCore.Common.Extensions
{
    using System.Text;

    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendIfTrue(this StringBuilder builder, string str, bool flag) => flag ? builder.Append(str) : builder;
    }
}
