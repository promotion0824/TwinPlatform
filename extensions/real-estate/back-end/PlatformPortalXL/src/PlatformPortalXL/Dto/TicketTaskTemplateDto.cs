using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

using Willow.Workflow;

namespace PlatformPortalXL.Dto
{
    public class TicketTaskTemplateDto
    {
        public string Description { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TicketTaskType Type { get; set; }
        public int? DecimalPlaces { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public string Unit { get; set; }

        public static TicketTaskTemplateDto MapFromModel(TicketTaskTemplate model)
        {
            return new TicketTaskTemplateDto
            {
                Description = model.Description,
                Type = model.Type,
                DecimalPlaces = model.DecimalPlaces,
                MinValue = model.MinValue,
                MaxValue = model.MaxValue,
                Unit = model.Unit
            };
        }

        public static List<TicketTaskTemplateDto> MapFromModels(List<TicketTaskTemplate> models)
        {
            return models?.Select(x => MapFromModel(x)).ToList();
        }
    }
}
