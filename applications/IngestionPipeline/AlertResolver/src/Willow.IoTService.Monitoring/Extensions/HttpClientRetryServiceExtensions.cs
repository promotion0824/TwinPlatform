using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace Willow.IoTService.Monitoring.Extensions
{
    public static class HttpClientRetryServiceExtensions
    {
        public static void ConfigureClient<TClient, TImplementation>(
            this IServiceCollection services,
            int medianRetryDelaySeconds = 2,
            int retryCount = 2,
            bool treat404AsTransientError = true)
            where TClient : class
            where TImplementation : class, TClient
        {
            // Use typed client instead of named client
            // to let DI container handles the HttpClient lifetime.
            services.AddHttpClient<TClient, TImplementation>()
                    .AddPolicyHandler(GetRetryPolicy<TImplementation>(medianRetryDelaySeconds, retryCount, treat404AsTransientError));
        }

        public static void ConfigureClient<TClient, TImplementation>(
            this IServiceCollection services,
            Action<IServiceProvider, HttpClient> configureAct,
            int medianRetryDelaySeconds = 2,
            int retryCount = 2,
            bool treat404AsTransientError = true)
            where TClient : class
            where TImplementation : class, TClient
        {
            // Use typed client instead of named client
            // to let DI container handles the HttpClient lifetime.
            services.AddHttpClient<TClient, TImplementation>(configureAct)
                    .AddPolicyHandler(GetRetryPolicy<TImplementation>(medianRetryDelaySeconds, retryCount, treat404AsTransientError));
        }

        public static void ConfigureClient<TClient, TImplementation>(
            this IServiceCollection services,
            Action<IServiceProvider, HttpClient> configureAct,
            HttpClientHandler handler,
            int medianRetryDelaySeconds = 2,
            int retryCount = 2,
            bool treat404AsTransientError = true)
            where TClient : class
            where TImplementation : class, TClient
        {
            // Use typed client instead of named client
            // to let DI container handles the HttpClient lifetime.
            services.AddHttpClient<TClient, TImplementation>(configureAct)
                    .AddPolicyHandler(GetRetryPolicy<TImplementation>(medianRetryDelaySeconds, retryCount, treat404AsTransientError))
                    .ConfigurePrimaryHttpMessageHandler(() => GetHandler(handler));
        }

        private static Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> GetRetryPolicy<T>(
            int medianRetryDelaySeconds,
            int retryCount,
            bool treat404AsTransientError)
            => (services, request) =>
            {
                // Jitter backoff.
                var delay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(medianRetryDelaySeconds), retryCount);

                var policy = HttpPolicyExtensions.HandleTransientHttpError();

                if (treat404AsTransientError)
                {
                    policy.OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound);
                }

                return policy.WaitAndRetryAsync(delay, LogOnRetryAct<T>(services));
            };

        private static Action<DelegateResult<HttpResponseMessage>, TimeSpan, int, Context> LogOnRetryAct<T>(
            IServiceProvider services)
            => (outcome,
                timeSpan,
                retryAttempt,
                context) =>
            {
                var logger = services.GetService<ILogger<T>>();
                logger?.LogWarning(outcome.Exception,
                                   "Delaying for {Delay}ms, then making retry {Retry}",
                                   timeSpan.TotalMilliseconds,
                                   retryAttempt);
            };

        private static HttpClientHandler GetHandler(HttpClientHandler handler)
        {
            return new HttpClientHandler()
            {
                AllowAutoRedirect = handler.AllowAutoRedirect,
                AutomaticDecompression = handler.AutomaticDecompression,
                CheckCertificateRevocationList = handler.CheckCertificateRevocationList,
                ClientCertificateOptions = handler.ClientCertificateOptions,
                CookieContainer = handler.CookieContainer,
                Credentials = handler.Credentials,
                DefaultProxyCredentials = handler.DefaultProxyCredentials,
                MaxAutomaticRedirections = handler.MaxAutomaticRedirections,
                MaxConnectionsPerServer = handler.MaxConnectionsPerServer,
                MaxRequestContentBufferSize = handler.MaxRequestContentBufferSize,
                MaxResponseHeadersLength = handler.MaxResponseHeadersLength,
                PreAuthenticate = handler.PreAuthenticate,
                Proxy = handler.Proxy,
                ServerCertificateCustomValidationCallback = handler.ServerCertificateCustomValidationCallback,
                SslProtocols = handler.SslProtocols,
                UseCookies = handler.UseCookies,
                UseDefaultCredentials = handler.UseDefaultCredentials,
                UseProxy = handler.UseProxy
            };
        }
    }
}