using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using System;
using PlatformPortalXL.ServicesApi.DirectoryApi;

namespace PlatformPortalXL.Filters.ArcGis
{
	/// <Summary>
	/// It validates whether the IsArcGisEnabled feature is enabled for the site.
	/// </Summary>
	public class ArcGisValidationFilter : IAsyncActionFilter
	{
		private readonly IDirectoryApiService _directoryApiService;
		private readonly string ArcGisNotEnabledForSiteErrorMessage = "ArcGis is not enabled for this site {0}";

		public ArcGisValidationFilter(IDirectoryApiService directoryApiService)
		{
			_directoryApiService = directoryApiService;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var siteId = (Guid)context.ActionArguments["siteId"];
			var siteFeatures = await _directoryApiService.GetSiteFeatures(siteId);

			if (!siteFeatures.IsArcGisEnabled)
			{
				throw new ArgumentException(string.Format(ArcGisNotEnabledForSiteErrorMessage, siteId));
			}

			await next();
		}
	}
}
