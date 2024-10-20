using System;
using System.Collections.Generic;
using System.Linq;

namespace MobileXL.Models
{
    public class DigitalTwinDocument
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Uri Uri { get; set; }

        public static List<AssetFile> MapToModels(List<DigitalTwinDocument> documents)
        {
            return documents.Select(d => new AssetFile
            {
                Id = d.Id,
                FileName = d.Name
            }).ToList();
        }
    }
}
