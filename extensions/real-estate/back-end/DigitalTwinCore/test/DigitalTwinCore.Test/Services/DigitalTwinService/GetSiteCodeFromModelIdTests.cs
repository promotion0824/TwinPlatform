using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Services.DigitalTwinService
{
    public class GetSiteCodeFromModelIdTests : BaseInMemoryTest
    {
        public GetSiteCodeFromModelIdTests(ITestOutputHelper output) : base(output)
        {
        }

        // Note: Support for site-specific models has been disabled due to 
        //    lack of need and testing, and also makes code-paths faster 
        [Fact]
        public void InterfaceModelIds_ReturnsCorrectSiteCode()
        {
            DigitalTwinCore.Services.DigitalTwinService.GetSiteCodeFromModelId("dtmi:com:willowinc:Company;1").Should().BeNull();
            //DigitalTwinCore.Services.DigitalTwinService.GetSiteCodeFromModelId("dtmi:com:willowinc:B121:Company;1").Should().Be("B121");
        }

       [Fact]
        public void NonWillowModelIds_ReturnsCorrectSiteCode()
        {
            DigitalTwinCore.Services.DigitalTwinService.GetSiteCodeFromModelId("dtmi:org:w3id:rec:agents:Company;1").Should().BeNull();
            //DigitalTwinCore.Services.DigitalTwinService.GetSiteCodeFromModelId("dtmi:com:facebook:User;1").Should().BeNull();
        }
    }
}
