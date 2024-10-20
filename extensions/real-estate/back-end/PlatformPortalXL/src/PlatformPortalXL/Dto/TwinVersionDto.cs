using System;

namespace PlatformPortalXL.Dto
{
    public class TwinVersionDto
    {
        public TwinAdxDto Twin { get; set; }
        public UserDto User { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
