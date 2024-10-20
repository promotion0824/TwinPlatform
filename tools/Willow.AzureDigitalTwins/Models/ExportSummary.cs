namespace Willow.AzureDigitalTwins.BackupRestore.Models
{
    public class ExportSummary
    {
        public int TwinsCount { get; set; }
        public int RelationshipsCount { get; set; }
        public int? ModelsCount { get; set; }
    }
}
