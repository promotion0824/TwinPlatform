using System;
using System.Collections.Generic;

namespace Willow.Platform.Models
{
    public class Widget
    {
        public Guid Id { get; set; }
        public WidgetType Type { get; set; }
        public string Metadata { get; set; }
        public List<WidgetPosition> Positions { get; set; }
    }
}
