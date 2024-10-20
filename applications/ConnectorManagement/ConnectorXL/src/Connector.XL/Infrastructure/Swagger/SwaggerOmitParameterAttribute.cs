namespace Connector.XL.Infrastructure.Swagger;

using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class SwaggerOmitParameterAttribute : Attribute
{
    public SwaggerOmitParameterAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}
