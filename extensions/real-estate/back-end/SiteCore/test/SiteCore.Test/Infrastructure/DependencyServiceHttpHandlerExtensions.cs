using FluentAssertions;
using Moq.Contrib.HttpClient;
using Moq.Language;
using Moq.Language.Flow;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Willow.Infrastructure;

namespace Willow.Tests.Infrastructure
{
    public static class MockHttpMessageHandlerExtensions
    {
        public static IReturnsResult<HttpMessageHandler> ReturnsJson<T>(this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup, T content)
        {
            return setup.ReturnsResponse(JsonSerializerExtensions.Serialize(content), "application/json");
        }

        public static IReturnsResult<HttpMessageHandler> ReturnsJson<T>(this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup, HttpStatusCode statusCode, T content)
        {
            return setup.ReturnsResponse(statusCode, JsonSerializerExtensions.Serialize(content), "application/json");
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsJson<T>(this ISetupSequentialResult<Task<HttpResponseMessage>> setup, T content)
        {
            return setup.ReturnsResponse(JsonSerializerExtensions.Serialize(content), "application/json");
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsJson<T>(this ISetupSequentialResult<Task<HttpResponseMessage>> setup, HttpStatusCode statusCode, T content)
        {
            return setup.ReturnsResponse(statusCode, JsonSerializerExtensions.Serialize(content), "application/json");
        }


        public static IReturnsResult<HttpMessageHandler> ReturnsJsonUsingNewtonsoft<T>(this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup, T content)
        {
            return setup.ReturnsResponse(Newtonsoft.Json.JsonConvert.SerializeObject(content), "application/json");
        }

        public static IReturnsResult<HttpMessageHandler> ReturnsJsonUsingNewtonsoft<T>(this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup, HttpStatusCode statusCode, T content)
        {
            return setup.ReturnsResponse(statusCode, Newtonsoft.Json.JsonConvert.SerializeObject(content), "application/json");
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsJsonUsingNewtonsoft<T>(this ISetupSequentialResult<Task<HttpResponseMessage>> setup, T content)
        {
            return setup.ReturnsResponse(Newtonsoft.Json.JsonConvert.SerializeObject(content), "application/json");
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsJsonUsingNewtonsoft<T>(this ISetupSequentialResult<Task<HttpResponseMessage>> setup, HttpStatusCode statusCode, T content)
        {
            return setup.ReturnsResponse(statusCode, Newtonsoft.Json.JsonConvert.SerializeObject(content), "application/json");
        }

        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl)
        {
            return handler.HttpHandler.SetupRequest(method, $"{handler.BaseUrl}/{requestUrl}");
        }

        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
        {
            return handler.HttpHandler.SetupRequest(method, $"{handler.BaseUrl}/{requestUrl}", match);
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl)
        {
            return handler.HttpHandler.SetupRequestSequence(method, $"{handler.BaseUrl}/{requestUrl}");
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
        {
            return handler.HttpHandler.SetupRequestSequence(method, $"{handler.BaseUrl}/{requestUrl}", match);
        }

        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequestWithExpectedBody<TRequestJsonType>(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl, TRequestJsonType expectedRequestBody)
        {
            return handler.SetupRequest(method, requestUrl, async(message) =>
            {
                var stringContent = await message.Content.ReadAsStringAsync();
                var requestBody = JsonSerializerExtensions.Deserialize<TRequestJsonType>(stringContent);
                requestBody.Should().BeEquivalentTo(expectedRequestBody);

                return true;
            });
        }

        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequestWithExpectedFileContent(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl, string fileParameterName, byte[] expectedFileBytes)
        {
            return handler.SetupRequest(method, requestUrl, async(message) =>
            {
                message.Content.GetType().Should().Be(typeof(MultipartFormDataContent));
                var multipartContent = (MultipartFormDataContent)message.Content;
                var filePart = multipartContent.FirstOrDefault(part => part.Headers.ContentDisposition.Name == fileParameterName);
                filePart.Should().NotBeNull();
                var bytes = await filePart.ReadAsByteArrayAsync();
                bytes.Should().BeEquivalentTo(expectedFileBytes);

                return true;
            });
        }

        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequestWithExpectedQueryParameters(this DependencyServiceHttpHandler handler, HttpMethod method, string path, NameValueCollection parameters)
        {
            return handler.HttpHandler.SetupRequest(message =>
            {
                if (message.Method != method)
                {
                    return false;
                }

                if (message.RequestUri.AbsolutePath.TrimStart('/') != path)
                {
                    return false;
                }

                var expetedParameters = parameters.AllKeys
                    .SelectMany(parameters.GetValues, (k, v) => $"{k.ToLowerInvariant()}={v.ToLowerInvariant()}")
                    .Distinct()
                    .ToList();

                var queryParams = HttpUtility.ParseQueryString(message.RequestUri.Query);

                var actualParameters = queryParams.AllKeys
                    .SelectMany(queryParams.GetValues, (k, v) => $"{k.ToLowerInvariant()}={v.ToLowerInvariant()}")
                    .Distinct()
                    .ToList();

                var intersection = expetedParameters.Intersect(actualParameters).ToList();
                if (intersection.Count != expetedParameters.Count)
                {
                    return false;
                }

                return true;
            });
        }

        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequestWithExpectedQueryParameter(this DependencyServiceHttpHandler handler, HttpMethod method, string path, string paramName, string paramValue)
        {
            var collection = new NameValueCollection();
            collection.Add(paramName, paramValue);
            return SetupRequestWithExpectedQueryParameters(handler, method, path, collection);
        }
    }
}
