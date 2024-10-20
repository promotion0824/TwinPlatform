using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Willow.Proxy;

public interface IHttpProxy
{
    Task ProxyAsync(
        HttpContext context,
        string clientName,
        Uri targetUri,
        IHttpClientFactory httpClientFactory,
        bool useQueryFromCurrentRequest = false,
        params string[] headersToForward);
}
public class HttpProxy : IHttpProxy
{
    private static readonly HashSet<string> _headersToSkipGoingDownstream = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Transfer-Encoding",
    };
    private static readonly HashSet<string> _headersToCopyGoingUpstream = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Content-Type",
        "Accept-Language"
    };

    public async Task ProxyAsync(
        HttpContext context,
        string clientName,
        Uri targetUri,
        IHttpClientFactory httpClientFactory,
        bool useQueryFromCurrentRequest = false,
        params string[] headersToForward)
    {
        var httpClient = httpClientFactory.CreateClient(clientName);

        var outgoingHttpRequest = CreateOutgoingHttpRequest(context, targetUri, useQueryFromCurrentRequest, headersToForward);

        var response = await httpClient.SendAsync(outgoingHttpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            context.RequestAborted);

        await WriteProxiedHttpResponseAsync(context, response);
    }

    private static HttpRequestMessage CreateOutgoingHttpRequest(HttpContext context, Uri targetUri, bool useQueryFromCurrentRequest, params string[] headersToForward)
    {
        var request = context.Request;

        var requestMessage = new HttpRequestMessage(new HttpMethod(request.Method), targetUri);

        var requestMethod = request.Method;

        if (useQueryFromCurrentRequest)
        {
            CopyRequestQueryToOutgoingHttpRequest(context, requestMessage);
        }

        CopyRequestBodyToOutgoingHttpRequest(requestMethod, request, requestMessage);

        CopyRequestHeadersToOutgoingHttpRequest(context, requestMessage, headersToForward);

        return requestMessage;
    }

    private static void CopyRequestQueryToOutgoingHttpRequest(HttpContext context, HttpRequestMessage requestMessage)
    {
        var requestQueryParams = context.Request.Query?.ToDictionary(x => x.Key, x => x.Value.ToString());
        if (requestQueryParams?.Any() == true)
        {
            var queryString = QueryHelpers.AddQueryString(requestMessage.RequestUri.OriginalString, requestQueryParams);
            requestMessage.RequestUri = new Uri(queryString, UriKind.RelativeOrAbsolute);
        }
    }

    private static void CopyRequestBodyToOutgoingHttpRequest(string requestMethod,
        HttpRequest request,
        HttpRequestMessage requestMessage)
    {
        if (!HttpMethods.IsGet(requestMethod) &&
            !HttpMethods.IsHead(requestMethod) &&
            !HttpMethods.IsDelete(requestMethod) &&
            !HttpMethods.IsTrace(requestMethod))
        {
            var streamContent = new StreamContent(request.Body);
            requestMessage.Content = streamContent;
        }
    }

    private static Task WriteProxiedHttpResponseAsync(HttpContext context, HttpResponseMessage responseMessage)
    {
        var response = context.Response;

        response.StatusCode = (int)responseMessage.StatusCode;
        CopyResponseMessageHeaderToHttpResponse(responseMessage, response);

        return CopyHttpResponseContentToHttpResponseBody(responseMessage, response);
    }

    private static Task CopyHttpResponseContentToHttpResponseBody(HttpResponseMessage responseMessage, HttpResponse response)
    {
        if (responseMessage.Content != null)
        {
            return responseMessage.Content.CopyToAsync(response.Body);
        }

        return Task.CompletedTask;
    }

    private static void CopyRequestHeadersToOutgoingHttpRequest(HttpContext context, HttpRequestMessage requestMessage, params string[] headersToForward)
    {
        foreach (var header in context.Request.Headers)
        {
            if (headersToForward.Contains(header.Key))
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
            if (!_headersToCopyGoingUpstream.Contains(header.Key))
            {
                continue;
            }
            if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }
    }

    private static void CopyResponseMessageHeaderToHttpResponse(HttpResponseMessage responseMessage, HttpResponse response)
    {
        CopyHeaders(responseMessage.Headers, response.Headers);
        if (responseMessage.Content != null)
        {
            CopyHeaders(responseMessage.Content.Headers, response.Headers);
        }

        static void CopyHeaders(HttpHeaders source, IHeaderDictionary destination)
        {
            foreach (var header in source)
            {
                if (_headersToSkipGoingDownstream.Contains(header.Key))
                {
                    continue;
                }

                destination.TryAdd(header.Key, new StringValues(header.Value.ToArray()));
            }
        }
    }
}
