using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Models;

namespace WorkflowCore.Dto
{
    public class TicketCategoryDto
    {
        public Guid Id { get; set; }
        public Guid? SiteId { get; set; }
        public string Name { get; set; }
        public static TicketCategoryDto MapFromModel(TicketCategory model)
        {
            return new TicketCategoryDto
            {
                Id = model.Id,
                SiteId = model.SiteId,
                Name = model.Name
            };
        }

        public static List<TicketCategoryDto> MapFromModels(List<TicketCategory> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}
