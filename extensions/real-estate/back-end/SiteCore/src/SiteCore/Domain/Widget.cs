using SiteCore.Entities;
using System;
using System.Collections.Generic;

namespace SiteCore.Domain
{
    public class Widget
    {
        public Guid Id { get; set; }
        public WidgetType Type { get; set; }
        public string Metadata { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public IEnumerable<WidgetPosition> Positions { get; set; }
    }
}
