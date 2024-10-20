using Willow.Model.Adt;

namespace Willow.Model.Requests
{
    public class ExportRequest
    {
        public ExportRequest()
        {
            ExportTargets = new List<EntityType> { EntityType.Twins, EntityType.Relationships, EntityType.Models };
        }

        public List<EntityType> ExportTargets { get; set; }
    }
}
