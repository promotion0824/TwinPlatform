using SiteCore.Domain;
using SiteCore.Entities;
using System.Collections.Generic;
using System.Text.Json;
using Willow.Infrastructure;

namespace SiteCore.Dto
{
    public class CreateUpdateWidgetRequest
    {
        public dynamic Metadata { get; set; }
        public WidgetType Type { get; set; }

        public List<WidgetPosition> Positions { get; set; }

        public static Widget MapToDomainObject(CreateUpdateWidgetRequest widget)
        {
            return new Widget
            {
                Type = widget.Type,
                Metadata = JsonSerializerExtensions.Serialize(widget.Metadata),
                Positions = widget.Positions
            };
        }

        public void MapTo(WidgetEntity widget)
        {
            widget.Type = Type;
            widget.Metadata = JsonSerializerExtensions.Serialize(Metadata);
        }
    }
}
