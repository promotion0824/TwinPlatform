using Microsoft.EntityFrameworkCore;
using SiteCore.Domain;
using SiteCore.Entities;
using SiteCore.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.ExceptionHandling.Exceptions;
using Willow.Infrastructure;

namespace SiteCore.Services
{
	public interface ISiteExtendService
	{
		Task<Site> GetSite(Guid siteId);
		Task<List<Site>> GetPortfolioSites(Guid customerId, Guid portfolioId);
		Task<Site> CreateSite(Guid customerId, Guid portfolioId, CreateSiteRequest request);
		Task UpdateSite(Guid customerId, Guid portfolioId, Guid siteId, UpdateSiteRequest request);
		Task<List<Site>> GetSites(bool? isInspectionEnabled, IEnumerable<Guid> siteIds = null);
		Task<List<Site>> GetSitesByCustomer(Guid customerId, bool? isInspectionEnabled, bool? isTicketingDisabled,
			bool? isScheduledTicketsEnabled);
		Task UpdateSiteFeatures(Guid siteId, SiteFeatures features);
	}

	public class SiteExtendService : ISiteExtendService
	{
		private readonly SiteDbContext _dbContext;
        private readonly ISiteService _siteService;

        public SiteExtendService(SiteDbContext dbContext, ISiteService siteService)
        {
            _dbContext = dbContext;
            _siteService = siteService;
        }

        public async Task<Site> GetSite(Guid siteId)
		{
            var sites = await _siteService.GetAllSites();
			var result = sites.FirstOrDefault(c => c.Id == siteId);

			if (result == null)
			{
                throw new NotFoundException(new { SiteId = siteId });
            }

            return result;
		}

		public async Task<List<Site>> GetPortfolioSites(Guid customerId, Guid portfolioId)
		{
            var sites = await _siteService.GetAllSites();
            var result = sites.Where(x => x.CustomerId == customerId && x.PortfolioId == portfolioId).ToList();
			return result;
		}

		public async Task<Site> CreateSite(Guid customerId, Guid portfolioId, CreateSiteRequest request)
		{
			var site = await _dbContext.Sites
				.Where(x => x.CustomerId == customerId && x.PortfolioId == portfolioId && x.Id == request.Id)
				.AsTracking()
				.FirstOrDefaultAsync();

			if (site == null)
			{
                throw new NotFoundException(new { SiteId = request.Id });
            }

			site.Id = request.Id;
			site.CustomerId = customerId;
			site.PortfolioId = portfolioId;
			site.Name = request.Name;
			site.Code = request.Code;
			site.FeaturesJson = JsonSerializerExtensions.Serialize(request.Features);
			site.TimezoneId = request.TimeZoneId;

			await _dbContext.SaveChangesAsync();

            _siteService.RemoveSitesCache();

            return SiteEntity.MapToDomainObject(site);
		}

		public async Task UpdateSite(Guid customerId, Guid portfolioId, Guid siteId, UpdateSiteRequest request)
		{
			var site = await _dbContext.Sites
				.Where(x => x.CustomerId == customerId && x.PortfolioId == portfolioId && x.Id == siteId)
				.AsTracking()
				.FirstOrDefaultAsync();

			if (site == null)
			{
                throw new NotFoundException(new { SiteId = siteId });
            }

			site.Name = request.Name;
			site.FeaturesJson = JsonSerializerExtensions.Serialize(request.Features);
			site.ArcGisLayersJson = JsonSerializerExtensions.Serialize(request.ArcGisLayers);
			site.TimezoneId = request.TimeZoneId;
			site.Status = request.Status;
			site.WebMapId = request.WebMapId;

			await _dbContext.SaveChangesAsync();

            _siteService.RemoveSitesCache();
        }

        public async Task<List<Site>> GetSites(bool? isInspectionEnabled, IEnumerable<Guid> siteIds = null)
		{
            var sites = await _siteService.GetAllSites();

			if (siteIds != null && siteIds.Count() > 0)
			{
                sites = sites.Where(x => siteIds.Contains(x.Id))
						     .ToList();
			}

			if (isInspectionEnabled.HasValue)
			{
				sites = sites.Where(s => s.Features.IsInspectionEnabled == isInspectionEnabled).ToList();
			}

			return sites;
		}

		public async Task<List<Site>> GetSitesByCustomer(Guid customerId, bool? isInspectionEnabled, bool? isTicketingDisabled, bool? isScheduledTicketsEnabled)
		{
            var sites = await _siteService.GetAllSites();

            sites = sites.Where(x => x.CustomerId == customerId).ToList();

            if (isInspectionEnabled.HasValue)
			{
				sites = sites.Where(s => s.Features.IsInspectionEnabled == isInspectionEnabled).ToList();
			}

			if (isTicketingDisabled.HasValue)
			{
				sites = sites.Where(s => s.Features.IsTicketingDisabled == isTicketingDisabled).ToList();
			}

			if (isScheduledTicketsEnabled.HasValue)
			{
				sites = sites.Where(s => s.Features.IsScheduledTicketsEnabled == isScheduledTicketsEnabled).ToList();
			}

			return sites.ToList();
		}

		public async Task UpdateSiteFeatures(Guid siteId, SiteFeatures features)
		{
			var entity = await _dbContext.Sites.Where(x => x.Id == siteId).AsTracking().FirstOrDefaultAsync();

			if (entity != null)
			{
				entity.FeaturesJson = JsonSerializerExtensions.Serialize(features);

				await _dbContext.SaveChangesAsync();

                _siteService.RemoveSitesCache();
            }
        }
	}
}
