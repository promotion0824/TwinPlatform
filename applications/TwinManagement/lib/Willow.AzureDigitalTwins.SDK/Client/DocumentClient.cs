using System.Text.Json;

namespace Willow.AzureDigitalTwins.SDK.Client
{
    public partial interface IDocumentsClient
	{
		public Task DownloadDocumentAsync(string DocumentTwinId, string filePath, CancellationToken cancellationToken);
	}

	public partial class DocumentsClient
	{
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            // Override timeout for Documents Client
            client.Timeout = TimeSpan.FromMinutes(10);
        }

        static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
		{
			if (settings == null)
				return;
			settings.PropertyNameCaseInsensitive = true;
		}

		partial void ProcessResponse(HttpClient client, HttpResponseMessage response)
		{
            this.ReadResponseAsString = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DocumentTwinId">Document Twin Identifier</param>
		/// <param name="filePath">File path in the filesystem where the file to be created. Argument Exception if a file already exist in the filepath</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public async Task DownloadDocumentAsync(string DocumentTwinId, string filePath, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(DocumentTwinId))
				throw new ArgumentNullException("DocumentTwinId cannot be null");

			if (File.Exists(filePath))
				throw new ArgumentException("File already exists in the supplied filePath");

			using var fileResponse = await GetDocumentStreamAsync(DocumentTwinId, cancellationToken);
			if (fileResponse.StatusCode == (int)HttpStatusCode.OK)
				using (var fs = new FileStream(filePath, FileMode.CreateNew))
				{
					await fileResponse.Stream.CopyToAsync(fs, cancellationToken);
				}
		}

	}
}
