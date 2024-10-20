using System.Collections.Generic;

namespace DigitalTwinCore.Models
{
    public class NestedTwin
    {
        public Twin Twin { get; set; }
        public IList<NestedTwin> Children { get; set; }

        public NestedTwin(Twin twin)
        {
            Twin = twin;
            Children = new List<NestedTwin>();
        }

        public NestedTwin(Twin twin, IList<NestedTwin> children)
        {
            Twin = twin;
            Children = children;
        }
    }
}
