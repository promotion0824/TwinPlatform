using Azure.DigitalTwins.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.DataQuality.Execution.Checkers;
using Willow.DataQuality.Model.Rules;
using Willow.Model.Adt;
using Xunit;

namespace Willow.AzureDigitalTwins.Api.UnitTests.Checkers;

public class PropertiesCheckerTests
{
    private readonly PropertiesChecker _propertiesChecker;
    public PropertiesCheckerTests()
    {
        _propertiesChecker = new PropertiesChecker();
    }

    [Fact]
    public async Task Check_TwinWithRequiredProperty_DoesNotAddErrorToResult()
    {
        var propertyName = "test-property";
        var rules = new List<RuleTemplateProperty> { new RuleTemplateProperty { Name = propertyName, Required = true } };
        var bdt = new BasicDigitalTwin { Contents = new Dictionary<string, object> { { propertyName, true } } };
        var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>(), TwinData = new Dictionary<string, object>() };

        var result = await _propertiesChecker.Check(twinWithRelationship, rules);

        Assert.NotNull(result);
        Assert.True(!result.Any());
    }

    [Fact]
    public async Task Check_TwinWithMissingRequiredProperty_ShouldAddErrorToResult()
    {
        var propertyName = "test-property";
        var rules = new List<RuleTemplateProperty> { new RuleTemplateProperty { Name = propertyName, Required = true } };
        var bdt = new BasicDigitalTwin { Contents = new Dictionary<string, object> { { "non-property", true } } };
        var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

        var result = await _propertiesChecker.Check(twinWithRelationship, rules);

        Assert.NotNull(result);
        Assert.True(result.Any());
    }
}
