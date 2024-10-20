using System;
using System.Collections.Generic;
using System.Linq;

using PlatformPortalXL.Services;

using Willow.Platform.Models;
using Willow.Platform.Statistics;

namespace PlatformPortalXL.Dto
{
    public class SiteDetailDto
    {
        public Guid Id { get; set; }
        public Guid? PortfolioId { get; set; }

        /// <summary>
        /// Digital Twin Identifier
        /// </summary>
        public string TwinId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Suburb { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public int NumberOfFloors { get; set; }
        public string LogoUrl { get; set; }
        public string LogoOriginalSizeUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string TimeZoneId { get; set; }
        public string Area { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string UserRole { get; set; }
        public string TimeZone { get; set; }
        public SiteFeaturesDto Features { get; set; }
        public SiteSettingsDto Settings { get; set; }
        public bool? IsOnline { get; set; }
        public WeatherDto Weather { get; set; }
        public int? ConstructionYear { get; set; }
        public string SiteCode { get; set; }
        public string SiteContactName { get; set; }
        public string SiteContactEmail { get; set; }
        public string SiteContactTitle { get; set; }
        public string SiteContactPhone { get; set; }
        public string WebMapId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateOnly? DateOpened { get; set; }
        public TicketStats TicketStats { get; set; }
        public InsightsStats InsightsStats { get; set; }
        public List<ArcGisLayerDto> ArcGisLayers { get; set; }
        public InsightsStatsByStatus InsightsStatsByStatus { get; set; }
        public TicketStatsByStatus TicketStatsByStatus { get; set; }

        public static SiteDetailDto Map(Site model, IImageUrlHelper helper)
        {
            if (model is null)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(model.LogoPath))
            {
                model.LogoPath = helper.GetSiteLogoPath(model.CustomerId, model.Id);
            }
            return new SiteDetailDto
            {
                Id = model.Id,
                PortfolioId = model.PortfolioId,
                Name = model.Name,
                Code = model.Code,
                Suburb = model.Suburb,
                Address = model.Address,
                State = model.State,
                Postcode = model.Postcode,
                Country = model.Country,
                NumberOfFloors = model.NumberOfFloors,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                TimeZoneId = model.TimeZoneId,
                Area = model.Area,
                Type = model.Type.ToString(),
                Status = model.Status.ToString(),
                ConstructionYear = model.ConstructionYear,
                SiteCode = model.SiteCode,
                SiteContactEmail = model.SiteContactEmail,
                SiteContactName = model.SiteContactName,
                SiteContactPhone = model.SiteContactPhone,
                SiteContactTitle = model.SiteContactTitle,
                WebMapId = model.WebMapId,
                CreatedDate = model.CreatedDate,
                DateOpened = model.DateOpened,
                LogoUrl = model.LogoId.HasValue ? helper.GetSiteLogoUrl(model.LogoPath, model.LogoId.Value) : null,
                LogoOriginalSizeUrl = model.LogoId.HasValue ? helper.GetSiteLogoOriginalSizeUrl(model.LogoPath, model.LogoId.Value) : null
            };
        }

        public static List<SiteDetailDto> Map(IEnumerable<Site> models, IImageUrlHelper helper)
        {
            return models?.Select(x => Map(x, helper)).ToList();
        }
    }
}
