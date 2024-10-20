using System;
using System.Collections.Generic;
using System.Linq;
using MobileXL.Models;

namespace MobileXL.Dto
{
public class AssetFileDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }

        public static AssetFileDto MapFromModel(AssetFile file)
        {
            if (file == null)
            {
                return null;
            }

            return new AssetFileDto
            {
                Id = file.Id,
                FileName = file.FileName
            };
        }

        public static List<AssetFileDto> MapFromModels(IEnumerable<AssetFile> files)
        {
            return files.Select(MapFromModel).ToList();
        }
    }
}
