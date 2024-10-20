using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Domain;
using DirectoryCore.Enums;
using DirectoryCore.Services;

namespace DirectoryCore.Dto
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Suburb { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public Guid? LogoId { get; set; }
        public int Status { get; set; }
        public string LogoPath { get; set; }
        public string AccountExternalId { get; set; }
        public string SigmaConnectionId { get; set; }
        public CustomerFeaturesDto Features { get; set; }
        public string CognitiveSearchUri { get; set; }
        public string CognitiveSearchIndex { get; set; }

        /// <summary>
        /// If the customer has been migrated to single tenant,
        /// this will be the URL to their single tenant instance. For example
        /// "https://sanofi.app.willowinc.com"
        /// </summary>
        public string SingleTenantUrl { get; set; }

        public static CustomerDto MapFrom(Customer customer, IImagePathHelper helper)
        {
            if (customer == null)
            {
                return null;
            }

            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Address1 = customer.Address1,
                Address2 = customer.Address2,
                Suburb = customer.Suburb,
                Postcode = customer.Postcode,
                Country = customer.Country,
                State = customer.State,
                Status = (int)customer.Status,
                LogoId = customer.LogoId,
                AccountExternalId = customer.AccountExternalId,
                SigmaConnectionId = customer.SigmaConnectionId,
                LogoPath = helper.GetCustomerLogoPath(customer.Id),
                Features = CustomerFeaturesDto.MapFrom(customer.Features),
                CognitiveSearchUri = customer.CognitiveSearchUri,
                CognitiveSearchIndex = customer.CognitiveSearchIndex,
                SingleTenantUrl = customer.SingleTenantUrl
            };
            return customerDto;
        }

        public static IList<CustomerDto> MapFrom(
            IEnumerable<Customer> customers,
            IImagePathHelper helper
        )
        {
            return customers?.Select(x => MapFrom(x, helper)).ToList();
        }
    }
}
