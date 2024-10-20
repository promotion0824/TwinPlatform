using System.Collections.Generic;

namespace AssetCoreTwinCreator.Features.Asset.Attachments.Models
{
    public class File
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string BlobName { get; set; }
        public long Size { get; set; }
        public List<int> AssetRegisterIds { get; set; }
    }
}