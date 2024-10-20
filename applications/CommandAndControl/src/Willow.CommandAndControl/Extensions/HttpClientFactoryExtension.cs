namespace Willow.CommandAndControl.Extensions;

internal static class HttpClientFactoryExtension
{
    public static IServiceCollection AddHttpClientFactory(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IMappedGatewayService, MappedGatewayService>(client =>
        {
            var url = configuration.GetValue<string>("Mapped:BaseUrl");

            if (string.IsNullOrEmpty(url))
            {
                throw new InvalidOperationException("Provide a Mapped:BaseUrl");
            }

            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();

            //TODO: Add PAT from Key Vault
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });

        return services;
    }
}
