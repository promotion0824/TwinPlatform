using System;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;

namespace PlatformPortalXL.Dto
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string AccountExternalId { get; set; }
        public string SigmaConnectionId { get; set; }
        public CustomerFeaturesDto Features { get; set; }

        public int Status { get; set; }
        public string SingleTenantUrl { get; set; }

        public static CustomerDto Map(Customer model, IImageUrlHelper helper)
        {
            return new CustomerDto
            {
                Id = model.Id,
                Name = model.Name,
                LogoUrl = model.LogoId.HasValue ? helper.GetCustomerLogoUrl(model.LogoPath, model.LogoId.Value) : string.Empty,
                AccountExternalId = model.AccountExternalId,
                SigmaConnectionId = model.SigmaConnectionId,
                Features = CustomerFeaturesDto.Map(model.Features),
                Status = model.Status,
                SingleTenantUrl = model.SingleTenantUrl
            };
        }
    }
}
