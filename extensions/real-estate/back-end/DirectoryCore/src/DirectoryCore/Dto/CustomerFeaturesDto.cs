using DirectoryCore.Domain;

namespace DirectoryCore.Dto
{
    public class CustomerFeaturesDto
    {
        public bool IsConnectivityViewEnabled { get; set; }
        public bool IsRulingEngineEnabled { get; set; }
        public bool IsDynamicsIntegrationEnabled { get; set; }
        public bool IsSmartPolesEnabled { get; set; }

        public static CustomerFeaturesDto MapFrom(CustomerFeatures model)
        {
            if (model == null)
            {
                model = new CustomerFeatures();
            }

            return new CustomerFeaturesDto
            {
                IsConnectivityViewEnabled = model.IsConnectivityViewEnabled,
                IsRulingEngineEnabled = model.IsRulingEngineEnabled,
                IsDynamicsIntegrationEnabled = model.IsDynamicsIntegrationEnabled,
                IsSmartPolesEnabled = model.IsSmartPolesEnabled
            };
        }
    }
}
