using AutoFixture.Xunit2;
using DigitalTwinCore.Infrastructure.Extensions;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Test.Infrastructure;
using DTDLParser;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DigitalTwinCore.Test.Extensions
{
    public class DigitalTwinServiceProviderExtensionsTests
    {
        private List<Model> _models;
        private DigitalTwinModelParser _modelParser;
        private Mock<IDigitalTwinServiceProvider> _digitalTwinServiceProvider;
        private Mock<IDigitalTwinService> _digitalTwinService;

        private async Task Setup(string modelFileName, SiteAdtSettings[] siteSettings)
        {
            _models = FileHelper.LoadFile<List<Model>>(modelFileName);
            _modelParser = await DigitalTwinModelParser.CreateAsync(_models, new Mock<ILogger<IDigitalTwinService>>().Object);
            _digitalTwinServiceProvider = new Mock<IDigitalTwinServiceProvider>();
            _digitalTwinService = new Mock<IDigitalTwinService>();

            foreach (var t in siteSettings)
            {
                t.AssetModelIds = _models.Take(2).Select(x => x.Id).ToArray();
                t.BuildingComponentModelIds = _models.Skip(2).Take(2).Select(x => x.Id).ToArray();
                t.SpaceModelIds = _models.Skip(4).Take(2).Select(x => x.Id).ToArray();
                t.StructureModelIds = _models.Skip(6).Take(2).Select(x => x.Id).ToArray();
                t.ComponentModelIds = _models.Skip(8).Take(2).Select(x => x.Id).ToArray();
                t.CollectionModelIds = _models.Skip(10).Take(2).Select(x => x.Id).ToArray();
            }

            var setupSequence = _digitalTwinService.SetupSequence(x => x.SiteAdtSettings);
            foreach (var siteSetting in siteSettings)
            {
                setupSequence.Returns(siteSetting);
            }

            _digitalTwinService.Setup(x => x.GetModelParserAsync()).ReturnsAsync(_modelParser);
            _digitalTwinServiceProvider.Setup(x => x.GetForSiteAsync(It.IsAny<Guid>())).ReturnsAsync(_digitalTwinService.Object);
        }

        [Theory(Skip = "Testing v3")]
        [InlineAutoData("wil-prd-lda-msft-eu22-adt-models.json")]
        public async Task Should_Return_Default_Models(string modelFileName, SiteAdtSettings[] siteSettings)
        {
            await Setup(modelFileName, siteSettings);

            var modelIds = await _digitalTwinServiceProvider.Object.GetModelIds(siteSettings.Select(x => x.SiteId)).ToArrayAsync();

            foreach (var pair in modelIds)
            {
                pair.Value.Should().HaveCount(215);
            }
        }

        [Theory(Skip = "Testing v3")]
        [InlineAutoData("wil-prd-lda-msft-eu22-adt-models.json")]
        public async Task Should_Return_Category_Models(string modelFileName, SiteAdtSettings[] siteSettings)
        {
            await Setup(modelFileName, siteSettings);
            
            var furniture = _modelParser.GetInterface("dtmi:com:willowinc:FurnitureCollection;1").GetUniqueId();
            var electricalCircuit = _modelParser.GetInterface("dtmi:com:willowinc:ElectricalCircuit;1").GetUniqueId();

            var modelIds = await _digitalTwinServiceProvider.Object.GetModelIds(siteSettings.Select(x => x.SiteId), new[] { furniture, electricalCircuit }, string.Empty).ToArrayAsync();

            foreach (var pair in modelIds)
            {
                pair.Value.Should().HaveCount(5);
            }
        }

        [Theory(Skip = "Testing v3")]
        [InlineAutoData("wil-prd-lda-msft-eu22-adt-models.json")]
        public async Task Should_Return_Type_Models(string modelFileName, SiteAdtSettings[] siteSettings)
        {
            await Setup(modelFileName, siteSettings);

            var modelIds = await _digitalTwinServiceProvider.Object.GetModelIds(siteSettings.Select(x => x.SiteId), Array.Empty<Guid>(), "dtmi:com:willowinc:ElectricalCircuit;1").ToArrayAsync();

            foreach (var pair in modelIds)
            {
                pair.Value.Should().HaveCount(4);
            }
        }
    }
}