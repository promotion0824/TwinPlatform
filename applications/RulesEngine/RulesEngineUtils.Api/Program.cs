using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RulesEngineUtils.Api.Mocks;
using Willow.Rules.Cache;
using Willow.Rules.Configuration;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using WillowRules.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services.AddOptions<CustomerOptions>().BindConfiguration(CustomerOptions.CONFIG);

var customerOptions = new CustomerOptions();
builder.Configuration.Bind(CustomerOptions.CONFIG, customerOptions);
builder.Services.AddOptions<CustomerOptions>().BindConfiguration(CustomerOptions.CONFIG)
	.Configure((b) =>
	{
		b.SQL.CacheExpiration = TimeSpan.FromMinutes(15);
	});
builder.Services.AddSingleton<WillowEnvironment, WillowEnvironment>((s) =>
{
	return new WillowEnvironment(customerOptions);
});

builder.Services.AddSingleton<WillowEnvironmentId, WillowEnvironmentId>((s) =>
{
	var willowEnvironmentId = new WillowEnvironmentId(customerOptions.Id);
	return willowEnvironmentId;
});

builder.Services.AddHttpClient();

builder.Services.AddTransient<IDataCacheFactory, DataCacheFactory>();

builder.Services.AddTransient<RulesContext, RulesContext>((s) =>
{
	var customerOptions = s.GetRequiredService<IOptions<CustomerOptions>>();
	string sqlConnection = customerOptions.Value.SQL.ConnectionString;
	var dbContextOptions = new DbContextOptionsBuilder<RulesContext>()
		.UseSqlServer(sqlConnection, b => b.MigrationsAssembly("WillowRules"))
		.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
		.EnableSensitiveDataLogging()
		.Options;
	return new RulesContext(dbContextOptions);
});

builder.Services.AddTransient<IRepositoryGlobalVariable, RepositoryGlobalVariable>();
builder.Services.AddTransient<IRepositoryCommand, RepositoryCommand>();
builder.Services.AddTransient<IRepositoryInsight, RepositoryInsight>();
builder.Services.AddTransient<IRepositoryInsightChange, RepositoryInsightChange>();
builder.Services.AddTransient<IRepositoryRuleInstanceMetadata, RepositoryRuleInstanceMetadata>();
builder.Services.AddTransient<IRepositoryRuleInstances, RepositoryRuleInstances>();
builder.Services.AddTransient<IRepositoryCalculatedPoint, RepositoryCalculatedPoint>();
builder.Services.AddTransient<IRepositoryRules, RepositoryRules>();
builder.Services.AddTransient<IRepositoryRuleMetadata, RepositoryRuleMetadata>();
builder.Services.AddTransient<IRepositoryRuleExecutions, RepositoryRuleExecutions>();
builder.Services.AddTransient<IRepositoryADTSummary, RepositoryADTSummary>();
builder.Services.AddTransient<IRepositoryProgress, RepositoryProgress>();
builder.Services.AddTransient<IRepositoryActorState, RepositoryActorState>();
builder.Services.AddTransient<IRepositoryTimeSeriesBuffer, RepositoryTimeSeriesBuffer>();
builder.Services.AddTransient<IRepositoryTimeSeriesMapping, RepositoryTimeSeriesMapping>();
builder.Services.AddTransient<IRepositoryRuleExecutionRequest, RepositoryRuleExecutionRequest>();
builder.Services.AddTransient<IRepositoryMLModel, RepositoryMLModel>();

builder.Services.AddSingleton<IEpochTracker, EpochTracker>();

builder.Services.AddSingleton<ITwinService, TwinServiceMock>();
builder.Services.AddSingleton<ITwinSystemService, TwinSystemServiceMock>();
builder.Services.AddSingleton<IADXService, ADXServiceMock>();
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<IMLService, MLService>();

builder.Services.AddSqlServerDistributedCache();

builder.Services.AddDbContextFactory<RulesContext>(b =>
{
	string sqlConnection = customerOptions.SQL.ConnectionString;
	b.UseSqlServer(sqlConnection, c => c.MigrationsAssembly("WillowRules").EnableRetryOnFailure())
		.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
		.EnableSensitiveDataLogging();
});

builder.Services.AddControllers().AddNewtonsoftJson(c =>
{
	c.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
	c.SerializerSettings.Converters.Add(new TokenExpressionJsonConverter());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen((o) =>
{
	o.IncludeXmlComments($"{System.IO.Path.GetDirectoryName(Environment.ProcessPath)}\\RulesEngineUtils.Api.xml");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

var dbContextOptions = new DbContextOptionsBuilder<RulesContext>()
	.UseSqlServer(customerOptions.SQL.ConnectionString, b => b.MigrationsAssembly("WillowRules"))
	.EnableSensitiveDataLogging()
	.Options;

using (var context = new RulesContext(dbContextOptions))
{
	context.Database.Migrate();
}

app.Run();
