using FluentAssertions;
using Moq.Contrib.HttpClient;
using Moq.Language.Flow;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Willow.Tests.Infrastructure
{
    public static class DependencyServiceHttpHandlerExtensions
    {
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
            var collection = new NameValueCollection { { paramName, paramValue } };
            return SetupRequestWithExpectedQueryParameters(handler, method, path, collection);
        }

        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequestWithExpectedFiles(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl, string filesPartName, IEnumerable<ExpectedFile> expectedFiles)
        {
            return handler.SetupRequest(method, requestUrl, async (message) =>
            {
                message.Content.GetType().Should().Be(typeof(MultipartFormDataContent));
                var multipartContent = (MultipartFormDataContent)message.Content;
                var fileParts = multipartContent.Where(part => part.Headers.ContentDisposition.Name == filesPartName).ToList();
                fileParts.Should().NotBeEmpty();
                var requestFiles = fileParts.Select(p => new ExpectedFile
                {
                    FileName = p.Headers.ContentDisposition.FileName,
                    Content = p.ReadAsByteArrayAsync().Result
                });

                requestFiles.Should().BeEquivalentTo(expectedFiles);

                return await Task.FromResult(true);
            });
        }

        public class ExpectedFile
        {
            public string FileName { get; set; }

            public byte[] Content { get; set; }
        }
    }
}
