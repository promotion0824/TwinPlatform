using Azure.DigitalTwins.Core;
using DTDLParser.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.DataQuality.Execution.Checkers;
using Willow.DataQuality.Model.Rules;
using Willow.DataQuality.Model.Validation;
using Willow.Model.Adt;
using Xunit;

namespace Willow.AzureDigitalTwins.Api.UnitTests.Checkers;

public class RuleTemplateCheckerTests
{
    private readonly RuleTemplateChecker _ruleTemplateChecker;
    private readonly Mock<IRuleBodyChecker<RuleTemplateProperty, PropertyValidationResult>> _propertiesCheckerMock;
    private readonly Mock<IRuleBodyChecker<RuleTemplatePath, PathValidationResult>> _pathsCheckerMock;
    private readonly Mock<IRuleBodyChecker<RuleTemplateExpression, ExpressionValidationResult>> _expressionsCheckerMock;
    private readonly Mock<IAzureDigitalTwinModelParser> _parserMock;
    public RuleTemplateCheckerTests()
    {
        _propertiesCheckerMock = new Mock<IRuleBodyChecker<RuleTemplateProperty, PropertyValidationResult>>();
        _pathsCheckerMock = new Mock<IRuleBodyChecker<RuleTemplatePath, PathValidationResult>>();
        _expressionsCheckerMock = new Mock<IRuleBodyChecker<RuleTemplateExpression, ExpressionValidationResult>>();
        _parserMock = new Mock<IAzureDigitalTwinModelParser>();
        _ruleTemplateChecker = new RuleTemplateChecker(_propertiesCheckerMock.Object, _pathsCheckerMock.Object, _expressionsCheckerMock.Object, _parserMock.Object);
    }

    [Fact(Skip = "TODO Fix: de-serialize unrecognized type discriminator")]
    public async Task Check_WithExactModelMatchAndNonMatchingModel_ReturnsNonApplicableModel()
    {
        var rule = new RuleTemplate { ExactModelOnly = true, PrimaryModelId = "test-model" };
        var bdt = new BasicDigitalTwin { Metadata = new DigitalTwinMetadata { ModelId = "non-matching-model" } };
        var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };


        _propertiesCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplateProperty>>(), null))
            .ReturnsAsync(Enumerable.Empty<PropertyValidationResult>());
        _pathsCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplatePath>>(), null))
            .ReturnsAsync(Enumerable.Empty<PathValidationResult>());
        _expressionsCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplateExpression>>(), null))
            .ReturnsAsync(Enumerable.Empty<ExpressionValidationResult>());
        var result = await _ruleTemplateChecker.Check(twinWithRelationship, rule);

        Assert.NotNull(result);
        //Assert.True(result.IsValid);
        Assert.False(result.IsApplicableModel);
        Assert.True(!result.PropertyValidationResults.Any());
        Assert.True(!result.PathValidationResults.Any());
        Assert.True(!result.ExpressionValidationResults.Any());
    }

    [Fact]
    public async Task Check_WithoutExactModelMatchAndNonMatchingModel_ReturnsNonApplicableModel()
    {
        var rule = new RuleTemplate { ExactModelOnly = false, PrimaryModelId = "test-model" };
        var bdt = new BasicDigitalTwin { Metadata = new DigitalTwinMetadata { ModelId = "non-matching-model" } };
        var twinWithRelationship = new TwinWithRelationships()
        {
            Twin = bdt,
            IncomingRelationships = new List<BasicRelationship>() { },
            OutgoingRelationships = new List<BasicRelationship>() { }
        };


        _parserMock.Setup(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>())).Returns(new Dictionary<string, DTInterfaceInfo>());

        var result = await _ruleTemplateChecker.Check(twinWithRelationship, rule);

        Assert.NotNull(result);
        //Assert.True(result.IsValid);
        Assert.False(result.IsApplicableModel);
        Assert.True(!result.PropertyValidationResults.Any());
        Assert.True(!result.PathValidationResults.Any());
        Assert.True(!result.ExpressionValidationResults.Any());
        _parserMock.Verify(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public async Task Check_WithValidTwinAndRule_ReturnsValidResult()
    {
        var rule = new RuleTemplate { ExactModelOnly = false, PrimaryModelId = "test-model" };
        var bdt = new BasicDigitalTwin { Metadata = new DigitalTwinMetadata { ModelId = "test-model" } };
        var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

        var result = await _ruleTemplateChecker.Check(twinWithRelationship, rule);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.True(result.IsApplicableModel);
        Assert.True(!result.PropertyValidationResults.Any());
        Assert.True(!result.PathValidationResults.Any());
        Assert.True(!result.ExpressionValidationResults.Any());
        _parserMock.Verify(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task Check_WithValidTwinAndRule_FailedProperties_ReturnsInvalidResult()
    {
        var rule = new RuleTemplate { ExactModelOnly = false, PrimaryModelId = "test-model" };
        var bdt = new BasicDigitalTwin { Metadata = new DigitalTwinMetadata { ModelId = "test-model" } };
        var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

        var propertyName = "property-name";

        _propertiesCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplateProperty>>(), null))
            .ReturnsAsync(new List<PropertyValidationResult> { new PropertyValidationResult(PropertyValidationResultType.RequiredPropertyMissing, propertyName) });

        var result = await _ruleTemplateChecker.Check(twinWithRelationship, rule);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.True(result.IsApplicableModel);
        Assert.True(result.PropertyValidationResults.Any());
        Assert.True(!result.PathValidationResults.Any());
        Assert.True(!result.ExpressionValidationResults.Any());
        _parserMock.Verify(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task Check_WithValidTwinAndRule_FailedPaths_ReturnsInvalidResult()
    {
        var rule = new RuleTemplate { ExactModelOnly = false, PrimaryModelId = "test-model" };
        var bdt = new BasicDigitalTwin { Metadata = new DigitalTwinMetadata { ModelId = "test-model" } };
        var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

        _pathsCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplatePath>>(), null))
                .ReturnsAsync(new List<PathValidationResult> { new PathValidationResult() });

        var result = await _ruleTemplateChecker.Check(twinWithRelationship, rule);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.True(result.IsApplicableModel);
        Assert.True(result.PathValidationResults.Any());
        Assert.True(!result.PropertyValidationResults.Any());
        Assert.True(!result.ExpressionValidationResults.Any());
        _parserMock.Verify(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task Check_WithValidTwinAndRule_FailedExpressions_ReturnsInvalidResult()
    {
        var rule = new RuleTemplate { ExactModelOnly = false, PrimaryModelId = "test-model" };
        var bdt = new BasicDigitalTwin { Metadata = new DigitalTwinMetadata { ModelId = "test-model" } };
        var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

        _expressionsCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplateExpression>>(), null))
                .ReturnsAsync(new List<ExpressionValidationResult> { new ExpressionValidationResult() });

        var result = await _ruleTemplateChecker.Check(twinWithRelationship, rule);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
        Assert.True(result.IsApplicableModel);
        Assert.True(result.ExpressionValidationResults.Any());
        Assert.True(!result.PropertyValidationResults.Any());
        Assert.True(!result.PathValidationResults.Any());
        _parserMock.Verify(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>()), Times.Never);
    }
}
