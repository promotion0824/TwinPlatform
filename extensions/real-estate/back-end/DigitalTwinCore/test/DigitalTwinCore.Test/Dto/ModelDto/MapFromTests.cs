using AutoFixture;
using DigitalTwinCore.Models;
using FluentAssertions;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace DigitalTwinCore.Test.Dto.ModelDto
{
    public class MapFromTests : BaseInMemoryTest
    {
        public MapFromTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void ModelContainsSiteCode_ReturnsIsShared_False()
        {
            var dto = DigitalTwinCore.Dto.ModelDto.MapFrom("B121", Fixture.Build<Model>().With(x => x.Id, "dtmi:com:willowinc:B121:Company;1").Create());
            dto.IsShared.Should().BeFalse();
            
            dto = DigitalTwinCore.Dto.ModelDto.MapFrom("B121", Fixture.Build<Model>().With(x => x.Id, "dtmi:com:willowinc:interface:B121:Company;1").Create());
            dto.IsShared.Should().BeFalse();
        }

        [Fact]
        public void ModelContainsNoSiteCode_ReturnsIsShared_True()
        {
            var dto = DigitalTwinCore.Dto.ModelDto.MapFrom("B121", Fixture.Build<Model>().With(x => x.Id, "dtmi:com:willowinc:Company;1").Create());
            dto.IsShared.Should().BeTrue();

            dto = DigitalTwinCore.Dto.ModelDto.MapFrom("B121", Fixture.Build<Model>().With(x => x.Id, "dtmi:com:willowinc:interface:Company;1").Create());
            dto.IsShared.Should().BeTrue();

            dto = DigitalTwinCore.Dto.ModelDto.MapFrom("B121", Fixture.Build<Model>().With(x => x.Id, "dtmi:com:willowinc:abstract:Company;1").Create());
            dto.IsShared.Should().BeTrue();

            dto = DigitalTwinCore.Dto.ModelDto.MapFrom("B121", Fixture.Build<Model>().With(x => x.Id, "dtmi:org:w3id:rec:agents:Company;1").Create());
            dto.IsShared.Should().BeTrue();
        }
    }
}
