using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;
using Willow.HealthChecks;
using Willow.Infrastructure.Entities;
using Willow.Telemetry.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    o.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddOpenApiDocument(c => { c.Version = "v1"; c.Title = "Willow.Support.Api"; });
builder.Services.ConfigureOpenTelemetry(builder.Configuration);

builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

var settings = builder.Configuration;

builder.Services.AddDbContext<WsupDbContext>(options =>
{
    var connectionString = settings["DBServerConnectionString"];

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string is missing");
    }

    Console.WriteLine("Connection string: {0}", connectionString);

    SqlConnection sqlConnection = new SqlConnection(connectionString);

    options.UseSqlServer(sqlConnection, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5,
                                        maxRetryDelay: TimeSpan.FromSeconds(60),
                                        errorNumbersToAdd: null);
    });
});

builder.Services.AddProblemDetails();

//if (!builder.Environment.IsDevelopment())
//{
    builder.Services.AddAuthentication()
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.Authority = builder.Configuration["Issuer"];
            options.Audience = builder.Configuration["Audience"];
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidateAudience = true;
            options.IncludeErrorDetails = true;
        });

    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("WsupContributor", p => p.RequireRole("WsupApi-Reader"));
//}

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem()
                     .ExecuteAsync(context)));

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers()
    .RequireAuthorization("WsupContributor");

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
    .WithName("Healthchecks");

app.MapGet("/livez", () => { return "Live"; }).WithName("Liveness probe");

app.MapGet("/readyz", () => { app.Lifetime.ApplicationStopping.ThrowIfCancellationRequested(); app.Lifetime.ApplicationStopped.ThrowIfCancellationRequested(); return "Ready"; }).WithName("Readiness probe");

try
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<WsupDbContext>().Database.Migrate();
    Console.WriteLine("Database migration complete");
}
catch (Exception ex)
{
    Console.WriteLine("Error occurred while migrating the database.");
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.StackTrace);
    Console.WriteLine(ex.InnerException?.Message);
    throw;
}

Console.WriteLine("Starting app...");
app.Run();
