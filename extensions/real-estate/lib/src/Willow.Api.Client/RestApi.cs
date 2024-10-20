using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Willow.Common;

namespace Willow.Api.Client
{
    public class RestApi : IRestApi
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiName;

        public RestApi(IHttpClientFactory httpClientFactory, string apiName)
        {
            _httpClientFactory = httpClientFactory;
            _apiName = apiName;
        }

        #region IRestApi

        public Task<T> Get<T>(string url, object headers = null)
        {
            return MakeRequestWithResponse<string, T>(HttpMethod.Get, url, null, headers);
        }

        public Task<TResponse> Post<TResponse>(string url)
        {
            return MakeRequestWithResponse<string, TResponse>(HttpMethod.Post, url, null, null);
        }

        public Task<TResponse> Post<TRequest, TResponse>(string url, TRequest content, object headers = null)
        {
            return MakeRequestWithResponse<TRequest, TResponse>(HttpMethod.Post, url, content, headers);
        }

        public async Task PostCommand<TRequest>(string url, TRequest content, object headers = null)
        {
            await MakeRequest<TRequest>(HttpMethod.Post, url, content, headers);
        }

        public Task<TResponse> Put<TRequest, TResponse>(string url, TRequest content, object headers = null)
        {
            return MakeRequestWithResponse<TRequest, TResponse>(HttpMethod.Put, url, content, headers);
        }

        public Task PutCommand<TRequest>(string url, TRequest content, object headers = null)
        {
            return MakeRequest<TRequest>(HttpMethod.Put, url, content, headers);
        }

        public async Task Delete(string url)
        {
            using (var client = _httpClientFactory.CreateClient(_apiName))
            {
                var response = await client.DeleteAsync(url).ConfigureAwait(false);

                await EnsureSuccessStatusCode(response, url, _apiName);
            }
        }

        public async Task Patch(string url)
        {
            using (var client = _httpClientFactory.CreateClient(_apiName))
            {
                var response = await client.PatchAsync(url, null).ConfigureAwait(false);

                await EnsureSuccessStatusCode(response, url, _apiName);
            }
        }

        public async Task PatchCommand<TRequest>(string url, TRequest content, object headers = null)
        {
            await MakeRequest(HttpMethod.Patch, url, content, headers);
        }

        #endregion

        public static async Task EnsureSuccessStatusCode(HttpResponseMessage message, string url, string apiName, object headers = null)
        {
            if (!message.IsSuccessStatusCode)
            {
                var exceptionMsg = await GetExceptionMessage(message);
                var exception = new RestException(exceptionMsg.Message, message.StatusCode, exceptionMsg.Response);

                exception.ApiName = apiName;
                exception.Url = url;
                exception.Headers = headers;
                exception.ResponseMessage = message;

                throw exception;
            }
        }

        #region Private

        private async Task<HttpResponseMessage> MakeRequest<TRequest>(HttpMethod method, string url, TRequest content, object headers)
        {
            using (var client = _httpClientFactory.CreateClient(_apiName))
            {
                var request = new HttpRequestMessage(method, url);

                if (headers != null)
                {
                    var dHeaders = headers.ToDictionary();

                    foreach (var kv in dHeaders)
                        request.Headers.Add(kv.Key, kv.Value.ToString());
                }

                if (content != null)
                {
                    if (content is HttpContent httpContent)
                        request.Content = httpContent;
                    else
                        request.Content = JsonContent.Create<TRequest>(content);
                }

                var response = await client.SendAsync(request).ConfigureAwait(false);

                await EnsureSuccessStatusCode(response, url, _apiName, headers);

                return response;
            }
        }

        private async Task<TResponse> MakeRequestWithResponse<TRequest, TResponse>(HttpMethod method, string url, TRequest content, object headers)
        {
            var response = await MakeRequest(method, url, content, headers);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TResponse>(result);
        }

        private static async Task<(string Message, string Response)> GetExceptionMessage(HttpResponseMessage responseMessage)
        {
            var responseString = "";

            try
            {
                responseString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (responseMessage?.Content?.Headers?.ContentType?.MediaType == "application/problem+json")
                {
                    var innerError = JsonConvert.DeserializeObject<ErrorResponse>(responseString);

                    if (!string.IsNullOrEmpty(innerError?.Message))
                    {
                        return (innerError.Message, responseString);
                    }
                }
            }
            catch
            {
                // Just return empty response
            }

            return ($"Rest Api Exception, status code = {responseMessage.StatusCode}", responseString);
        }

        private sealed class ErrorResponse
        {
            public int      StatusCode  { get; set; }
            public string   Message     { get; set; }
            public object   Data        { get; set; }
            public string[] CallStack   { get; set; }
        }

        #endregion
    }
}
