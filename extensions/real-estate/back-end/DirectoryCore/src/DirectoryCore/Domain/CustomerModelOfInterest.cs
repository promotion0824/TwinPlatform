using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Dto.Requests;

namespace DirectoryCore.Domain
{
    public class CustomerModelOfInterest
    {
        public Guid Id { get; set; }
        public string ModelId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }

        public static CustomerModelOfInterest MapFrom(UpdateCustomerModelOfInterestRequest model)
        {
            if (model == null)
            {
                return null;
            }

            return new CustomerModelOfInterest
            {
                Id = model.Id,
                ModelId = model.ModelId,
                Name = model.Name,
                Color = model.Color,
                Text = model.Text,
                Icon = model.Icon
            };
        }

        public static List<CustomerModelOfInterest> MapFrom(
            IEnumerable<UpdateCustomerModelOfInterestRequest> customerModelsOfInterest
        )
        {
            return customerModelsOfInterest?.Select(x => MapFrom(x)).ToList();
        }
    }
}
