using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using PlatformPortalXL.ServicesApi.SiteApi;
using Willow.Common;
using System.Collections.Generic;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Extensions;
using System.Dynamic;
using Willow.Platform.Models;
using PlatformPortalXL.Features.Sigma;

namespace PlatformPortalXL.Services.Sigma
{
    public interface ISigmaService
    {
        Task<List<SigmaEmbedUrlDto>> GetScopeEmbedUrls(Guid userId, string connectionId, string scopeId, WidgetRequest request);
        Task<List<SigmaEmbedUrlDto>> GetSiteEmbedUrls(Guid userId, string connectionId, Guid siteId, WidgetRequest request);
        Task<List<SigmaEmbedUrlDto>> GetPortfolioEmbedUrls(Guid userId, string connectionId, Guid portfolioId, WidgetRequest request);
    }

	public class SigmaService : ISigmaService
	{
		private readonly IConfiguration _config;
		private readonly IDateTimeService _dateTimeService;
		private readonly IWidgetApiService _widgetApi;

        private readonly IConfiguration _configuration;

        public static class MetadataConstants
		{
			public const string EmbedLocation = "embedLocation";
			public const string EmbedPath = "embedPath";
			public const string EmbedGroup = "embedGroup";
			public const string Controls = "controls";
			public const string Name = "name";
			public const string Url = "url";
			public const string Reports = "reports";
            public const string HideFolderNavigation = "hideFolderNavigation";

            public static class EmbedLocations
			{
				public const string ReportsTab = "reportsTab";
			}
		}

		public SigmaService(
			IConfiguration config,
			IDateTimeService dateTimeService,
			IWidgetApiService widgetApi,
            IConfiguration configuration)
		{
			_config = config;
			_dateTimeService = dateTimeService;
			_widgetApi = widgetApi;
            _configuration = configuration;
        }

        public async Task<List<SigmaEmbedUrlDto>> GetScopeEmbedUrls(Guid userId, string connectionId, string scopeId, WidgetRequest request)
        {
            return await GetEmbedUrls(userId, connectionId, scopeId, request, _widgetApi.GetWidgetsByScopeId);
        }

        public async Task<List<SigmaEmbedUrlDto>> GetSiteEmbedUrls(Guid userId, string connectionId, Guid siteId, WidgetRequest request)
		{
			return await GetEmbedUrls(userId, connectionId, siteId.ToString(), request, _widgetApi.GetWidgetsBySiteId);
		}

		public async Task<List<SigmaEmbedUrlDto>> GetPortfolioEmbedUrls(Guid userId, string connectionId, Guid portfolioId, WidgetRequest request)
		{
			return await GetEmbedUrls(userId, connectionId, portfolioId.ToString(), request, _widgetApi.GetWidgetsByPortfolioId);
		}

        private async Task<List<SigmaEmbedUrlDto>> GetEmbedUrls(Guid userId, string connectionId,
            string contextId, WidgetRequest request, Func<string, Task<List<Widget>>> getWidgets)
        {
            var sigmaReportDtos = new List<SigmaEmbedUrlDto>();
            var widgets = await getWidgets(contextId);

            var sigmaWidget = widgets.FirstOrDefault(w => w.Id == (request.ReportId ?? w.Id));

            if (request.ReportId.HasValue)
            {
                widgets = widgets.Where(x => x.Id == request.ReportId.Value).ToList();
            }

            foreach (var widget in widgets)
            {
                var metadata = ParseMetadata(widget.Metadata, userId, connectionId, request);

                sigmaReportDtos.AddRange(MapFrom(metadata, widget.Id));
            }

            return sigmaReportDtos.Where(FilterBy(request)).ToList();
        }

		private static Func<SigmaEmbedUrlDto, bool> FilterBy(WidgetRequest request)
		{
			return dto => string.IsNullOrEmpty(request.ReportName)
				|| dto.Name == request.ReportName
				|| (dto.Url?.StartsWith(request.ReportName, StringComparison.OrdinalIgnoreCase) ?? false);
		}

		private static IEnumerable<SigmaEmbedUrlDto> MapFrom(ExpandoObject metadata, Guid reportId)
		{
			var reports = metadata.FirstOrDefault<IEnumerable<object>>(MetadataConstants.Reports)?.Select(x => x as ExpandoObject);

			return reports.Select(report => new SigmaEmbedUrlDto
			{
				Url = report.FirstOrDefault<string>(MetadataConstants.Url),
				Name = report.FirstOrDefault<string>(MetadataConstants.Name),
                EmbedLocation = report.FirstOrDefault<string>(MetadataConstants.EmbedLocation),
				Id = reportId
			});
		}

		private static void AddToReports(List<ExpandoObject> reports, ExpandoObject data)
		{
			if (data != null)
			{
				dynamic report = new ExpandoObject();
				report.name = data.FirstOrDefault<string>(MetadataConstants.Name);
				report.embedPath = data.FirstOrDefault<string>(MetadataConstants.EmbedPath);
                report.embedLocation = data.FirstOrDefault<string>(MetadataConstants.EmbedLocation);

                if (report.name != null || report.embedPath != null)
				{
					reports.Add(report);
				}
			}
		}

		private ExpandoObject ParseMetadata(string json, Guid userId, string connectionId, WidgetRequest request)
		{
			var metadata = json.ToCamelCaseExpandoObject();

			var reports = new List<ExpandoObject>();

			AddToReports(reports, metadata);

			var embedGroup = metadata.FirstOrDefault<IEnumerable<object>>(MetadataConstants.EmbedGroup)?.Select(x => x as ExpandoObject);
			if (embedGroup != null)
			{
				foreach (var report in embedGroup)
				{
					AddToReports(reports, report);
				}
			}

            var hideFolderNavigation = metadata.FirstOrDefault(MetadataConstants.HideFolderNavigation, true);
            var controls = BuildControls(metadata, request);

			foreach (var report in reports)
			{
				var embedPath = report.FirstOrDefault<string>(MetadataConstants.EmbedPath);
				((dynamic)report).url = BuildEmbedUrl(embedPath, userId, connectionId, request.Start, request.End, hideFolderNavigation, controls);
			}

			((dynamic)metadata).reports = reports;

			return metadata;
		}

		public static readonly string Default = null;

		private static readonly Dictionary<string, string> _dayRangeLookup = new()
		{
			{"allDays", Default},
			{"weekDays", "Weekday"},
			{"weekEnds", "Weekend"}
		};

		private static readonly Dictionary<string, string> _businessHourRangeLookup = new()
		{
			{"allHours", Default},
			{"inBusinessHours", "During Business Hours"},
			{"outBusinessHours", "Outside Business Hours"}
		};

		private IDictionary<string, string> BuildControls(ExpandoObject metadata, WidgetRequest request)
		{
			var controls = metadata.AllOrDefault(MetadataConstants.Controls);

            if (Guid.TryParse(request.ScopeId, out var siteId))
            {
                controls.Add("site-id", siteId.ToString());
            }
            else
            {
                controls.Add("scope-id", request.ScopeId);
            }

            controls.Add("customer-id", request.CustomerId.ToString());
			controls.Add("tenant-id", CombineIds(request.TenantIds));

			var dayRange = request.SelectedDayRange.ToRange(_dayRangeLookup);
			if (!string.IsNullOrEmpty(dayRange)) {
				controls.Add("day-of-week", request.SelectedDayRange.ToRange(_dayRangeLookup, Default));
			}

			var hoursRange = request.SelectedBusinessHourRange.ToRange(_businessHourRangeLookup, Default);
			if (!string.IsNullOrEmpty(hoursRange))
			{
				controls.Add("business-hours", hoursRange);
			}

			return controls;
		}

		private string BuildEmbedUrl(string embedPath, Guid userId, string connectionId, DateTime? start, DateTime? end,
			bool hideFolderNavigation, IDictionary<string, string> controls)
		{
			if (string.IsNullOrWhiteSpace(embedPath))
			{
				return null;
			}

            if (string.IsNullOrEmpty(connectionId))
            {
                connectionId = _configuration.GetValue<string>("SigmaConnectionId");

                if (connectionId != null
                    && ((connectionId.StartsWith("%") && connectionId.EndsWith("%")) || connectionId.Contains("CustomerInstance")))
                {
                    connectionId = null;
                }
            }

			var queryParams = new Dictionary<string, string>
			{
				{ ":nonce", SigmaHelper.GetRandomNonce() },
				{ ":time", _dateTimeService.UtcNow.GetUnixTime().ToString() },
				{ ":session_length", _config["SigmaOptions:SessionLength"] },              
                { ":external_user_id", userId.ToString() },
				{ ":eval_connection_id", connectionId?.ToString() },
                { ":account_type", _config["SigmaOptions:AccountType"] },
                { ":mode", _config["SigmaOptions:Mode"] },
                { ":theme", _config["SigmaOptions:Theme"] },
                { ":client_id", _config["SigmaOptions:ClientId"] },
                { ":email", _config["SigmaOptions:Mode"] == "userbacked" ? userId.ToString() + "@willowexternal.com" : null },
                { ":external_user_team", _config["SigmaOptions:Team"] },
                { ":hide_folder_navigation", hideFolderNavigation.ToString() },
            };

            controls?.ToList().ForEach(x => queryParams.Add(x.Key, x.Value));

            queryParams = queryParams.Select(ReplacePlaceholderValue).Where(x => !string.IsNullOrEmpty(x.Value)).ToDictionary();

            if (start == null)
			{
				return SignEmbedUrl(QueryHelpers.AddQueryString(embedPath, queryParams));
			}

			var startDate = start?.ToString("s");
			var endDate = end != null ? end?.ToString("s") : "";

			// date-range should not be URL-encoded, otherwise Sigma can't parse it
			return SignEmbedUrl($"{QueryHelpers.AddQueryString(embedPath, queryParams)}&date-range=min:{startDate},max:{endDate}");
		}

		private string SignEmbedUrl(string embedUrl)
		{
			var signature = SigmaHelper.Sign(_config["SigmaOptions:EmbedSecret"], embedUrl);

			return QueryHelpers.AddQueryString(embedUrl, ":signature", signature);
		}

		private string CombineIds<T>(T[] ids)
		{
			if (ids == null || ids.Length == 0)
			{
				return null;
			}

			if (ids.Length == 1)
			{
				return ids[0].ToString();
			}

			return string.Join(",", ids);
		}

        private KeyValuePair<string, string> ReplacePlaceholderValue(KeyValuePair<string, string> keyValuePair)
        {
            if (keyValuePair.Value == "[value from app service setting]")
            {
                return new KeyValuePair<string, string>(keyValuePair.Key, null);
            }

            return keyValuePair;
        }
    }

	public static class RangeExtensions
	{
		public static T ToRange<T>(this string[] keys, IDictionary<string, T> lookup, T defaultVal = default)
		{
			return (keys?.Length ?? 0) > 0 ? keys[0].ToRange(lookup, defaultVal) : defaultVal;
		}

		public static T ToRange<T>(this string key, IDictionary<string, T> lookup, T defaultVal = default)
		{
			return !string.IsNullOrEmpty(key) && lookup.ContainsKey(key) ? lookup[key] : defaultVal;
		}
	}
}
