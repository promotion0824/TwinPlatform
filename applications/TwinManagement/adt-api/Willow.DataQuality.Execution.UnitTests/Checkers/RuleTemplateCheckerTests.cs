namespace Willow.DataQuality.Execution.UnitTests.Checkers
{
    using Azure.DigitalTwins.Core;
    using DTDLParser.Models;
    using Moq;
    using Willow.AzureDigitalTwins.Services.Interfaces;
    using Willow.DataQuality.Execution.Checkers;
    using Willow.DataQuality.Model.Rules;
    using Willow.DataQuality.Model.Validation;
    using Willow.Model.Adt;

    public class RuleTemplateCheckerTests
    {
        private readonly RuleTemplateChecker ruleTemplateChecker;
        private readonly Mock<IRuleBodyChecker<RuleTemplateProperty, PropertyValidationResult>> propertiesCheckerMock;
        private readonly Mock<IRuleBodyChecker<RuleTemplatePath, PathValidationResult>> pathsCheckerMock;
        private readonly Mock<IRuleBodyChecker<RuleTemplateExpression, ExpressionValidationResult>> expressionsCheckerMock;
        private readonly Mock<IAzureDigitalTwinModelParser> parserMock;

        public RuleTemplateCheckerTests()
        {
            propertiesCheckerMock = new Mock<IRuleBodyChecker<RuleTemplateProperty, PropertyValidationResult>>();
            pathsCheckerMock = new Mock<IRuleBodyChecker<RuleTemplatePath, PathValidationResult>>();
            expressionsCheckerMock = new Mock<IRuleBodyChecker<RuleTemplateExpression, ExpressionValidationResult>>();
            parserMock = new Mock<IAzureDigitalTwinModelParser>();
            ruleTemplateChecker = new RuleTemplateChecker(propertiesCheckerMock.Object, pathsCheckerMock.Object, expressionsCheckerMock.Object, parserMock.Object);
        }

        [Fact(Skip = "TODO Fix: de-serialize unrecognized type discriminator")]
        public async Task Check_WithExactModelMatchAndNonMatchingModel_ReturnsNonApplicableModel()
        {
            var rule = new RuleTemplate { ExactModelOnly = true, PrimaryModelId = "test-model" };
            var bdt = new BasicDigitalTwin { Metadata = new DigitalTwinMetadata { ModelId = "non-matching-model" } };
            var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

            propertiesCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplateProperty>>(), null))
                .ReturnsAsync(Enumerable.Empty<PropertyValidationResult>());
            pathsCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplatePath>>(), null))
                .ReturnsAsync(Enumerable.Empty<PathValidationResult>());
            expressionsCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplateExpression>>(), null))
                .ReturnsAsync(Enumerable.Empty<ExpressionValidationResult>());
            var result = await ruleTemplateChecker.Check(twinWithRelationship, rule);

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
                OutgoingRelationships = new List<BasicRelationship>() { },
            };

            parserMock.Setup(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>())).Returns(new Dictionary<string, DTInterfaceInfo>());

            var result = await ruleTemplateChecker.Check(twinWithRelationship, rule);

            Assert.NotNull(result);
            //Assert.True(result.IsValid);
            Assert.False(result.IsApplicableModel);
            Assert.True(!result.PropertyValidationResults.Any());
            Assert.True(!result.PathValidationResults.Any());
            Assert.True(!result.ExpressionValidationResults.Any());
            parserMock.Verify(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        [Fact]
        public async Task Check_WithValidTwinAndRule_ReturnsValidResult()
        {
            var rule = new RuleTemplate { ExactModelOnly = false, PrimaryModelId = "test-model" };
            var bdt = new BasicDigitalTwin { Metadata = new DigitalTwinMetadata { ModelId = "test-model" } };
            var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

            var result = await ruleTemplateChecker.Check(twinWithRelationship, rule);

            Assert.NotNull(result);
            Assert.True(result.IsValid);
            Assert.True(result.IsApplicableModel);
            Assert.True(!result.PropertyValidationResults.Any());
            Assert.True(!result.PathValidationResults.Any());
            Assert.True(!result.ExpressionValidationResults.Any());
            parserMock.Verify(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public async Task Check_WithValidTwinAndRule_FailedProperties_ReturnsInvalidResult()
        {
            var rule = new RuleTemplate { ExactModelOnly = false, PrimaryModelId = "test-model" };
            var bdt = new BasicDigitalTwin { Metadata = new DigitalTwinMetadata { ModelId = "test-model" } };
            var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

            var propertyName = "property-name";

            propertiesCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplateProperty>>(), null))
                .ReturnsAsync(new List<PropertyValidationResult> { new PropertyValidationResult(PropertyValidationResultType.RequiredPropertyMissing, propertyName) });

            var result = await ruleTemplateChecker.Check(twinWithRelationship, rule);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.IsApplicableModel);
            Assert.True(result.PropertyValidationResults.Any());
            Assert.True(!result.PathValidationResults.Any());
            Assert.True(!result.ExpressionValidationResults.Any());
            parserMock.Verify(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public async Task Check_WithValidTwinAndRule_FailedPaths_ReturnsInvalidResult()
        {
            var rule = new RuleTemplate { ExactModelOnly = false, PrimaryModelId = "test-model" };
            var bdt = new BasicDigitalTwin { Metadata = new DigitalTwinMetadata { ModelId = "test-model" } };
            var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

            pathsCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplatePath>>(), null))
                .ReturnsAsync(new List<PathValidationResult> { new PathValidationResult() });

            var result = await ruleTemplateChecker.Check(twinWithRelationship, rule);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.IsApplicableModel);
            Assert.True(result.PathValidationResults.Any());
            Assert.True(!result.PropertyValidationResults.Any());
            Assert.True(!result.ExpressionValidationResults.Any());
            parserMock.Verify(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public async Task Check_WithValidTwinAndRule_FailedExpressions_ReturnsInvalidResult()
        {
            var rule = new RuleTemplate { ExactModelOnly = false, PrimaryModelId = "test-model" };
            var bdt = new BasicDigitalTwin { Metadata = new DigitalTwinMetadata { ModelId = "test-model" } };
            var twinWithRelationship = new TwinWithRelationships() { Twin = bdt, IncomingRelationships = new List<BasicRelationship>() { }, OutgoingRelationships = new List<BasicRelationship>() };

            expressionsCheckerMock.Setup(x => x.Check(It.IsAny<TwinWithRelationships>(), It.IsAny<IEnumerable<RuleTemplateExpression>>(), null))
                .ReturnsAsync(new List<ExpressionValidationResult> { new ExpressionValidationResult() });

            var result = await ruleTemplateChecker.Check(twinWithRelationship, rule);

            Assert.NotNull(result);
            Assert.False(result.IsValid);
            Assert.True(result.IsApplicableModel);
            Assert.True(result.ExpressionValidationResults.Any());
            Assert.True(!result.PropertyValidationResults.Any());
            Assert.True(!result.PathValidationResults.Any());
            parserMock.Verify(x => x.GetInterfaceDescendants(It.IsAny<IEnumerable<string>>()), Times.Never);
        }
    }
}
