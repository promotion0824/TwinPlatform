using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using DirectoryCore.Domain;
using Willow.Infrastructure;

namespace DirectoryCore.Entities
{
    public class PortfolioEntity
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid CustomerId { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1000)]
        public string FeaturesJson { get; set; }

        public static Portfolio MapTo(PortfolioEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new Portfolio
            {
                Id = entity.Id,
                Name = entity.Name,
                CustomerId = entity.CustomerId,
                Features = MapSiteFeatures(entity.FeaturesJson)
            };
        }

        public static List<Portfolio> MapTo(IList<PortfolioEntity> entities)
        {
            return entities?.Select(MapTo).ToList();
        }

        private static PortfolioFeatures MapSiteFeatures(string featuresJson)
        {
            if (string.IsNullOrWhiteSpace(featuresJson))
            {
                featuresJson = "{}";
            }

            try
            {
                return JsonSerializerExtensions.Deserialize<PortfolioFeatures>(featuresJson);
            }
            catch (Exception)
            {
                //Not a valid json format - return default site features
            }

            return new PortfolioFeatures();
        }
    }
}
