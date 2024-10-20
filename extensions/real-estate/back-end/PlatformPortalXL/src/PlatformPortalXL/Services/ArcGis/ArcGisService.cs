using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;

namespace PlatformPortalXL.Services.ArcGis
{
	public interface IArcGisService
	{
		Task<ArcGisDto> GetToken(string referer = null);
		Task<ArcGisLayersDto> GetArcGisLayers();
		Task<string> GetArcGisMapsJson(string token);
	}

	public class ArcGisService : IArcGisService
	{
		private readonly IMemoryCache _cache;
		private readonly ILogger<ArcGisService> _logger;
		private readonly ArcGisOptions _options;
		private readonly IHttpClientFactory _httpClientFactory;
		//The query that is made by the mapsonline.dfwairport.com UI when you select to view all the layers
		private readonly string LayersQuery = "orgid:0123456789ABCDEF -type:\"Code Attachment\" -type:\"Featured Items\" -type:\"Symbol Set\" -type:\"Color Set\" -type:\"Windows Viewer Add In\" -type:\"Windows Viewer Configuration\" -type:\"Map Area\" -typekeywords:\"MapAreaPackage\" -owner:\"esri_apps\" -owner:\"esri\" -type:\"Layer\" -type: \"Map Document\" -type:\"Map Package\" -type:\"Basemap Package\" -type:\"Mobile Basemap Package\" -type:\"Mobile Map Package\" -type:\"ArcPad Package\" -type:\"Project Package\" -type:\"Project Template\" -type:\"Desktop Style\" -type:\"Pro Map\" -type:\"Layout\" -type:\"Explorer Map\" -type:\"Globe Document\" -type:\"Scene Document\" -type:\"Published Map\" -type:\"Map Template\" -type:\"Windows Mobile Package\" -type:\"Layer Package\" -type:\"Explorer Layer\" -type:\"Geoprocessing Package\" -type:\"Desktop Application Template\" -type:\"Code Sample\" -type:\"Geoprocessing Package\" -type:\"Geoprocessing Sample\" -type:\"Locator Package\" -type:\"Workflow Manager Package\" -type:\"Windows Mobile Package\" -type:\"Explorer Add In\" -type:\"Desktop Add In\" -type:\"File Geodatabase\" -type:\"Feature Collection Template\" -type:\"Map Area\" -typekeywords:\"MapAreaPackage\"";
		//The query that is made by the mapsonline.dfwairport.com UI when you select to view all the maps
		private readonly string MapsQuery = "orgid:0123456789ABCDEF (type:(\"Web Map\" OR \"CityEngine Web Scene\") -type:\"Web Mapping Application\")  -type:\"Code Attachment\" -type:\"Featured Items\" -type:\"Symbol Set\" -type:\"Color Set\" -type:\"Windows Viewer Add In\" -type:\"Windows Viewer Configuration\" -type:\"Map Area\" -typekeywords:\"MapAreaPackage\" -owner:\"esri_apps\" -owner:\"esri\" -type:\"Layer\" -type: \"Map Document\" -type:\"Map Package\" -type:\"Basemap Package\" -type:\"Mobile Basemap Package\" -type:\"Mobile Map Package\" -type:\"ArcPad Package\" -type:\"Project Package\" -type:\"Project Template\" -type:\"Desktop Style\" -type:\"Pro Map\" -type:\"Layout\" -type:\"Explorer Map\" -type:\"Globe Document\" -type:\"Scene Document\" -type:\"Published Map\" -type:\"Map Template\" -type:\"Windows Mobile Package\" -type:\"Layer Package\" -type:\"Explorer Layer\" -type:\"Geoprocessing Package\" -type:\"Desktop Application Template\" -type:\"Code Sample\" -type:\"Geoprocessing Package\" -type:\"Geoprocessing Sample\" -type:\"Locator Package\" -type:\"Workflow Manager Package\" -type:\"Windows Mobile Package\" -type:\"Explorer Add In\" -type:\"Desktop Add In\" -type:\"File Geodatabase\" -type:\"Feature Collection Template\" -type:\"Map Area\" -typekeywords:\"MapAreaPackage\"";

		public ArcGisService(
			IMemoryCache cache,
			ILogger<ArcGisService> logger,
			IOptions<ArcGisOptions> options,
			IHttpClientFactory httpClientFactory)
		{
			_cache = cache;
			_logger = logger;
			_options = options.Value;
			_httpClientFactory = httpClientFactory;
		}

		public async Task<ArcGisDto> GetToken(string referer = null)
		{
			var token = await GetTokenAsync(referer);
			return new ArcGisDto
			{
				GisBaseUrl = _options.GisBaseUrl,
				Token = token.Token,
				AuthRequiredPaths = _options.AuthRequiredPaths.Split(","),
				GisPortalPath = _options.GisPortalPath
			};
		}

		private async Task<TokenResponse> GetTokenAsync(string referer = null)
		{
			try
			{
				if (referer != null)
				{
					return await GetTokenFromEndpointAsync(referer);
				}

				var result = await _cache.GetOrCreateAsync(
					"ArcGisPlatform",
					async entry =>
					{
						entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
						return await GetTokenFromEndpointAsync();
					});

				if (string.IsNullOrWhiteSpace(result?.Token))
					throw new HttpRequestException("Failed to get ArcGis token");

				return result;
            }
            catch (Exception ex)
            {
                _logger.LogError("unable to get key from memory cache", ex);
                throw;
            }
        }

		private async Task<TokenResponse> GetTokenFromEndpointAsync(string referer = null)
		{
			using (var client = _httpClientFactory.CreateClient())
			{
				client.BaseAddress = new Uri(_options.GisBaseUrl);

				var content = new FormUrlEncodedContent(new[]
				{
							new KeyValuePair<string, string>("UserName", _options.UserName),
							new KeyValuePair<string, string>("Password", _options.Password),
							new KeyValuePair<string, string>("ip", ""),
							new KeyValuePair<string, string>("client", "referer"),
							new KeyValuePair<string, string>("referer", referer ?? _options.GisPortalPath),
							new KeyValuePair<string, string>("expiration", "1440"),
							new KeyValuePair<string, string>("f", "json")
						});

				var result = await client.PostAsync(_options.GisTokenPath, content);
				result.EnsureSuccessStatusCode();
				return await result.Content.ReadAsAsync<TokenResponse>();
			}
		}

		public async Task<ArcGisLayersDto> GetArcGisLayers()
		{
			var token = await GetTokenAsync();
			using (var client = _httpClientFactory.CreateClient())
			{
				var url = $"{_options.GisBaseUrl}/arcgis/sharing/rest/search?token={token.Token}&f=json&q={LayersQuery}";
				var result = await client.GetAsync(url);
				result.EnsureSuccessStatusCode();
				var json = await result.Content.ReadAsStringAsync();
				var obj = JObject.Parse(json);
				var results = (JArray)obj["results"];
				var layers = results.Select(r => new
				{
					id = (string)r["id"],
					title = (string)r["title"],
					type = (string)r["type"],
					url = (string)r["url"]
				}).ToList();
				var arcGisLayers = new List<ArcGisLayerDto>();
				foreach (var layer in layers)
				{
					arcGisLayers.Add(new ArcGisLayerDto
					{
						Id = layer.id,
						Title = layer.title,
						Type = layer.type,
						Url = layer.url,
					});
				}
				return new ArcGisLayersDto() { Layers = arcGisLayers };
			}
		}

		/// <summary>The query that is made by the mapsonline.dfwairport.com UI when you select to view all the maps.</summary>
		/// <returns>The Json result which is returned by DFW.</returns>
		/// <param name="token">The generated token which is passed.</param>
		public async Task<string> GetArcGisMapsJson(string token)
		{
			using (var client = _httpClientFactory.CreateClient())
			{
				var url = $"{_options.GisBaseUrl}/arcgis/sharing/rest/search?token={token}&f=json&q={MapsQuery}";
				var result = await client.GetAsync(url);
				result.EnsureSuccessStatusCode();
				return await result.Content.ReadAsStringAsync();
			}
		}
	}
}
