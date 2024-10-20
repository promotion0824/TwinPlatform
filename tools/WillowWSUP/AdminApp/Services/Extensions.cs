using Willow.Infrastructure.Entities;

namespace Willow.AdminApp.Services
{
    public static class Extensions
    {
        public static CustomerInstance ToCust(this Willow.Support.SDK.CustomerInstance customerInstance, Support.SDK.Stamp stamp)
        {
            var isDevelopment = stamp.EnvironmentName == "dev";
            if (!Enum.TryParse<DeploymentPhase>(customerInstance.DeploymentPhase, out var deploymentPhase))
            {
                deploymentPhase = DeploymentPhase.CI;
            }

            if (!Enum.TryParse<LifeCycleState>(customerInstance.LifecycleState, out var lifeCycleState))
            {
                lifeCycleState = LifeCycleState.Unknown;
            }

            var appSpecifications = new List<ApplicationSpecification>();

            foreach (var app in customerInstance.CustomerInstanceApplications)
            {
                appSpecifications.Add(app.Application.ToApp());
            }

            var cust = new CustomerInstance(isDevelopment,
                                            stamp.RegionShortName,
                                            customerInstance.ShortName,
                                            customerInstance.FullCustomerInstanceName,
                                            customerInstance.DisplayName,
                                            customerInstance.FullDomain,
                                            deploymentPhase,
                                            customerInstance.ResourceGroupName,
                                            stamp.SubscriptionId.ToString(),
                                            lifeCycleState,
                                            appSpecifications.ToArray());
            return cust;
        }

        public static ApplicationSpecification ToApp(this Willow.Support.SDK.Application application)
        {
            var appName = string.IsNullOrEmpty(application.DisplayName) ? application.Name : application.DisplayName;
            return new ApplicationSpecification(appName, application.Path, application.HealthEndpointPath, application.RoleName, application.Name);
        }
    }
}
