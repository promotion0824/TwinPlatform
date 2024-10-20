using Newtonsoft.Json.Serialization;

namespace WorkflowCore.Infrastructure.Json
{
    public class CaseSensitiveContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName;
        }
    }
}
