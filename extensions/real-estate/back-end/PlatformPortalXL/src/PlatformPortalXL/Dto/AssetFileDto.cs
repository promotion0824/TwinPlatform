using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class AssetFileDto
    {
        /// <summary>
        /// The dtId of a twin 
        /// </summary>
        public string TwinId { get; set; }

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
                TwinId = file.TwinId,
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
