using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Platform.Models;

namespace PlatformPortalXL.Dto
{
    public class WidgetDto
    {
        public Guid Id { get; set; }
        public dynamic Metadata { get; set; }
        public WidgetType Type { get; set; }
        public List<WidgetPosition> Positions { get; set; }

        public static WidgetDto Map(Widget model)
        {
            if (model == null)
            {
                return null;
            }

            return new WidgetDto
            {
                Id = model.Id,
                Metadata = model.Metadata.ToCamelCaseExpandoObject(),
                Type = model.Type,
                Positions = model.Positions
            };
        }

        public static List<WidgetDto> Map(IEnumerable<Widget> models) => models?.Select(Map).ToList();
    }
}
