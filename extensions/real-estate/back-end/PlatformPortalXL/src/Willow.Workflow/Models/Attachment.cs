using System;

namespace Willow.Workflow
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public AttachmentType Type { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }

        public string Path { get; set; }
    }
}
