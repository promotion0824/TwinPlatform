using Willow.TwinLifecycleManagement.Web.Extensions;

namespace Willow.TwinLifecycleManagement.Web;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddJsonFile("appsettings.json", optional: false)
                                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                                .AddEnvironmentVariables("tlm_");

        builder.Configuration.AddUMEnvironmentSpecificConfigSource();

        var startup = new Startup(builder.Configuration, builder.Environment);
        startup.ConfigureServices(builder.Services);

        var app = builder.Build();

        startup.Configure(app,app.Lifetime);
        app.Run();
    }
}
