using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostFormAsync(this HttpClient client, string url, Connector postData, CancellationToken cancellationToken)
        {
            var content = new FormUrlEncodedContent(postData.ToFormData(true).AsEnumerable());

            var responseMessage = await client.PostAsync(url, content, cancellationToken);

            return responseMessage;
        }

        public static async Task<HttpResponseMessage> PostFormAsync(this HttpClient client, string url, Connector postData)
        {
            return await client.PostFormAsync(url, postData, CancellationToken.None);
        }
        
        public static async Task<HttpResponseMessage> PutFormAsync(this HttpClient client, string url, Connector postData)
        {
            var content = new FormUrlEncodedContent(postData.ToFormData(true).AsEnumerable());

            var responseMessage = await client.PutAsync(url, content, CancellationToken.None);

            return responseMessage;
        }
    }
}
