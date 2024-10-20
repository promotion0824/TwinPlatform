using System;

namespace ImageHub.Models
{
    public class ImageDescriptor
    {
        public Guid ImageId { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
    }
}
