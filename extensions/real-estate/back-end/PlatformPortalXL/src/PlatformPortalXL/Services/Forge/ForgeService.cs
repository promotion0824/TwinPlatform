using Autodesk.Forge;
using Autodesk.Forge.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PlatformPortalXL.Services.GeometryViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Common;

namespace PlatformPortalXL.Services.Forge
{
    public interface IForgeService
    {
        Task<List<ForgeOperationContext>> StartConvertToSvfAsync(Guid siteId, List<IFormFile> files);
        Task<string> GetToken();
    }

    public class ForgeService : IForgeService, IGeometryViewerService
    {
        private readonly Scope[] Scopes = new [] { Scope.BucketCreate, Scope.BucketRead, Scope.BucketUpdate, Scope.DataRead, Scope.DataWrite, Scope.DataCreate };

        private readonly IDateTimeService _dateTimeService;
        private readonly IMemoryCache _cache;
        private readonly ForgeOptions _options;
        private readonly IForgeApi _forgeApi;

        public ForgeService(IDateTimeService dateTimeService, IMemoryCache cache, IOptions<ForgeOptions> options, IForgeApi forgeApi)
        {
            _dateTimeService = dateTimeService;
            _cache = cache;
            _options = options.Value;
            _forgeApi = forgeApi;
        }

        public async Task<List<ForgeOperationContext>> StartConvertToSvfAsync(Guid siteId, List<IFormFile> files)
        {
            var token = await GetToken();
            _forgeApi.AccessToken = token;

            var contexts = new List<ForgeOperationContext>();

            foreach (var file in files)
            {
                var context = new ForgeOperationContext(_forgeApi, siteId, token, file, _options.BucketPostfix);
                contexts.Add(context);

                AbstractOperation operation = new CreateForgeSiteBucketIfNotExistsOperation(context);
                await operation.ExecuteAsync();

                operation = new UploadFileToForgeServerOperation(context, _dateTimeService);
                await operation.ExecuteAsync();

                operation = new TranslateToSvfOperation(context, _options);
                await operation.ExecuteAsync();
            }

            return contexts;
        }

        public async Task<ForgeManifestStatus> CheckStatus(string urn)
        {
            _forgeApi.AccessToken = await GetToken();

            dynamic response = await _forgeApi.GetManifestAsync(urn);

            return Enum.TryParse(response.status, true, out ForgeManifestStatus status) ? status : ForgeManifestStatus.failed;
        }

        public async Task<List<Guid>> GetViewModelIds(string urn)
        {
            _forgeApi.AccessToken = await GetToken();

            var response = await _forgeApi.GetMetadataAsync(urn);

			var data = response.Dictionary["data"];
			var metadata = data.Dictionary["metadata"];
			var modelViews = metadata.Items() as Dictionary<string, object>;

			return modelViews?.Values.Select(x => x.GetGuid("guid"))?.ToList() ?? new List<Guid>();
        }

        public async Task<List<string>> GetViewModelExternalIds(string urn, Guid modelViewId)
        {
            _forgeApi.AccessToken = await GetToken();

            var response = await _forgeApi.GetModelViewPropertiesAsync(urn, modelViewId);

			var data = response.Dictionary["data"];
			var metadata = data.Dictionary["collection"];
			var modelViewProperties = metadata.Items() as Dictionary<string, object>;

			return modelViewProperties?.Values.Select(x => x.Get("externalId").ToString())?.ToList() ?? new List<string>();
        }

        public async Task<List<string>> GetViewModelExternalIds(string urn)
        {
            var externalIds = new List<string>();

            var viewModelIds = await GetViewModelIds(urn);
            foreach(var viewModelId in viewModelIds)
            {
                externalIds.AddRange(await GetViewModelExternalIds(urn, viewModelId));
            } 

            return externalIds;
        }

        public async Task<List<string>> GetGeometryViewerIds(string urn)
        {
            var status = await CheckStatus(urn);

            switch (status)
            {
                case ForgeManifestStatus.pending:
                case ForgeManifestStatus.inprogress: return null;
                case ForgeManifestStatus.success: return await GetViewModelExternalIds(urn);
                default: return new List<string>();
            }
        }

        public async Task<string> GetToken()
        {
            var token = await _cache.GetOrCreateAsync(
                "forge_token",
                async (entry) =>
                {
                    var result = await _forgeApi.AuthenticateAsync(
                        _options.ClientId,
                        _options.ClientSecret,
                        oAuthConstants.CLIENT_CREDENTIALS,
                        Scopes);

                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds((int)result.expires_in);
                    return (result.access_token as string);
                }
            );
            return token;
        }
    }

	public static class DynamicDictionaryExtensions
	{
		public static object Get(this object obj, string id)
		{
			try
			{
				return (obj as DynamicDictionary)?.Dictionary[id];
			}
			catch
			{
				return null;
			}
		}

		public static Guid GetGuid(this object obj, string id)
		{
			var guidid = obj.Get(id)?.ToString();

			return Guid.TryParse(guidid, out Guid guid) ? guid : Guid.Empty;
		}
	}
}
