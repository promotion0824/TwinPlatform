using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.ServicesApi.DigitalTwinApi
{
    public class DigitalTwinDocument
    {
        public string ModelId { get; set; }
        public string TwinId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Uri Uri { get; set; }

        public static List<AssetFile> MapToModels(List<DigitalTwinDocument> documents)
        {
            return documents.Select(d => new AssetFile
            {
                TwinId = d.TwinId,
                Id = d.Id,
                FileName = d.Name
            }).ToList();
        }
    }
}
