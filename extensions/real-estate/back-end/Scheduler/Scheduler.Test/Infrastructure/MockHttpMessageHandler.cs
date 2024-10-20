using System.Net;

namespace Scheduler.Test.Infrastructure
{
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage _fakeResponse;
        private readonly string _path;
        public MockHttpMessageHandler(HttpResponseMessage responseMessage, string path)
        {
            _fakeResponse = responseMessage;
            _path = path;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if(request?.RequestUri?.AbsolutePath == _path)
            {
                return await Task.FromResult(_fakeResponse);
            } else
            {
                return await Task.FromResult(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("Not Found")
                });
            }
            
        }
    }
}
