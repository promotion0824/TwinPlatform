using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using PlatformPortalXL.Services.ArcGis;
using PlatformPortalXL.ServicesApi.SiteApi;
using System;
using System.Threading.Tasks;

namespace PlatformPortalXL.Filters.ArcGis
{
	/// <Summary>
	/// It validates whether the site belongs to the DFW customer.
	/// </Summary>
	public class CustomerValidationFilter : IAsyncActionFilter
	{
		private readonly ISiteApiService _siteApiService;
		private readonly ArcGisOptions _options;
		private readonly string ArcGisNotEnabledForCustomerErrorMessage = "ArcGis is not enabled for this customer {0}";

		public CustomerValidationFilter(ISiteApiService siteApiService,
			IOptions<ArcGisOptions> options)
		{
			_siteApiService = siteApiService;
			_options = options.Value;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var siteId = (Guid)context.ActionArguments["siteId"];
			var site = await _siteApiService.GetSite(siteId);

			//We are limiting this feature to DFW customer as of now.
			if (site.CustomerId != _options.CustomerId)
			{
				throw new ArgumentException(string.Format(ArcGisNotEnabledForCustomerErrorMessage, site.CustomerId));
			}

			await next();
		}
	}
}
