using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Willow.AppContext;
using Willow.TwinLifecycleManagement.Web.Controllers;
using Willow.TwinLifecycleManagement.Web.Models;
using Willow.TwinLifecycleManagement.Web.Options;
using Willow.TwinLifecycleManagement.Web.Services;
using Xunit;

namespace Willow.TwinLifecycleManagement.Web.UnitTests.Controllers
{
    public class ConfigControllerTests
    {
        private ConfigController _sut;

        public ConfigControllerTests()
        {
            var applicationInsightsOptions = new ApplicationInsightsDto();
            var azureAppOptions = new AzureAppOptions();
            var willowContextOptions = new WillowContextOptions();
            var mtiOptions = new MtiOptions();
            var envService = new Mock<IEnvService>();
            _sut = new ConfigController(
                new OptionsWrapper<ApplicationInsightsDto>(applicationInsightsOptions),
                new OptionsWrapper<AzureAppOptions>(azureAppOptions),
                new OptionsWrapper<WillowContextOptions>(willowContextOptions),
                new OptionsWrapper<MtiOptions>(mtiOptions),
                envService.Object);
        }

        [Fact]
        public void AnonymousGet_ExposesPublicDataOnly()
        {
            var result = _sut.GetConfig();
            var responseData = (result as OkObjectResult)?.Value;

            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(responseData);

            // Response should contain only the expected types
            var responseProperties = responseData.GetType().GetProperties();

            Assert.Collection(responseProperties,
                p => Assert.Equal("Willow.Api.Logging.ApplicationInsights.ApplicationInsightsOptions", p.PropertyType.FullName),
                p => Assert.Equal("Willow.TwinLifecycleManagement.Web.Options.AzureAppOptions", p.PropertyType.FullName),
                p => Assert.Equal("System.String", p.PropertyType.FullName),
                p => Assert.Equal("Willow.AppContext.WillowContextOptions", p.PropertyType.FullName));

            // Response should contain no fields
            var responseFields = responseData.GetType().GetFields();

            Assert.Empty(responseFields);

            // Each type in the response should contain only the expected properties
            var applicationInsightsOptionsProperties = responseProperties[0].PropertyType.GetProperties();
            var azureAppOptionsProperties = responseProperties[1].PropertyType.GetProperties();
            var tlmMetadataOptionsProperties = responseProperties[3].PropertyType.GetProperties();

            Assert.Collection(azureAppOptionsProperties,
                p => Assert.Equal("ClientId", p.Name),
                p => Assert.Equal("BaseUrl", p.Name),
                p => Assert.Equal("BackendB2CScopes", p.Name),
                p => Assert.Equal("FrontendB2CScopes", p.Name),
                p => Assert.Equal("KnownAuthorities", p.Name),
                p => Assert.Equal("Authority", p.Name));
            Assert.Equal(8,tlmMetadataOptionsProperties.Count());

            // Each type in the response should contain only the expected fields
            var applicationInsightsOptionsFields = responseProperties[0].PropertyType.GetFields();
            var azureAppOptionsFields = responseProperties[1].PropertyType.GetFields();

            Assert.Collection(azureAppOptionsFields,
                f => Assert.Equal("Config", f.Name));
        }
    }
}
