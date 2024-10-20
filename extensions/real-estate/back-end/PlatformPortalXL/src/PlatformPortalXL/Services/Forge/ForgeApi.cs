using System;
using System.IO;
using System.Threading.Tasks;
using Autodesk.Forge;
using Autodesk.Forge.Model;

namespace PlatformPortalXL.Services.Forge
{
    public interface IForgeApi
    {
        string AccessToken { get; set; }
        Task<dynamic> AuthenticateAsync(string clientId, string clientSecret, string grantType, Scope[] scope);

        Task<dynamic> CreateBucketAsync(PostBucketsPayload postBuckets);
        Task<dynamic> GetBucketsAsync(string startAt);

        Task<dynamic> UploadObjectAsync(string bucketKey, string objectName, int? contentLength, Stream body, string contentDisposition);
        Task<dynamic> UploadChunkAsync(string bucketKey, string objectName, int? contentLength, string contentRange, string sessionId, Stream body);

        Task<dynamic> TranslateAsync(JobPayload job, bool? xAdsForce);
        Task<dynamic> GetManifestAsync(string urn);
        Task<dynamic> GetMetadataAsync(string urn);
        Task<dynamic> GetModelViewMetadataAsync(string urn, Guid modelViewId);
        Task<dynamic> GetModelViewPropertiesAsync(string urn, Guid modelViewId);
    }

    public class ForgeApi : IForgeApi
    {
        public string AccessToken { get; set; }

        public async Task<dynamic> AuthenticateAsync(string clientId, string clientSecret, string grantType, Scope[] scope)
        {
            var oauth = new TwoLeggedApi();
            return await oauth.AuthenticateAsync(clientId, clientSecret, grantType, scope);
        }

        public async Task<dynamic> CreateBucketAsync(PostBucketsPayload postBuckets)
        {
            var api = new BucketsApi();
            api.Configuration.AccessToken = AccessToken;
            return await api.CreateBucketAsync(postBuckets);
        }

        public async Task<dynamic> GetBucketsAsync(string startAt)
        {
            var api = new BucketsApi();
            api.Configuration.AccessToken = AccessToken;
            return await api.GetBucketsAsync(startAt: startAt);
        }

        public async Task<dynamic> UploadObjectAsync(string bucketKey, string objectName, int? contentLength, Stream body, string contentDisposition)
        {
            var api = new ObjectsApi();
            api.Configuration.AccessToken = AccessToken;
            return await api.UploadObjectAsync(bucketKey, objectName, contentLength, body, contentDisposition);
        }

        public async Task<dynamic> UploadChunkAsync(string bucketKey, string objectName, int? contentLength, string contentRange, string sessionId, Stream body)
        {
            var api = new ObjectsApi();
            api.Configuration.AccessToken = AccessToken;
            return await api.UploadChunkAsync(bucketKey, objectName, contentLength, contentRange, sessionId, body);
        }

        public async Task<dynamic> TranslateAsync(JobPayload job, bool? xAdsForce)
        {
            var api = new DerivativesApi();
            api.Configuration.AccessToken = AccessToken;
            return await api.TranslateAsync(job, xAdsForce);
        }

        public async Task<dynamic> GetManifestAsync(string urn)
        {
            var api = new DerivativesApi();
            api.Configuration.AccessToken = AccessToken;
            return await api.GetManifestAsync(urn);
        }

        public async Task<dynamic> GetMetadataAsync(string urn)
        {
            var api = new DerivativesApi();
            api.Configuration.AccessToken = AccessToken;
            return await api.GetMetadataAsync(urn);
        }

        public async Task<dynamic> GetModelViewMetadataAsync(string urn, Guid modelViewId)
        {
            var api = new DerivativesApi();
            api.Configuration.AccessToken = AccessToken;
            return await api.GetModelviewMetadataAsync(urn, modelViewId.ToString());
        }

        public async Task<dynamic> GetModelViewPropertiesAsync(string urn, Guid modelViewId)
        {
            var api = new DerivativesApi();
            api.Configuration.AccessToken = AccessToken;
            return await api.GetModelviewPropertiesAsync(urn, modelViewId.ToString());
        }
    }
}
