using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using PlatformPortalXL.Services.Forge;

namespace PlatformPortalXL.Test.MockServices
{
    public class MockForgeApi : IForgeApi
    {
        public string AccessToken { get; set; }

        public async Task<dynamic> AuthenticateAsync(string clientId, string clientSecret, string grantType, Scope[] scope)
        {
            dynamic result = new ExpandoObject();
            result.access_token = Guid.NewGuid().ToString();
            result.expires_in = 3600;
            return await Task.FromResult(result);
        }

        public async Task<dynamic> CreateBucketAsync(PostBucketsPayload postBuckets)
        {
            dynamic result = new ExpandoObject();
            return await Task.FromResult(result);
        }

        public async Task<dynamic> GetBucketsAsync(string startAt)
        {
            dynamic result = new DynamicJsonResponse();
            var dict = new DynamicDictionary();
            dynamic bucket = new ExpandoObject();
            bucket.bucketKey = "random";
            dict.Dictionary.Add("abc", bucket);
            result.items = dict;
            result.Dictionary = new Dictionary<string, object>();
            return await Task.FromResult(result);
        }

        public async Task<dynamic> GetManifestAsync(string urn)
        {
            dynamic result = new ExpandoObject();
            return await Task.FromResult(result);
        }

        public async Task<dynamic> GetMetadataAsync(string urn)
        {
            dynamic result = new ExpandoObject();
            return await Task.FromResult(result);
        }

        public async Task<dynamic> GetModelViewMetadataAsync(string urn, Guid modelViewId)
        {
            dynamic result = new ExpandoObject();
            return await Task.FromResult(result);
        }

        public async Task<dynamic> GetModelViewPropertiesAsync(string urn, Guid modelViewId)
        {
            dynamic result = new ExpandoObject();
            return await Task.FromResult(result);
        }

        public async Task<dynamic> TranslateAsync(JobPayload job, bool? xAdsForce)
        {
            dynamic result = new ExpandoObject();
            result.result = "success";
            return await Task.FromResult(result);
        }

        public async Task<dynamic> UploadChunkAsync(string bucketKey, string objectName, int? contentLength, string contentRange, string sessionId, Stream body)
        {
            dynamic result = new ExpandoObject();
            result.objectId = $"urn:adsk.objects:os.object:{bucketKey}/{objectName}";
            return await Task.FromResult(result);
        }

        public async Task<dynamic> UploadObjectAsync(string bucketKey, string objectName, int? contentLength, Stream body, string contentDisposition)
        {
            dynamic result = new ExpandoObject();
            return await Task.FromResult(result);
        }
    }
}