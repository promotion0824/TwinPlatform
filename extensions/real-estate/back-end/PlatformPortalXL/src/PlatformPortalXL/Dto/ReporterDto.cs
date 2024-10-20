using System;
using System.Collections.Generic;
using System.Linq;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class ReporterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }

        public static ReporterDto MapFromModel(Reporter model)
        {
            return new ReporterDto
            {
                Id = model.Id,
                Name = model.Name,
                Phone = model.Phone,
                Email = model.Email,
                Company = model.Company,
            };
        }

        public static List<ReporterDto> MapFromModels(List<Reporter> models)
        {
            return models?.Select(MapFromModel).ToList();
        }
    }
}
