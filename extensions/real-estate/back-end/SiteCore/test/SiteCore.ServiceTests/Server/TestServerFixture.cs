using Alba;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;

namespace SiteCore.ServiceTests.Server
{
    public class TestServerFixture : IDisposable
    {
        public IAlbaHost albaHost;
        private readonly int _port;
        public TestcontainersContainer dbTestContainer;

        private readonly TestcontainerDatabaseConfiguration configuration = new MsSqlTestcontainerConfiguration("mcr.microsoft.com/mssql/server:2019-latest")
        {
            Password = "Password01!",
            Port = 1433
        };

        public TestServerFixture(int port)
        {
            _port = port;

            // Check if the container is already running. Else spin it up.
            if (!IsTestDbContainerRunning().Result)
            {
                dbTestContainer = InitialiseDbTestContainer();
                Task dbRunTask = dbTestContainer.StartAsync();
                Task.WaitAll(dbRunTask);
            }

            // Alba host
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(c => c.UseStartup<Startup>())
                .UseEnvironment("servicetests")
                .ConfigureAppConfiguration(configbuilder =>
                {
                    configbuilder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["ConnectionStrings:Sitedb"] =
                            $"Server=localhost,{_port};Database=SiteCoreDB;MultipleActiveResultSets=true;user id=sa;password=Password01!;TrustServerCertificate=True"
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IAuthenticationSchemeProvider, TestSchemeProvider>();
                });

            albaHost = new AlbaHost(builder);
        }

        private async Task<bool> IsTestDbContainerRunning()
        {
            var isRunning = false;

            DockerClient dockerClient = new DockerClientConfiguration()
                .CreateClient();

            IList<ContainerListResponse> containers = await dockerClient.Containers.ListContainersAsync(
                new ContainersListParameters()
                {
                    All = true
                });

            foreach (var container in containers)
            {
                if (container.Names[0].Contains(String.Concat("/siteCoreDb", _port.ToString())))
                {
                    // Check the state
                    if (container.State == "running")
                    {
                        isRunning = true;
                    }
                }
            }

            return isRunning;
        }

        private TestcontainersContainer InitialiseDbTestContainer()
        {
            var testContainersBuilder = new TestcontainersBuilder<MsSqlTestcontainer>()
                .WithName(String.Concat("siteCoreDb", _port.ToString()))
                .WithDatabase(this.configuration)
                .WithExposedPort(_port)
                .WithPortBinding(_port, 1433);

            MsSqlTestcontainer dbTestContainer = testContainersBuilder.Build();
            return dbTestContainer;
        }

        public void Dispose()
        {
            albaHost.Dispose();
            dbTestContainer.DisposeAsync();
        }
    }
}
