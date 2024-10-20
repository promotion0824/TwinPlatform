using DigitalTwinCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Dto
{
    [Serializable]
    public class PointDto
    {
        public string ModelId { get; set; }
        public string TwinId { get; set; }
        public Guid Id { get; set; }
        public Guid TrendId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<TagDto> Tags { get; set; }
        public PointType Type { get; set; }
        public PointValue CurrentValue { get; set; }
        public decimal? DisplayPriority { get; set; }
        public string DisplayName { get; set; }
        public List<PointAssetDto> Assets { get; set; }
        public Dictionary<string, Property> Properties { get; set; }
        public bool? IsEnabled { get; set; }
        public bool? IsDetected { get; set; }
        public Guid? DeviceId { get; set; }
        public string TrendInterval { get; set; }
        public string CategoryName { get; set; }
        public PointMetadataDto Metadata { get; set; }

        internal static PointDto MapFrom(Point point, bool? includeAssets, bool? includeMetadata) =>
            new PointDto
            {
                ModelId = point.ModelId,
                TwinId = point.TwinId,
                Id = point.Id,
                ExternalId = point.ExternalId,

                TrendId = point.TrendId,
                TrendInterval =  Math.Round( point.TrendInterval?.TotalSeconds ?? 0).ToString("N0"),

                Name = point.Name,
                Type = point.Type,
                CategoryName = point.CategoryName,
                Description = point.Description,
                Tags = TagDto.MapFrom(point.Tags),

                DisplayPriority = point.DisplayPriority,
                IsDetected = point.IsDetected,
                IsEnabled = point.IsEnabled,
                DisplayName = point.DisplayName,

                Assets = (includeAssets.GetValueOrDefault()) ? PointAssetDto.MapFrom(point.Assets) : null,
                Properties = point.Properties,
                Metadata = (includeMetadata.GetValueOrDefault()) ? PointMetadataDto.MapFrom(point) : null,

                DeviceId = point.Devices.FirstOrDefault()?.UniqueId,

                CurrentValue = point.CurrentValue,
            };

        internal static List<PointDto> MapFrom(List<Point> points, bool? includeAssets, bool? includeMetaData) =>
            points?.Select(p => MapFrom(p, includeAssets, includeMetaData)).ToList() ?? new List<PointDto>();
    }
}
