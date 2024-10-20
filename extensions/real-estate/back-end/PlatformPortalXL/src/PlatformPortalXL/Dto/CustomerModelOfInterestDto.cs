using PlatformPortalXL.Models;
using System;

namespace PlatformPortalXL.Dto
{
    public class CustomerModelOfInterestDto
    {
        public Guid Id { get; set; }
        public string ModelId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Text { get; set; }
        public string Icon { get; set; }
    }
}
