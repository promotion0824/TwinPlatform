using Newtonsoft.Json;

namespace WorkflowCore.Infrastructure.Json
{
    public static class JsonSettings
    {
        private static JsonSerializerSettings _jsonSerializerSettings;

        public static JsonSerializerSettings CaseSensitive
        {
            get
            {
                if (_jsonSerializerSettings is null)
                    _jsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CaseSensitiveContractResolver() };

                return _jsonSerializerSettings;
            }
        }
    }
}
