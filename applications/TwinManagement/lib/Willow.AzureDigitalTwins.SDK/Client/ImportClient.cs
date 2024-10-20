
using Willow.Model.Async;

namespace Willow.AzureDigitalTwins.SDK.Client
{
	public partial interface IImportClient
	{
		public Task<CancelResponseJob> CancelImportJobAsync(string id, string userId, CancellationToken cancellationToken = default(CancellationToken));
	}

	public partial class ImportClient
	{
		static partial void UpdateJsonSerializerSettings(System.Text.Json.JsonSerializerOptions settings)
		{
			if (settings == null)
				return;
			settings.PropertyNameCaseInsensitive = true;
		}

		partial void ProcessResponse(HttpClient client, HttpResponseMessage response)
		{
			this.ReadResponseAsString = true;
		}

		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <summary>
		/// Cancel async job by id
		/// </summary>
		/// <param name="id">Async job id</param>
		/// <param name="userId">User id</param>
		/// <returns>Cancel Job Reponse which provides the httprequests status code</returns>
		/// <exception>No exception will be thrown if request failed.</exception>
		public virtual async System.Threading.Tasks.Task<CancelResponseJob> CancelImportJobAsync(string id, string userId, CancellationToken cancellationToken = default(CancellationToken))
		{
			var urlBuilder_ = new System.Text.StringBuilder();
			urlBuilder_.Append("Import/cancel/{id}");
			urlBuilder_.Replace("{id}", System.Uri.EscapeDataString(ConvertToString(id, System.Globalization.CultureInfo.InvariantCulture)));

			var client_ = _httpClient;
			var disposeClient_ = false;
			try
			{
				using (var request_ = new HttpRequestMessage())
				{

					if (!string.IsNullOrEmpty(userId))
						request_.Headers.TryAddWithoutValidation("User-Id", ConvertToString(userId, System.Globalization.CultureInfo.InvariantCulture));
					request_.Method = new HttpMethod("GET");

					PrepareRequest(client_, request_, urlBuilder_);

					var url_ = urlBuilder_.ToString();
					request_.RequestUri = new Uri(url_, UriKind.RelativeOrAbsolute);

					PrepareRequest(client_, request_, url_);

					var response_ = await client_.SendAsync(request_, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
					var disposeResponse_ = true;
					try
					{
						var headers_ = Enumerable.ToDictionary(response_.Headers, h_ => h_.Key, h_ => h_.Value);
						if (response_.Content != null && response_.Content.Headers != null)
						{
							foreach (var item_ in response_.Content.Headers)
								headers_[item_.Key] = item_.Value;
						}

						ProcessResponse(client_, response_);

						return new CancelResponseJob(response_.StatusCode);
					}
					finally
					{
						if (disposeResponse_)
							response_.Dispose();
					}
				}
			}
			finally
			{
				if (disposeClient_)
					client_.Dispose();
			}
		}

	}
}
