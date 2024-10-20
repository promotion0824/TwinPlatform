using DigitalTwinCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Dto
{
    public class ModelDto
    {
        public bool? IsDecommissioned { get; set; }
        public IReadOnlyDictionary<string, string> Descriptions { get; set; }
        public IReadOnlyDictionary<string, string> DisplayNames { get; set; }
        public string Id { get; set; }
        public DateTimeOffset? UploadTime { get; set; }
        public string Model { get; set; }
        public bool IsShared { get; set; }

        public static ModelDto MapFrom(string siteCodeForModelId, Model model)
        {
            return new ModelDto
            {
                Descriptions = model.Descriptions,
                DisplayNames = model.DisplayNames,
                Id = model.Id,
                IsDecommissioned = model.IsDecommissioned,
                Model = model.ModelDefinition,
                UploadTime = model.UploadTime,
                IsShared = !model.Id.Contains($":{siteCodeForModelId}:")
            };
        }

        public static IEnumerable<ModelDto> MapFrom(string siteCodeForModelId, IEnumerable<Model> modelEntities)
        {
            return modelEntities.Select(m => MapFrom(siteCodeForModelId, m));
        }
    }
}
