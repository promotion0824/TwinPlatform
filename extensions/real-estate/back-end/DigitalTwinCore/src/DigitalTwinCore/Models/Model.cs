using DigitalTwinCore.Services.AdtApi;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Models
{
    public class Model
    {
        public bool? IsDecommissioned { get; set; }
        public IReadOnlyDictionary<string, string> Descriptions { get; set; }
        public IReadOnlyDictionary<string, string> DisplayNames { get; set; }
        public string Id { get; set; }
        public DateTimeOffset? UploadTime { get; set; }
        public string ModelDefinition { get; set; }

        internal static Model MapFrom(AdtModel dto)
        {
            return new Model
            {
                Descriptions = dto.Descriptions,
                DisplayNames = dto.DisplayNames,
                Id = dto.Id,
                IsDecommissioned = dto.Decommissioned,
                ModelDefinition = dto.Model,
                UploadTime = dto.UploadTime
            };
        }

        internal static IEnumerable<Model> MapFrom(IEnumerable<AdtModel> dtos)
        {
            return dtos.Select(MapFrom);
        }
    }
}
