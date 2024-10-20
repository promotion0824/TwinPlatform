using DigitalTwinCore.Models;
using System;
using System.Linq;

namespace DigitalTwinCore.Dto
{
    [Serializable]
    public class PointMetadataDto
    {
        public PointCommunicationDto Communication { get; set; }
        public DeviceMetadataDto DeviceMetadata { get; set; }

        internal static PointMetadataDto MapFrom(Point point)
        {
            if (point.Communication != null || point.Devices != null)
            {
                return new PointMetadataDto
                {
                    Communication = PointCommunicationDto.MapFrom(point.Communication),
                    DeviceMetadata = DeviceMetadataDto.MapFrom(point.Devices?.FirstOrDefault())
                };
            }

            return null;
        }
    }
}
