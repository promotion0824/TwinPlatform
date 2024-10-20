using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class CustomerFeaturesDto
    {
        public bool IsConnectivityViewEnabled { get; set; }
        public bool IsDynamicsIntegrationEnabled { get; set; }
        public bool IsSmartPolesEnabled { get; set; }

        public static CustomerFeaturesDto Map(CustomerFeatures model) => 
            new CustomerFeaturesDto
            {
                IsConnectivityViewEnabled = model.IsConnectivityViewEnabled,
                IsDynamicsIntegrationEnabled = model.IsDynamicsIntegrationEnabled,
                IsSmartPolesEnabled = model.IsSmartPolesEnabled
            };
    }
}
