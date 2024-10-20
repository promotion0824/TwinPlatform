using Authorization.TwinPlatform.Common.Authorization.Handlers;
using Authorization.TwinPlatform.Common.Extensions;
using Authorization.TwinPlatform.Common.Options;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Web.Auth;
using Authorization.TwinPlatform.Web.Extensions;
using Authorization.TwinPlatform.Web.Middleware;
using Authorization.TwinPlatform.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Willow.Api.Authentication;
using Willow.Api.Common.Extensions;
using Willow.AppContext;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.AzureDigitalTwins.SDK.Extensions;
using Willow.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

//Ensure environment variables are loaded
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddTwinPlatformAuthServices(builder.Configuration, builder.Environment);
builder.Services.AddMicrosoftGraphAPIServices(builder.Configuration.GetSection("GraphAPIApps"));
builder.Services.AddEmailNotificationServices(builder.Configuration.GetSection("HostedServices"));
builder.Services.AddBackendServices();
builder.Services.AddCors();

CancellationTokenSource readinessCancellationTokenSource = new();
builder.Services.AddUMHealthChecks(builder.Configuration, (healthBuilder) => {
    healthBuilder.AddCheck("livez", () => HealthCheckResult.Healthy("System is live."), tags: ["livez"])
    .AddCheck("readyz", () =>
    {
        readinessCancellationTokenSource.Token.ThrowIfCancellationRequested();
        return HealthCheckResult.Healthy("System is ready.");
    }, tags: ["readyz"]);
});

builder.Services.AddUMTelemetry(builder.Configuration);

//Configure Options
builder.Services.Configure<SPAConfig>(builder.Configuration.GetSection(SPAConfig.PropertyName));
builder.Services.Configure<JwtBearerConfig>(builder.Configuration.GetSection(JwtBearerConfig.CONFIG));
builder.Services.Configure<MicrosoftIdentityOptions>(builder.Configuration.GetSection("AzureAdB2C"));
builder.Services.Configure<AppInsightsSettings>(builder.Configuration.GetSection("ApplicationInsights"));
builder.Services.Configure<WillowContextOptions>(builder.Configuration.GetSection("WillowContext"));

// Configure User Management Permission Based Policy Authorization
builder.Services.AddPermissionBasedPolicyAuthorization(builder.Configuration.GetSection(AuthorizationAPIOption.APIName),builder.Environment.IsDevelopment());

// Add Admin Policy handler
builder.Services.AddScoped<IAuthorizationHandler, AuthorizeAdminPolicyHandler>();

// Configure Adt Api Service
var azureAdSection = builder.Configuration.GetSection("AzureAD", AzureADOptions.PopulateDefaults);
builder.Services.Configure<AzureADOptions>(azureAdSection);
builder.Services.Configure<TwinsApiOptions>(builder.Configuration.GetSection("TwinsApi"));
builder.Services.AddAdtApiHttpClient(builder.Configuration.GetSection("TwinsApi"));
builder.Services.AddHttpClient<ITwinsClient, TwinsClient>(Willow.AzureDigitalTwins.SDK.Extensions.ServiceCollectionExtensions.AdtApiClientName);

// Configure User Management Auto Import Settings
builder.Configuration.AddEnvironmentSpecificConfigSource(builder.Configuration.GetSection(AuthorizationAPIOption.APIName).GetValue<string>("InstanceType")!);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    IdentityModelEventSource.ShowPII = true;
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders("Content-Disposition")
    .WithExposedHeaders("x-correlation-id")
    .SetIsOriginAllowed(origin => true) // allow any origin (Development-only)
    .AllowCredentials());
app.UseCors();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.Lifetime.ApplicationStopping.Register(readinessCancellationTokenSource.Cancel);
app.Lifetime.ApplicationStopped.Register(readinessCancellationTokenSource.Cancel);
var applicationName = app.Configuration.GetValue<string>("ApplicationInsights:CloudRoleName");
app.UseWillowHealthChecks(new HealthCheckResponse()
{
    HealthCheckDescription = $"{applicationName} health.",
    HealthCheckName=applicationName!,
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.Run();
