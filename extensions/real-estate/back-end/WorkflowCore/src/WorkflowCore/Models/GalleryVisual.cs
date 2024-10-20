using System;

namespace WorkflowCore.Models
{
    public class GalleryVisual
    {
        public Guid Id { get; set; }
        public Guid ImageId { get; set; }

        public string ImagePath { get; set; }
    }
}
