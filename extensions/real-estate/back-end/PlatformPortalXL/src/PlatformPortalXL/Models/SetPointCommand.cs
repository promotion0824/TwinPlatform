using System;

namespace PlatformPortalXL.Models
{
    public class SetPointCommand
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public Guid ConnectorId { get; set; }
        public Guid EquipmentId { get; set; }
        public Guid InsightId { get; set; }
        public Guid PointId { get; set; }
        public Guid SetPointId { get; set; }
        public decimal CurrentReading { get; set; }
        public decimal OriginalValue { get; set; }
        public decimal DesiredValue { get; set; }
        public int DesiredDurationMinutes { get; set; }
        public SetPointCommandStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string ErrorDescription { get; set; }
        public Guid? CreatedBy { get; set; }
        public string Unit { get; set; }
        public SetPointCommandType Type { get; set; }
    }

    public enum SetPointCommandStatus
    {
        Submitted = 0,
        ActivationFailed = 1,
        Active = 2,
        ResetFailed = 3,
        Completed = 4,
        Deleted = 5
    }

    public enum SetPointCommandType
    {
        Temperature = 0
    }
}
