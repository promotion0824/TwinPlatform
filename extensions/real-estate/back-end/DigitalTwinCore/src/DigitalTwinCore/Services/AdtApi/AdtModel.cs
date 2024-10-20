using Azure.DigitalTwins.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Services.AdtApi
{
    public interface IAdtModel
    {
        string Model { get; }
        IReadOnlyDictionary<string, string> DisplayNames { get; }
        IReadOnlyDictionary<string, string> Descriptions { get; }
        string Id { get; }
        DateTimeOffset? UploadTime { get; }
        bool? Decommissioned { get; }
    }

    public class AdtModel : IAdtModel
    {
        public string Model { get; set; }

        public IReadOnlyDictionary<string, string> DisplayNames { get; set; }

        public IReadOnlyDictionary<string, string> Descriptions { get; set; }

        public string Id { get; set; }

        public DateTimeOffset? UploadTime { get; set; }

        public bool? Decommissioned { get; set; }

        public static AdtModel MapFrom(DigitalTwinsModelData dto)
        {
            return new AdtModel
            {
                Id = dto.Id,
                Decommissioned = dto.Decommissioned,
                DisplayNames = dto.LanguageDisplayNames,
                Descriptions = dto.LanguageDescriptions,
                Model = dto.DtdlModel,
                UploadTime = dto.UploadedOn
            };
        }
        public static List<AdtModel> MapFrom(IEnumerable<DigitalTwinsModelData> dtos)
        {
            if (dtos == null)
            {
                return new List<AdtModel>();
            }
            return dtos.Select(MapFrom).ToList();
        }
    }
}
