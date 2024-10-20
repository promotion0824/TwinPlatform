using System;

namespace DigitalTwinCore.Models
{
    public enum ExportStatus
    {
        Queued,
        Exporting,
        Done,
        Error,
        Swapping,
        Ingesting,
        Canceled,
        CreatingMaterializedViews,
        CreatingFunctions
    }

    public class DetailStatus
    {
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Error { get; set; }
        public string Details { get; set; }
    }

    public class SourceInfo
    {
        public string AccountName { get; set; }
        public string ContainerName { get; set; }
        public string Path { get; set; }

        //Either all or non provided
        public bool IsValid => IsEmpty ||
            (!string.IsNullOrWhiteSpace(AccountName) &&
            !string.IsNullOrWhiteSpace(ContainerName) &&
            !string.IsNullOrWhiteSpace(Path));

        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(AccountName) &&
            string.IsNullOrWhiteSpace(ContainerName) &&
            string.IsNullOrWhiteSpace(Path);
    }

    public class Export
    {
        public Export(Guid siteId)
        {
            CreateTime = DateTime.UtcNow;
            SiteId = siteId;
        }

        public Guid Id { get; set; }
        public ExportStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public SourceInfo SourceInformation { get; set; }
        public DetailStatus TwinsExport { get; set; }
        public DetailStatus RelationshipsExport { get; set; }
        public DetailStatus ModelsExport { get; set; }
        public DateTime CreateTime { get; set; }
        public Guid SiteId { get; private set; }
        public string Error { get; set; }

        public bool HasErrors =>
            !string.IsNullOrEmpty(Error) ||
            (RelationshipsExport != null && !string.IsNullOrWhiteSpace(RelationshipsExport.Error)) ||
            (TwinsExport != null && !string.IsNullOrWhiteSpace(TwinsExport.Error)) ||
            (ModelsExport != null && !string.IsNullOrWhiteSpace(ModelsExport.Error));
    }
}
