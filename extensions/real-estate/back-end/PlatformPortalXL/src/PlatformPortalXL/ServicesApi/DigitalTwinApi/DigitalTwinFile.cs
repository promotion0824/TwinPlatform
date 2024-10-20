using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlatformPortalXL.ServicesApi.DigitalTwinApi
{
    public class DigitalTwinFile
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public Guid UniqueId { get; set; }
        public DigitalTwinFileMetadata Metadata { get; set; }
        public DigitalTwinFileCustomProperties CustomProperties { get; set; }
    }
}
