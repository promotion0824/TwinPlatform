using System;
using System.Collections.Generic;
using System.Linq;
using AdminPortalXL.Models.Directory;

namespace AdminPortalXL.Dto
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public Guid? LogoId { get; set; }
        public int Status { get; set; }
        public string LogoPath { get; set; }
        public string AccountExternalId { get; set; }
        public string SigmaConnectionId { get; set; }
        public string RegionId { get; set; }

        public static CustomerDto Map(Customer model)
        {
            if (model == null)
            {
                return null;
            }

            return new CustomerDto
            {
                Id = model.Id,
                Name = model.Name,
                Country = model.Country,
                LogoId = model.LogoId,
                Status = model.Status,
                AccountExternalId = model.AccountExternalId,
                SigmaConnectionId = model.SigmaConnectionId,
                RegionId = model.RegionId,
            };
        }

        public static List<CustomerDto> Map(IEnumerable<Customer> models)
        {
            return models?.Select(Map).ToList();
        }
    }
}
