using System.Diagnostics.Metrics;
using System.Reflection;
using System.Security.Claims;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Willow.AdminApp;
using Willow.AppContext;
using Willow.HealthChecks;
using Willow.Telemetry;
using Willow.Telemetry.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the containers.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var settings = builder.Configuration;

var vaultName = settings["Azure:KeyVault:KeyVaultName"];
var vaultUri = $"https://{vaultName}.vault.azure.net/";
var userAssignedClientId = settings["UserAssignedId"];
var credential = builder.Environment.IsDevelopment() ? new DefaultAzureCredential() : new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId });
builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), credential);

builder.Services.AddHttpClient();
builder.Services.ConfigureOpenTelemetry(builder.Configuration);

builder.Services.AddSingleton(sp =>
{
    var willowContext = builder.Configuration.GetSection("WillowContext").Get<WillowContextOptions>();
    return new Meter(willowContext?.MeterOptions.Name ?? "Unknown", willowContext?.MeterOptions.Version ?? "Unknown");
});

var metricsAttributesHelper = new MetricsAttributesHelper(builder.Configuration);
builder.Services.AddSingleton(metricsAttributesHelper);

builder.Services.AddSingleton<WsupOptions>();
builder.Services.AddHostedService<HealthScannerService<WsupOptions>>();
builder.Services.AddSingleton<CustomerInstancesService>();
builder.Services.AddSingleton<OverallStateService>();

builder.Services.AddCors();

// 3: Unable to obtain configuration from: 'https://willowdevb2c.b2clogin.com/a80618f8-f5e9-43bf-a98f-107dd8f54aa9/v2.0/.well-known/openid-configuration'.
//          at Microsoft.IdentityModel.Protocols.Configuration

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
.Configure<ILogger<Program>>((options, logger) =>
{
    options.Authority = "https://willowdevb2c.b2clogin.com/willowdevb2c.onmicrosoft.com/B2C_1A_SeamlessMigration_SignUpOrSignIn";
    options.Audience = "6bb6cec6-8309-4891-9b25-42a3ef3247ec";
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = (context) =>
        {
            logger.LogError(context.Exception, "Error encountered during authentication: {Message}", context.Exception.Message);
            return Task.CompletedTask;
        },
        OnChallenge = (context) =>
        {
            logger.LogInformation("JWT Challenge");
            return Task.CompletedTask;
        }
    };

    /* Note:Authority and audience are enough to validate AD B2C Authentication but if your requirement is to validate the Token issuer & SigningKey also then include the below code as well.*/
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        //ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "https://willowdevb2c.b2clogin.com/a80618f8-f5e9-43bf-a98f-107dd8f54aa9/v2.0/",
        ValidAudience = "6bb6cec6-8309-4891-9b25-42a3ef3247ec",
        // extra ... IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["AzureAdB2C:JwtKey"]))
    };
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddAuthorization((config) =>
{
    config.AddPolicy("Willower", (configurePolicy) =>
    {
        configurePolicy.RequireAuthenticatedUser();
        // TODO: Improve this
        configurePolicy.RequireAssertion((s) => s.User.FindFirst(ClaimTypes.Email)?.Issuer?.StartsWith("https://willowdevb2c.b2clogin.com/") ?? false);
        configurePolicy.RequireAssertion((s) => s.User.FindFirst(ClaimTypes.Email)?.Value?.EndsWith("@willowinc.com") ?? false);
    });

});

builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDeveloperExceptionPage();

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem()
                     .ExecuteAsync(context)));

//app.UseHttpsRedirection();

app.UseCors(builder => builder
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader()
);

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/state", ([FromServices] OverallStateService overallStateService) =>
{
    var overallState = overallStateService.State;
    return overallState;
})
    .WithName("State")
    .WithOpenApi()
    .RequireAuthorization("Willower")
    .RequireCors();

string? GetInformationalVersion() =>
    Assembly
        .GetEntryAssembly()
        ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion;

// Try the informational version
string version = GetInformationalVersion() ?? Assembly.GetExecutingAssembly().GetName()?.Version?.ToString() ?? "";

app.MapGet("/healthz", () =>
{
    return new HealthCheckDto() { Description = "Wsup", Key = "wsup", Status = HealthStatus.Healthy, Version = version };
})
    .WithName("Healthchecks")
    .WithOpenApi()
    .RequireCors();

app.MapGet("/livez", () => { return "Live"; }).WithName("Liveness probe");

app.MapGet("/readyz", () => { app.Lifetime.ApplicationStopping.ThrowIfCancellationRequested(); app.Lifetime.ApplicationStopped.ThrowIfCancellationRequested(); return "Ready"; }).WithName("Readiness probe");

if (!app.Environment.IsDevelopment())  // PRODUCTION
{
    var staticFileOptions = new StaticFileOptions
    {
        FileProvider = new IndexFallbackFileProvider(new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "ClientApp"))),
        ServeUnknownFileTypes = true,
        ContentTypeProvider = new FileExtensionContentTypeProvider(),
        DefaultContentType = "text/html"
    };
    //    app.UseDefaultFiles();
    app.UseStaticFiles(staticFileOptions);

    // // For SPA to work we need any reload of any page to deliver the indedx.html file
    // app.MapFallbackToFile("../ClientApp/index.html", "text/html");
}
else                                  // DEVELOPMENT
{
    var staticFileOptions = new StaticFileOptions
    {
        FileProvider = new IndexFallbackFileProvider(new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "../ClientApp/dist"))),
        ServeUnknownFileTypes = true,
        ContentTypeProvider = new FileExtensionContentTypeProvider(),
        DefaultContentType = "text/html"
    };

    app.UseStaticFiles(staticFileOptions);

    // app.MapGet("/", () => Results.File(File.ReadAllBytes("../ClientApp/dist/index.html"), "text/html")).ShortCircuit();

    // // For SPA to work we need any reload of any page to deliver the indedx.html file
    // app.MapFallbackToFile("../ClientApp/dist/index.html", "text/html");
}

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

app.Run();

