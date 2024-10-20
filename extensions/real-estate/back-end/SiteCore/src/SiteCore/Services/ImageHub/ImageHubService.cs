using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SiteCore.Services.ImageHub
{
    public interface IImageHubService
    {
        Task<OriginalImageDescriptor> CreateSiteLogo(Guid customerId, Guid siteId, byte[] logoImageContent);
        Task<OriginalImageDescriptor> CreateFloorModule(Guid customerId, Guid siteId, Guid floorId, byte[] planContent, string originalFileName);
        Task DeleteFloorModule(Guid customerId, Guid siteId, Guid floorId, Guid imageId);
    }

    public class ImageHubService : IImageHubService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IImagePathHelper _imagePathHelper;

        public ImageHubService(IHttpClientFactory httpClientFactory, IImagePathHelper imagePathHelper)
        {
			_httpClientFactory = httpClientFactory;
            _imagePathHelper = imagePathHelper;
        }

        public async Task<OriginalImageDescriptor> CreateSiteLogo(
            Guid customerId, 
            Guid siteId, 
            byte[] logoImageContent)
        {
            var path = _imagePathHelper.GetSiteLogoPath(customerId, siteId);
            return await CreateModule(path, logoImageContent);
        }

        public async Task<OriginalImageDescriptor> CreateFloorModule(
            Guid customerId, 
            Guid siteId, 
            Guid floorId, 
            byte[] planContent, 
            string originalFileName)
        {
            var path = _imagePathHelper.GetFloorModulePath(customerId, siteId, floorId);
            return await CreateModule(path, planContent, originalFileName);
        }

        public async Task DeleteFloorModule(Guid customerId, Guid siteId, Guid floorId, Guid imageId)
        {
            var path = _imagePathHelper.GetFloorModulePath(customerId, siteId, floorId, imageId);
            await DeleteModule(path);
        }

        private async Task DeleteModule(string path)
        {
			var client = _httpClientFactory.CreateClient(ApiServiceNames.ImageHub);
			var response = await client.DeleteAsync(path);
            response.EnsureSuccessStatusCode(ApiServiceNames.ImageHub);
        }

        private async Task<OriginalImageDescriptor> CreateModule(string path, byte[] imageContent)
        {
            return await CreateModule(path, imageContent, "originalFileName");
        }

        private async Task<OriginalImageDescriptor> CreateModule(
            string path, 
            byte[] imageContent, 
            string originalFileName)
        {
            var dataContent = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(imageContent)
            {
                Headers = { ContentLength = imageContent.Length }
            };
            dataContent.Add(fileContent, "imageFile", originalFileName);
			var client = _httpClientFactory.CreateClient(ApiServiceNames.ImageHub);
			var response = await client.PostAsync(path, dataContent);
            response.EnsureSuccessStatusCode(ApiServiceNames.ImageHub);
            var strResponse = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<OriginalImageDescriptor>(strResponse);
            return result;
        }
    }

    public class OriginalImageDescriptor
    {
        public Guid ImageId { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
    }
}
