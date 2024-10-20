using System;

namespace PlatformPortalXL.Models
{
    public class InspectionRecord
    {
        public Guid Id { get; set; }
        public Guid InspectionId { get; set; }
        public string EffectiveDate { get; set; }
    }
}
