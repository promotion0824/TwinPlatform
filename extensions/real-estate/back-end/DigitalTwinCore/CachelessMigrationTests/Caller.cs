using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CachelessMigrationTests
{
	public class Caller
	{
		private readonly HttpClient _client;
        private static TokenResponse _token;

		public Caller(int timeoutSeconds = 100)
		{
			_client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
		}

		public async Task<Result> Get(string url, ICollection<KeyValuePair<string,string>> queryParams = null, string continuationToken = null)
		{
            if (queryParams != null && queryParams.Count > 0)
            {
                url = $"{url}?{string.Join('&', queryParams.Select(qp => $"{qp.Key}={qp.Value}"))}";
            }

            _client.DefaultRequestHeaders.TryAddWithoutValidation("ContinuationToken", continuationToken);
            return await Request(url, x => x.GetAsync(url));
		}

        private async Task<Result> Request(string url, Func<HttpClient, Task<HttpResponseMessage>> performRequest)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
            HttpResponseMessage response = null;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Exception exception = null;
            try
            {
                response = await performRequest(_client);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            sw.Stop();

            return new Result
            {
                Error = exception != null ? exception.Message : string.Empty,
                Success = exception == null && response.IsSuccessStatusCode,
                Content = exception == null ? await response.Content.ReadAsStringAsync() : string.Empty,
                Span = sw.Elapsed,
                Url = url,
                ContentLenght = exception == null ? response.Content.Headers.ContentLength : null
            };
        }

        public async Task<Result> Post<T>(string url, T payload)
        {
            return await Request(url, x => x.PostAsJsonAsync<T>(url, payload));
        }

        public async Task<Result> Delete(string url)
        {
            return await Request(url, x => x.DeleteAsync(url));
        }

        private string GetToken()
        {
            if (_token == null && TryGetFromFile(out var token))
            {
                _token = token;
            }

            if (_token == null || DateTime.UtcNow > _token.ExpiresOnUtc)
            {
                _token = RequestToken().GetAwaiter().GetResult();
                _token.ExpiresOnUtc = DateTime.UtcNow.AddSeconds(_token.ExpiresIn - 30);
                File.WriteAllText("TokenResponse.json", JsonSerializer.Serialize(_token));
            }

            return _token.AccessToken;
        }

		private bool TryGetFromFile(out TokenResponse token)
		{
            // Tests are restarted often, to avoid getting token from auth0, retrieve from file.
            try
            {
                var tokenStr = File.ReadAllText("TokenResponse.json");
                token = JsonSerializer.Deserialize<TokenResponse>(tokenStr);
                return true;
            }
            catch (Exception ex)
            {
                token = null;
                return false;
            }
		}

		private async Task<TokenResponse> RequestToken()
        {
            var response = await _client.PostAsync(
                "https://willowtwin-uat.auth0.com/oauth/token", 
                new StringContent(JsonSerializer.Serialize(new
                {
                    client_id = "tG6FQz91sof74x6KWgYHmrBJr5YSRCVL",
                    client_secret = "iHUbqn0olfuqSJtuiL8jSpeZ6iMHIvqsDlfbkqXRQAeDA1EePmbIDUGQZ528YL7H",
                    audience = "https://willowtwin-web-uat",
                    grant_type = "client_credentials"
                }), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to get access token. Http status code: {response.StatusCode}");

            return await response.Content.ReadAsAsync<TokenResponse>();
        }

        class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("id_token")]
            public string IdToken { get; set; }

            [JsonPropertyName("scope")]
            public string Scope { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }

            public DateTime? ExpiresOnUtc { get; set; } = null;
        }
    }

	public class Result
	{
        public string Error { get; set; }
        public bool Success { get; set; }
		public string Content { get; set; }
		public TimeSpan Span { get; set; }
        public string Url { get; set; }
        public long? ContentLenght { get; set; }
	}

    public class TestResult
    {
        public string Name { get; set; }
        public Result CurrentDtCoreResult { get; set; }
        public Result CachelessDtCoreResult { get; set; }
        public bool MatchingContent { get; set; }
    }
}
