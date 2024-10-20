using System.Threading.Tasks;

namespace Willow.Api.Client
{
    /// <summary>
    /// Interface for calling a REST API
    /// </summary>
    public interface IRestApi
    {
        Task<T> Get<T>(string url, object headers = null);

        Task<TResponse> Post<TResponse>(string url);

        Task<TResponse> Post<TRequest, TResponse>(string url, TRequest content, object headers = null);

        Task PostCommand<TRequest>(string url, TRequest content, object headers = null);

        Task<TResponse> Put<TRequest, TResponse>(string url, TRequest content, object headers = null);

        Task PutCommand<TRequest>(string url, TRequest content, object headers = null);

        Task Patch(string url);

        Task PatchCommand<TRequest>(string url, TRequest content, object headers = null);

        Task Delete(string url);
    }
}
