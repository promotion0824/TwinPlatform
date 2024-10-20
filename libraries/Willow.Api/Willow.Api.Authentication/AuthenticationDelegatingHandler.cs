namespace Willow.Api.Authentication
{
    using System.Net;
    using System.Net.Http.Headers;

    /// <summary>
    /// A delegating handler for authentication.
    /// </summary>
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IClientCredentialTokenService clientCredentialTokenService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationDelegatingHandler"/> class.
        /// </summary>
        /// <param name="clientCredentialTokenService">An instance of the ClientCredentialToken service to resolve the access token.</param>
        public AuthenticationDelegatingHandler(IClientCredentialTokenService clientCredentialTokenService)
        {
            this.clientCredentialTokenService = clientCredentialTokenService;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = clientCredentialTokenService.GetClientCredentialToken(cancellationToken: cancellationToken);

            request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.HeaderBearer, token);
            var response = await base.SendAsync(request, cancellationToken);

            // This should never happen due to a token expiring since the cache will be refreshed before it expires.... but just in case.
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                token = clientCredentialTokenService.GetClientCredentialToken(cancellationToken: cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue(AuthenticationSchemes.HeaderBearer, token);
                response = await base.SendAsync(request, cancellationToken);
            }

            return response;
        }
    }
}
