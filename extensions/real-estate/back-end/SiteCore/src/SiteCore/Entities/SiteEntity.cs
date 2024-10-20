using SiteCore.Domain;
using SiteCore.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Willow.Calendar;
using Willow.Infrastructure;
namespace SiteCore.Entities
{
    [Table("Sites")]
    public class SiteEntity
    {
        public SiteEntity()
        {
            Floors = new HashSet<FloorEntity>();
        }

        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? PortfolioId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Suburb { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public int NumberOfFloors { get; set; }
        public string Area { get; set; }
        public Guid? LogoId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string TimezoneId { get; set; }
        public SiteStatus Status { get; set; }
        public PropertyType Type { get; set; }
        public int? ConstructionYear { get; set; }
        public string SiteCode { get; set; }
        public string SiteContactName { get; set; }
        public string SiteContactEmail { get; set; }
        public string SiteContactTitle { get; set; }
        public string SiteContactPhone { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? DateOpened { get; set; }

        public virtual ICollection<FloorEntity> Floors { get; set; }

        public static Site MapToDomainObject(SiteEntity siteEntity)
        {
            if (siteEntity == null)
            {
                return null;
            }

            return new Site
            {
                Id = siteEntity.Id,
                CustomerId = siteEntity.CustomerId,
                PortfolioId = siteEntity.PortfolioId,
                Name = siteEntity.Name,
                Code = siteEntity.Code,
                Address = siteEntity.Address,
                Suburb = siteEntity.Suburb,
                State = siteEntity.State,
                Postcode = siteEntity.Postcode,
                Country = siteEntity.Country,
                NumberOfFloors = siteEntity.NumberOfFloors,
                Area = siteEntity.Area,
                LogoId = siteEntity.LogoId,
                Latitude = siteEntity.Latitude,
                Longitude = siteEntity.Longitude,
                Timezone = siteEntity.TimezoneId.FindEquivalentWindowsTimeZoneInfo(),
				TimezoneId = siteEntity.TimezoneId,
                Status = siteEntity.Status,
                Type = siteEntity.Type,
                SiteCode = siteEntity.SiteCode,
                ConstructionYear = siteEntity.ConstructionYear,
                SiteContactEmail = siteEntity.SiteContactEmail,
                SiteContactName = siteEntity.SiteContactName,
                SiteContactPhone = siteEntity.SiteContactPhone,
                SiteContactTitle = siteEntity.SiteContactTitle,
                CreatedDate = siteEntity.CreatedDate,
                DateOpened = siteEntity.DateOpened != null ? DateOnly.FromDateTime(siteEntity.DateOpened.Value) : null,
				Features = MapSiteFeatures(siteEntity.FeaturesJson),
				WebMapId = siteEntity.WebMapId,
				ArcGisLayers = MapSiteArcGisLayers(siteEntity.ArcGisLayersJson)
			};
        }

        public static List<Site> MapToDomainObjects(IEnumerable<SiteEntity> siteEntities)
        {
            return siteEntities.Select(MapToDomainObject).ToList();
        }

		public string FeaturesJson { get; set; }
		public string WebMapId { get; set; }
		public string ArcGisLayersJson { get; set; }

		private static SiteFeatures MapSiteFeatures(string featuresJson)
		{
			if (string.IsNullOrWhiteSpace(featuresJson))
			{
				featuresJson = "{}";
			}

			try
			{
				return JsonSerializerExtensions.Deserialize<SiteFeatures>(featuresJson);
			}
			catch (Exception)
			{
				//Not a valid json format - return default site features
			}

			return new SiteFeatures();
		}

		private static List<ArcGisLayer> MapSiteArcGisLayers(string arcGisLayersJson)
		{
			var arcGisLayers = new List<ArcGisLayer>();

			if (!string.IsNullOrWhiteSpace(arcGisLayersJson))
			{
				try
				{
					arcGisLayers = JsonSerializerExtensions.Deserialize<List<ArcGisLayer>>(arcGisLayersJson);
				}
				catch (Exception)
				{
					//Not a valid json format - return empty arcGisLayers
				}
			}

			return arcGisLayers;
		}
	}
}
