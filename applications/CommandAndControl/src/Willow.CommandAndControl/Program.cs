using Azure.Core;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Willow.Email.SendGrid;
using Willow.HealthChecks;
using Willow.Hosting.Web;

var assemblyNameObj = Assembly.GetExecutingAssembly().GetName();
var version = assemblyNameObj.Version?.ToString(2) ?? "0.0";
const string AppName = "ActiveControl";

return WebApplicationStart.Run(args, "ActiveControl", Configure, ConfigureApp, ConfigureHealthChecks);

void Configure(WebApplicationBuilder builder)
{
    builder.Services.AddHttpLogging(o => { });

    builder.Services.AddMemoryCache();

    builder.Services.AddAuthentication()
                    .AddMicrosoftIdentityWebApi(jwtOptions => { }, identityOptions => builder.Configuration.Bind("AzureAdAuth", identityOptions), Constants.AzureAd);

    builder.Services.AddAuthentication()
                    .AddMicrosoftIdentityWebApi(jwtOptions => { jwtOptions.Audience = builder.Configuration["AzureAdB2C:Audience"]; }, identityOptions => builder.Configuration.Bind("AzureAdB2C", identityOptions), Constants.AzureAdB2C);

    // Default Schema to use has selector based on a header key
    builder.Services.AddAuthentication(
            options =>
            {
                options.DefaultScheme = "SchemeSelection";
            })
            .AddPolicyScheme("SchemeSelection", "SchemeSelection", options =>
                {
                    options.ForwardDefaultSelector = context => context.Request.Headers["Authorization-Scheme"].SingleOrDefault() ?? Constants.AzureAdB2C;
                });

    if (!string.IsNullOrWhiteSpace(builder.Configuration.GetValue<string>("AuthorizationAPI:BaseAddress")) &&
        !string.IsNullOrWhiteSpace(builder.Configuration.GetValue<string>("AuthorizationAPI:TokenAudience")))
    {
        // Registers the IUserAuthorizationService
        builder.Services.AddPermissionBasedPolicyAuthorization(builder.Configuration.GetSection("AuthorizationAPI"));
    }

    builder.Services.AddClientCredentialToken(builder.Configuration);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddSingleton<IClientCredentialTokenService, ClientCredentialTokenService>();
    builder.Services.AddTransient<ICurrentHttpContext, CurrentHttpContext>();

    builder.Services.AddAuthorization();

    builder.Services.AddScoped<IActivityLogger, ActivityLogger>();
    builder.Services.AddScoped<IUserInfoService, UserInfoService>();
    builder.Services.AddSingleton<IMappedGatewayService, MockMappedGatewayService>();
    builder.Services.AddTransient<IWillowConnectorCommandSender, WillowConnectorCommandSender>();

    builder.Services.AddProblemDetails();

    builder.Services.AddSingleton<IDbTransactions, DbTransactions>();
    builder.Services.AddTransient<BaseEntitySaveChangesInterceptor>();
    builder.Services.AddDbContext<IApplicationDbContext, ApplicationDbContext>((serviceProvider, options) =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.ExecutionStrategy(deps => new CustomAzureSqlExecutionStrategy(deps, 6, TimeSpan.FromSeconds(30), null, serviceProvider.GetRequiredService<ILogger<CustomAzureSqlExecutionStrategy>>()));
        });
    });
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<IActionContextAccessor, ActionContextAccessor>();
    builder.Services.AddValidators();
    builder.Services.AddWillowAdxService(options => builder.Configuration.Bind("Adx", options));
    builder.Services.AddTransient<ITwinInfoService, TwinInfoService>();

    builder.Services.AddOptions<ServiceBusOptions>().Bind(builder.Configuration.GetSection(ServiceBusOptions.CONFIG));
    builder.Services.AddSingleton(serviceProvider =>
    {
        var serviceBusOptions = serviceProvider.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
        var retryOptions = new ServiceBusRetryOptions
        {
            Mode = ServiceBusRetryMode.Exponential,
            MaxRetries = 5,
            MaxDelay = TimeSpan.FromMinutes(5),
            Delay = TimeSpan.FromSeconds(30),
        };
        var clientOptions = new ServiceBusClientOptions { RetryOptions = retryOptions };
        var tokenCredential = serviceProvider.GetRequiredService<TokenCredential>();
        return new ServiceBusClient(serviceBusOptions.FullyQualifiedNamespace, tokenCredential, clientOptions);
    });

    builder.Services.AddSingleton<PeriodicCommandExecutorJob>();
    builder.Services.AddSingleton<HealthCheckCommandExecutor>();
    builder.Services.AddHostedService(provider => provider.GetRequiredService<PeriodicCommandExecutorJob>());
    builder.Services.AddHostedService<CommandStatusProcessor>();

    builder.Services.AddOptions<ADB2COptions>().BindConfiguration(ADB2COptions.CONFIG);

    builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = $"{AppName} API",
            Description = $"{AppName} API",
            Version = version,
        });
        options.IncludeXmlComments($"{assemblyNameObj.Name}.xml");
        options.MapType<FileStreamHttpResult>(() =>
        {
            return new Microsoft.OpenApi.Models.OpenApiSchema
            {
                Type = "string",
                Format = "binary",
            };
        });
    });

    builder.Services.AddCors(builder =>
    {
        builder.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin();
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
        });
    });

    builder.Services.AddSendGrid();

    builder.Services.Configure<ContactUs>(options => builder.Configuration.Bind("ContactUs", options));
}

async ValueTask ConfigureApp(WebApplication app)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    if (app.Environment.IsDevelopment())
    {
        app.UseCors();
    }

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", $"{AppName} API v{version}");
        c.OAuthClientId(app.Configuration["AzureAdB2C:ClientId"]);
        c.OAuthUsePkce();
        c.OAuthScopeSeparator(" ");
    });

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGroup("/api").MapApplicationEndpoints();

    app.MapFallbackToFile("index.html");

    //DB Migration
    app.Services.GetRequiredService<IServiceProvider>()
        .CreateScope()
        .ServiceProvider.GetRequiredService<ApplicationDbContext>()
        .Database.Migrate();

    // Import permissions when the application starts
    await app.Services.GetRequiredService<IImportService>().ImportDataFromConfigLazy();
}

static void ConfigureHealthChecks(IHealthChecksBuilder healthChecksBuilder)
{
    healthChecksBuilder.AddAuthz();
}
