
namespace Authorization.TwinPlatform.Permission.Api;
/// <summary>
/// Program class for startup
/// </summary>
public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		//Configure Logging
		Startup.ConfigureLogging(builder);

		// Add services to the container.
		Startup.ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

		//Build the Web Application
		var app = builder.Build();

        //Migrate Database
        Startup.MigrateDatabase(builder.Configuration,app.Services);

       // Configure the HTTP request pipeline.
        Startup.Configure(app);

		app.Run();
	}
}
