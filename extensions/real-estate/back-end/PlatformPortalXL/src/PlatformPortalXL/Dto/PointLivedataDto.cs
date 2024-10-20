using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class PointLivedataDto
    {
        public Guid PointEntityId { get; set; }

        public RawDataDto RawData { get; set; }

        public string EquipmentId { get; set; }

        public string Unit { get; set; }

        public static PointLivedataDto MapFrom(PointLive pointLive)
        {
            return new PointLivedataDto
            {
                PointEntityId = pointLive.Point.EntityId,
                EquipmentId = pointLive.Point?.Equipment == null ? "" : string.Join(",", pointLive.Point.Equipment.Select(e => e.Id)),
                RawData = RawDataDto.MapFrom(pointLive.RawData),
                Unit = pointLive.Point.Unit
            };
        }

        public static List<PointLivedataDto> MapFrom(IEnumerable<PointLive> pointsLive)
        {
            return pointsLive?.Select(MapFrom).ToList();
        }
    }

    public class RawDataDto
    {
        public DateTime Timestamp { get; set; }

        public double Value { get; set; }

        public static RawDataDto MapFrom(PointTimeSeriesRawData rawData)
        {
            if (rawData == null)
            {
                return null;
            }

            return new RawDataDto
            {
                Value = rawData.Value,
                Timestamp = rawData.Timestamp
            };
        }
    }
}
