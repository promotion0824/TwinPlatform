using DigitalTwinCore.Features.DirectoryCore.Dtos;
using DigitalTwinCore.Models;
using System;

namespace DigitalTwinCore.Dto
{
    public class TwinVersionDto
    {
        public TwinAdxDto Twin { get; set; }
        public UserDto User { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
