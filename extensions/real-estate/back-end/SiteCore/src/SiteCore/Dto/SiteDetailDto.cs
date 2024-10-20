using System;
using System.Collections.Generic;
using System.Linq;
using SiteCore.Domain;
using SiteCore.Enums;
using SiteCore.Services.ImageHub;

namespace SiteCore.Dto
{
    public class SiteDetailDto
    {
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
        public string TimeZoneId { get; set; }
        public SiteStatus Status { get; set; }
        public PropertyType Type { get; set; }
        public int? ConstructionYear { get; set; }
        public string SiteCode { get; set; }
        public string SiteContactName { get; set; }
        public string SiteContactEmail { get; set; }
        public string SiteContactTitle { get; set; }
        public string SiteContactPhone { get; set; }
        public string LogoPath { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateOnly? DateOpened { get; set; }

        public static SiteDetailDto MapFrom(Site site, IImagePathHelper helper)
        {
            if (site == null)
            {
                return null;
            }

            return new SiteDetailDto
            {
                Id = site.Id,
                CustomerId = site.CustomerId,
                PortfolioId = site.PortfolioId,
                Name = site.Name,
                Code = site.Code,
                Suburb = site.Suburb,
                Address = site.Address,
                State = site.State,
                Postcode = site.Postcode,
                Country = site.Country,
                NumberOfFloors = site.NumberOfFloors,
                Area = site.Area,
                LogoId = site.LogoId,
                Latitude = site.Latitude,
                Longitude = site.Longitude,
                TimeZoneId = site.Timezone.Id,
                Status = site.Status,
                LogoPath = helper.GetSiteLogoPath(site.CustomerId, site.Id),
                Type = site.Type,
                ConstructionYear = site.ConstructionYear,
                SiteCode = site.SiteCode,
                SiteContactEmail = site.SiteContactEmail,
                SiteContactName = site.SiteContactName,
                SiteContactTitle = site.SiteContactTitle,
                SiteContactPhone = site.SiteContactPhone,
                CreatedDate = site.CreatedDate,
                DateOpened = site.DateOpened
            };
        }

        public static IEnumerable<SiteDetailDto> MapFrom(IEnumerable<Site> sites, IImagePathHelper helper)
        {
            return sites?.Select(x => MapFrom(x, helper)).ToList();
        }
    }
}
