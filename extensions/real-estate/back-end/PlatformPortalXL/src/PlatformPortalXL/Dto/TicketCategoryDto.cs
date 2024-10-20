using System.Collections.Generic;
using System;
using System.Linq;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class TicketCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get;set; }

        public static TicketCategoryDto MapFrom(TicketCategory ticketCategory)
        {
            if (ticketCategory == null)
            {
                return null;
            }

            return new TicketCategoryDto
            {
                Id = ticketCategory.Id,
                Name = ticketCategory.Name
            };
        }

        public static List<TicketCategoryDto> MapFrom(IEnumerable<TicketCategory> eTags)
        {
            return eTags?.Select(MapFrom).ToList();
        }
    }
}
