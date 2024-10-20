using System;
using System.Collections.Generic;
using System.Linq;
using DirectoryCore.Domain;

namespace DirectoryCore.Dto
{
    public class CustomerModelOfInterestDto
    {
        public Guid Id { get; set; }
        public string ModelId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }

        public static CustomerModelOfInterestDto MapFrom(CustomerModelOfInterest model)
        {
            if (model == null)
            {
                return null;
            }

            return new CustomerModelOfInterestDto
            {
                Id = model.Id,
                ModelId = model.ModelId,
                Name = model.Name,
                Color = model.Color,
                Text = model.Text,
                Icon = model.Icon
            };
        }

        public static List<CustomerModelOfInterestDto> MapFrom(
            IEnumerable<CustomerModelOfInterest> customerModelsOfInterest
        )
        {
            return customerModelsOfInterest?.Select(x => MapFrom(x)).ToList();
        }
    }
}
