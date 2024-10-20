namespace Willow.AdminApp;

using AdminApp.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Metrics;
using Willow.HealthChecks;

/// <summary>
/// Service that periodically scans the health of the application and its dependencies.
/// </summary>
public class HealthScannerService<TOptions> : BackgroundService
    where TOptions : WsupOptions
{
    private readonly IHttpClientFactory clientFactory;
    private readonly OverallStateService overallStateService;
    private readonly CustomerInstancesService customerInstancesService;
    private readonly ILogger<HealthScannerService<TOptions>> logger;
    private readonly Counter<long> healthCheckCounter;

    public HealthScannerService(IHttpClientFactory clientFactory,
        OverallStateService overallStateService,
        CustomerInstancesService customerInstancesService,
        IMeterFactory meterFactory,
        IOptions<TOptions> options,
        ILogger<HealthScannerService<TOptions>> logger)
    {
        this.clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        this.overallStateService = overallStateService ?? throw new ArgumentNullException(nameof(overallStateService));
        this.customerInstancesService = customerInstancesService ?? throw new ArgumentNullException(nameof(customerInstancesService));
        this.logger = logger;
        var meterOptions = options.Value.MeterOptions;
        var meter = meterFactory.Create(meterOptions.Name, meterOptions.Version, meterOptions.Tags);
        healthCheckCounter = meter.CreateCounter<long>("HealthCheck", null, "Count of Health Checks by Customer Instance and Application", options.Value.MeterOptions.Tags ?? Array.Empty<KeyValuePair<string, object?>>());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var overallState = overallStateService.State;

        var httpClient = clientFactory.CreateClient("HealthCheck");
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // startup delay

        logger.LogInformation("HealthScannerService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var customerHttpClient = await customerInstancesService.GetHttpClient();

            // Get all the data we need to scan the health of the applications
            var customerInstances = await customerInstancesService.GetCustomerInstancesAsync(customerHttpClient);
            var allCustomerInstanceApplications = await customerInstancesService.GetCustomerInstanceApplicationsAsync(customerHttpClient);
            var applications = await customerInstancesService.GetApplicationsAsync(customerHttpClient);
            var stamps = await customerInstancesService.GetStampsAsync(customerHttpClient);

            Parallel.ForEach(customerInstances, async (customerInstance) =>
            {
                var stamp = stamps.First(s => s.Id == customerInstance.StampId);
                var custInstance = customerInstance.ToCust(stamp);

                try
                {
                    List<HealthStatus> statuses = [];
                    var customerInstanceApplications = allCustomerInstanceApplications.Where(cia => cia.CustomerInstanceId == customerInstance.Id);
                    var customerInstanceApplicationIds = customerInstanceApplications.Select(cia => cia.ApplicationId).ToList();
                    var isNewBuild = applications.Any(a => customerInstanceApplicationIds.Contains(a.Id) && a.Name.Contains("new-build", StringComparison.InvariantCultureIgnoreCase));
                    custInstance.IsNewBuild = isNewBuild;

                    foreach (var customerInstanceApplication in customerInstanceApplications)
                    {
                        var application = applications.First(a => a.Id == customerInstanceApplication.ApplicationId);

                        if (!application.HasPublicEndpoint)
                        {
                            continue;
                        }

                        var app = application.ToApp();

                        custInstance.Applications.Add(app);

                        var result = await GetHealth(custInstance, httpClient, customerInstance.FullDomain, app, overallState, stoppingToken);
                        statuses.Add(result);
                        healthCheckCounter.Add(1,
                                               new KeyValuePair<string, object?>("CustomerInstanceName", customerInstance.Name),
                                               new KeyValuePair<string, object?>("AppName", app.Name),
                                               new KeyValuePair<string, object?>("Status", result.ToString()));
                    }

                    var ci = new CustomerInstanceState(custInstance, statuses);

                    overallState.Report(ci);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error executing HealthCheck request: {Domain} {Message}", customerInstance.FullDomain, ex.Message);
                }
            });

            logger.LogInformation($"{"Customer Name",25}    {"Status",15} {"RulesEngine",15} {"TLM",15} {"UserManagement",15} {"Command",15}");

            var customerStatuses = overallState.CustomerInstances.Where(ci => ci != null).Select(ci => $"{Environment.NewLine}{ci.CustomerInstance.Name,25}    {ci.Status,15}");

            logger.LogInformation("{CustomerStatuses}", customerStatuses);

            logger.LogInformation("Total applications: {Count}", overallState.ApplicationInstances.Count());

            logger.LogInformation("Total applications scanned: {Count}", overallState.ApplicationInstances.Count());

            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }

    }

    private async Task<HealthStatus> GetHealth(
        CustomerInstance customerInstance,
        HttpClient httpClient,
        string domain,
        ApplicationSpecification app,
        OverallState overallState,
        CancellationToken stoppingToken)
    {
        string uri =
            app.HealthEndpointPath.StartsWith("https://") ? app.HealthEndpointPath :
             $"https://{domain}/{app.HealthEndpointPath.TrimStart('/')}";
        string accessUri =
             app.Path.StartsWith("https://") ? app.Path :  // direct reference
             $"https://{domain}/{app.Path.TrimStart('/')}"; // relative to single tenant
        string region = customerInstance.Region;
        string shortCode = customerInstance.CustomerInstanceCode;
        string devOrPrd = customerInstance.IsDevelopment ? "dev" : "prd";
        string cloudRoleName = app.RoleName;
        var deploymentPhase = customerInstance.DeploymentPhase;

        logger.LogInformation("Fetch {Uri}", uri);

        string applicationInsightsLink = customerInstance.LogUrlForApp($"cloud_RoleName startswith \"{app.RoleName}\"");
        string exceptionsLink = customerInstance.ExceptionUrlForApp($"cloud_RoleName startswith \"{app.RoleName}\"");

        try
        {
            var dependency = await httpClient.GetAsync(uri, stoppingToken);

            if (dependency.IsSuccessStatusCode)
            {
                try
                {
                    HealthCheckDto healthDto;
                    var json = await dependency.Content.ReadAsStringAsync(stoppingToken);

                    dynamic jsonObject = JObject.Parse(json);
                    string status = jsonObject.status;
                    var isBasicHealthCheck = string.Equals(status, "OK", StringComparison.InvariantCultureIgnoreCase);
                    if (isBasicHealthCheck)
                    {
                        healthDto = new HealthCheckDto() {
                            Key = app.ServiceName ?? app.Name,
                            Status = HealthStatus.Healthy,
                            Description = app.RoleName,
                            Version = "0.1.0.0"
                        };
                    }
                    else
                    {
                        healthDto = JsonConvert.DeserializeObject<HealthCheckDto>(json)!;
                    }

                    if (healthDto is null)
                    {
                        return HealthStatus.Unhealthy;
                    }
                    else
                    {
                        logger.LogInformation("HealthDto Info: {Domain} {HealthEndpointPath} {Status} {Description} {Version}", domain, app.HealthEndpointPath, healthDto.Status, healthDto.Description, healthDto.Version);

                        overallState.Report(new ApplicationInstance(true, region, devOrPrd, shortCode, app.Name, deploymentPhase, domain, accessUri, uri, cloudRoleName, applicationInsightsLink, exceptionsLink, healthDto, DateTimeOffset.Now));

                        var data2 = healthDto.EntriesWithPayload?.ToDictionary(x => x.Key, x => x.Value as HealthCheckDto) ?? new();
                        foreach ((string key, var health) in data2)
                        {
                            if (health is null) continue;
                            if (key == "Assembly Version") continue;

                            string mergedKey = health.Key ?? key;

                            overallState.Report(new ApplicationInstance(false, region, devOrPrd, shortCode, ApplicationName: mergedKey, deploymentPhase, domain, "", "", CloudRoleName: mergedKey, applicationInsightsLink, exceptionsLink, health, DateTimeOffset.Now));
                            logger.LogInformation("Saved [Success] Health Report for -> AppName: {MergedKey:-7}, {Description}", mergedKey, health.Description);
                        }
                    }
                    return healthDto.Status;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to parse JSON {Message}", ex.Message);
                    return HealthStatus.Unhealthy;
                }
            }
            else if (dependency.StatusCode == System.Net.HttpStatusCode.InternalServerError)
            {
                var json = await dependency.Content.ReadAsStringAsync(stoppingToken);
                var healthDto = JsonConvert.DeserializeObject<HealthCheckDto>(json);
                if (healthDto is not null)
                {
                    overallState.Report(new ApplicationInstance(true, region, devOrPrd, shortCode, app.Name, deploymentPhase, domain, accessUri, uri, cloudRoleName, applicationInsightsLink, exceptionsLink, healthDto, DateTimeOffset.Now));

                    logger.LogWarning("InternalServerError on Health Request for: {Domain} {HealthEndpointPath} {Status} {Description}", domain, app.HealthEndpointPath, healthDto.Status, healthDto.Description);
                }
                logger.LogWarning("InternalServerError on GetHealth: {Uri}", uri);
            }
            else if (dependency.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                var json = await dependency.Content.ReadAsStringAsync(stoppingToken);

                if (json.Contains("no healthy upstream"))
                {
                    var healthReport3 = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Unhealthy, TimeSpan.FromSeconds(2));
                    var healthResult3 = new HealthCheckDto(app.Name, "No healthy upstream", healthReport3) { Version = "?" };
                    overallState.Report(new ApplicationInstance(true, region, devOrPrd, shortCode, app.Name, deploymentPhase, domain, accessUri, uri, cloudRoleName, applicationInsightsLink, exceptionsLink, healthResult3, DateTimeOffset.Now));
                    logger.LogWarning("(No healthy upstream) - Uri: {Uri}", uri);
                    return HealthStatus.Degraded;
                }
                else
                {
                    try
                    {
                        var healthDto = JsonConvert.DeserializeObject<HealthCheckDto>(json);
                        if (healthDto is not null)
                        {
                            overallState.Report(new ApplicationInstance(true, region, devOrPrd, shortCode, app.Name, deploymentPhase, domain, accessUri, uri, cloudRoleName, applicationInsightsLink, exceptionsLink, healthDto, DateTimeOffset.Now));
                            logger.LogWarning("(Service Unavailable) - {Uri} {Status} {Description} {Version}", uri, healthDto.Status, healthDto.Description, healthDto.Version);
                            return HealthStatus.Degraded;
                        }
                    }
                    catch (Exception ex)
                    {
                        // In the future, need to handle optional apps better
                        HealthStatus failing2 = HealthStatus.Unhealthy;
                        var healthReport2 = new HealthReport(new Dictionary<string, HealthReportEntry>(), failing2, TimeSpan.FromSeconds(2));
                        var healthResult2 = new HealthCheckDto(app.Name, "No healthy upstream", healthReport2) { Version = "?" };
                        overallState.Report(new ApplicationInstance(true, region, devOrPrd, shortCode, app.Name, deploymentPhase, domain, accessUri, uri, cloudRoleName, applicationInsightsLink, exceptionsLink, healthResult2, DateTimeOffset.Now));
                        logger.LogError(ex, "(No healthy upstream) - {Domain} {HealthEndpointPath} ", domain, app.HealthEndpointPath);
                        return HealthStatus.Degraded;
                    }
                }
            }
            else
            {
                logger.LogWarning("{Domain} {HealthEndpointPath} service unavailable and no content {StatusCode}", domain, app.HealthEndpointPath, dependency.StatusCode);
            }
        }
        catch (Exception ex) when (ex.Message.Contains("Unexpected character encountered while parsing value: <"))
        {
            logger.LogError(ex, "{Domain} {Uri} Received HTML not JSON", domain, uri);
        }
        catch (TaskCanceledException)
        {
            // ignore, timeout
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Domain} {Uri} {Message} (UNSPECIFIED)", domain, uri, ex.Message);
        }

        HealthStatus failing = HealthStatus.Unhealthy;

        var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), failing, TimeSpan.FromSeconds(2));
        var healthResult = new HealthCheckDto(app.Name, "Failed to contact", healthReport) { Version = "?" };
        overallState.Report(new ApplicationInstance(true, region, devOrPrd, shortCode, app.Name, deploymentPhase, domain, accessUri, uri, cloudRoleName, applicationInsightsLink, exceptionsLink, healthResult, DateTimeOffset.Now));
        return failing;
    }
}
