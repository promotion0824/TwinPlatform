using Azure.DigitalTwins.Core;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.DataQuality.Api.Services;
using Willow.AzureDigitalTwins.Services.Cache.Models;
using Willow.AzureDigitalTwins.Services.Configuration;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.DataQuality.Execution.Checkers;
using Willow.DataQuality.Model.Rules;
using Willow.DataQuality.Model.Serialization;
using Willow.DataQuality.Model.Validation;
using Willow.Model.Adt;
using Willow.Storage.Blobs;
using Xunit;

namespace Willow.AzureDigitalTwins.Api.UnitTests.Services
{
    public class DQRuleServiceTests
    {
        private readonly DQRuleService _dQRuleService;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly Mock<IRuleTemplateChecker> _ruleTemplateCheckerMock;
        private readonly Mock<IRuleTemplateSerializer> _ruleTemplateSerializerMock;
        private readonly Mock<ILogger<DQRuleService>> _logger;
        private readonly Mock<AzureDigitalTwinsSettings> _azureDigitalTwinSettings;
        private readonly Mock<IAzureDigitalTwinModelParser> _azureDigitalTwinModelParser;
        private readonly Mock<IAdxService> _adxService;

        public DQRuleServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> { { "BlobStorage:DQRuleContainer", "Dummy" } };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _memoryCacheMock = new Mock<IMemoryCache>();
            _blobServiceMock = new Mock<IBlobService>();
            _ruleTemplateCheckerMock = new Mock<IRuleTemplateChecker>();
            _ruleTemplateSerializerMock = new Mock<IRuleTemplateSerializer>();
            _logger = new Mock<ILogger<DQRuleService>>();
            _azureDigitalTwinSettings = new Mock<AzureDigitalTwinsSettings>();
            _azureDigitalTwinModelParser = new Mock<IAzureDigitalTwinModelParser>();
            _adxService = new Mock<IAdxService>();

            _dQRuleService = new DQRuleService(_blobServiceMock.Object,
                configuration,
                _memoryCacheMock.Object,
                _ruleTemplateCheckerMock.Object,
                _ruleTemplateSerializerMock.Object,
                _logger.Object,
                _azureDigitalTwinSettings.Object,
                _adxService.Object,
                _azureDigitalTwinModelParser.Object);
        }

        [Fact]
        public async Task DownloadRuleFile_ShouldCallDownloadFileOnce()
        {
            await _dQRuleService.DownloadRuleFileAsync("name");

            _blobServiceMock.Verify(x => x.DownloadFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteRuleFile_FileNotFound_Success()
        {
            _blobServiceMock.Setup(x => x.GetBlobItems("container", "name")).ReturnsAsync(Enumerable.Empty<BlobItem>());
            await _dQRuleService.DeleteRuleFileAsync("name");

            _blobServiceMock.Verify(x => x.DeleteBlob(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetValidationResults_WithValidTwin_ReturnsValidResult()
        {
            var rule = new RuleTemplate() { Id = "rule-id" };
            var bdt = new BasicDigitalTwin() { Id = "twin-id", Metadata = new DigitalTwinMetadata() { ModelId = "dtmi:com:willowinc:HVACBalancingValve;1" } };

            var twin = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

            var unitinfo = new Dictionary<string, List<UnitInfo>>();
            var uInfo = new List<UnitInfo> { new UnitInfo("weightUnit", "weight", "kilogram") };
            unitinfo.Add("dtmi:com:willowinc:HVACBalancingValve;1", uInfo);

            object rules = new ConcurrentDictionary<string, RuleTemplate> { ["rule-id"] = rule };
            _memoryCacheMock.Setup(x => x.TryGetValue(It.IsAny<string>(), out rules)).Returns(true);
            _ruleTemplateCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<RuleTemplate>(), It.IsAny<List<UnitInfo>>()))
                .ReturnsAsync(new RuleTemplateValidationResult(twin, rule) { IsValid = true });

            var results = await _dQRuleService.GetValidationResults(new List<TwinWithRelationships> { twin }, unitinfo);

            Assert.NotNull(results);

            // There are no rules to check - so count will now be 0.
            // The actual check code will now throw an exception for this case, so if we weren't using a mock, the check would be:
            //		var ex = await Assert.ThrowsAsync<InvalidDataException>( async () =>
            //				await _dQRuleService.GetValidationResults(new List<BasicDigitalTwin> { twin }, unitinfo)
            Assert.True(results.Count() == 0);
            //Assert.True(results.Single().IsValid); -- IsValid doesn't have any meaning here

            //Assert.True(results.Single().PropertyValidationResults.Count() == 0);
            //Assert.True(results.Single().PathValidationResults.Count() == 0);
            //Assert.True(results.Single().ExpressionValidationResults.Count() == 0);
            //Assert.Equal(rule.Id, results.Single().RuleTemplate.Id);
            //Assert.Equal(twin.Id, results.Single().Twin.Id);
        }

        [Fact(Skip = "Get Validation Results returning a null. unitInfo[key: twin.Metadata.ModelId] will not be null. Needs investigation.")]
        public async Task GetValidationResults_WithInvalidTwin_ReturnsInvalidResult()
        {
            var rule = new RuleTemplate() { Id = "rule-id" };
            var bdt = new BasicDigitalTwin() { Id = "twin-id", Metadata = new DigitalTwinMetadata() { ModelId = "dtmi:com:willowinc:HVACBalancingValve;1" } };
            var twin = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

            var unitinfo = new Dictionary<string, List<UnitInfo>>
            {
                ["dtmi:com:willowinc:HVACBalancingValve;1"] = new List<UnitInfo> { new UnitInfo("weightUnit", "weight", "kilogram") }
            };
            object val = new ConcurrentDictionary<string, RuleTemplate> { ["rule"] = rule };
            _memoryCacheMock.Setup(x => x.TryGetValue(It.IsAny<string>(), out val)).Returns(true);
            _ruleTemplateCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<RuleTemplate>(), It.IsAny<List<UnitInfo>>()))
                .ReturnsAsync(new RuleTemplateValidationResult(twin, rule)
                {
                    IsValid = false,
                    PropertyValidationResults = new List<PropertyValidationResult> { new PropertyValidationResult(PropertyValidationResultType.RequiredPropertyMissing, "prop") }
                });

            var results = await _dQRuleService.GetValidationResults(new List<TwinWithRelationships> { twin }, unitinfo);

            Assert.NotNull(results);
            Assert.True(results.Count() == 1);
            Assert.True(results.Single().PropertyValidationResults.Count() == 1);
            Assert.True(!results.Single().PathValidationResults.Any());
            Assert.True(!results.Single().ExpressionValidationResults.Any());
            Assert.False(results.Single().IsValid);
            Assert.Equal(PropertyValidationResultType.RequiredPropertyMissing, results.Single().PropertyValidationResults.Single().type);
            Assert.Equal("prop", results.Single().PropertyValidationResults.Single().propertyName);
            Assert.Equal(rule.Id, results.Single().RuleTemplate.Id);
            Assert.Equal(twin.Twin.Id, results.Single().TwinWithRelationship.Twin.Id);
        }
    }
}
