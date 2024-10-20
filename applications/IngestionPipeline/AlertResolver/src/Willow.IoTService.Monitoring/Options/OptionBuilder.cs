using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Willow.IoTService.Monitoring.Options
{
    public class OptionBuilder : IOptionBuilder
    {
        public IServiceCollection Services { get; }
        public IConfiguration Configuration { get; }
        public string? BaseKey { get; }

        public OptionBuilder(IServiceCollection services, IConfiguration configuration, string? baseKey)
        {
            Services = services;
            Configuration = configuration;
            BaseKey = baseKey;
        }
    }

    public interface IOptionBuilder
    {
        IServiceCollection Services { get; }
        IConfiguration Configuration { get; }
        string? BaseKey { get; }
    }
}