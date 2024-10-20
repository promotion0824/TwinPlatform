using System;
using System.Collections.Generic;
using AssetCoreTwinCreator.Models;

namespace AssetCoreTwinCreator.Dto
{
    public class AssetHistoryFilesDto
    {
        public Guid AssetId { get; set; }

        public List<ChangeHistoryRecord> HistoryRecords { get; set; } = new List<ChangeHistoryRecord>();

        public List<FileDto> Files { get; set; } = new List<FileDto>();
    }

    public class FileDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string BlobName { get; set; }
        public long Size { get; set; }
        public List<Guid> AssetIds { get; set; }
    }
}
