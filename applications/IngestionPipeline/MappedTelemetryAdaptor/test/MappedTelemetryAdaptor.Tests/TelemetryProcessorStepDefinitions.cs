namespace Willow.MappedTelemetryAdaptor.Tests;

using System.Text.Json;
using NSubstitute;
using Willow.MappedTelemetryAdaptor.Models;
using Willow.MappedTelemetryAdaptor.Services;
using Xunit;

[Binding]
public sealed class TelemetryProcessorStepDefinitions
{
    private MappedInput mappedInput = null!;
    private LiveData.Pipeline.Telemetry? result;

    [Given(@"I have Mapped Telemetry")]
    public void GivenIHaveMappedTelemetry()
    {
        this.mappedInput = new MappedInput
        {
            PointId = "PNTTestPointId01",
            Timestamp = DateTime.UtcNow,
            Value = 20.5d,
        };
    }

    [Given(@"the Value is scalar")]
    public void GivenTheValueIsScalar()
    {
        this.mappedInput = new MappedInput
        {
            PointId = "PNTTestPointId01",
            Timestamp = DateTime.UtcNow,
            Value = 20.5d,
        };
    }

    [Given(@"the Value is Json")]
    public void GivenTheValueIsJson()
    {
        dynamic jsonObject = new
        {
            CredentialIdentity = new
            {
                Type = "Access_Credential_Identity",
                Value = "1234",
            },
            Result = "granted",
        };

        var jsonString = JsonSerializer.Serialize(jsonObject);

        this.mappedInput = new MappedInput
        {
            PointId = "PNTTestPointId01",
            Timestamp = DateTime.UtcNow,
            Value = jsonString,
        };
    }

    [When(@"I call ProcessMappedTelemetry")]
    public void WhenICallProcessMappedTelemetry()
    {
        var cacheService = Substitute.For<IIdMapCacheService>();

        cacheService.GetConnectorId(this.mappedInput.PointId).Returns(Guid.NewGuid().ToString());

        // Substitutes
        var telemetryProcessor = new TelemetryProcessor(null!, new LiveData.Pipeline.HealthCheckTelemetryProcessor(), null!, cacheService);

        this.result = telemetryProcessor.ProcessMappedTelemetry(this.mappedInput);
    }

    [Then(@"the properties should be null")]
    public void ThenThePropertiesShouldBeNull()
    {
        Assert.Null(this.result?.Properties);
    }

    [Then(@"the ScalarValue should be Value")]
    public void ThenTheScalarValueShouldBeValue()
    {
        Assert.Equal(this.mappedInput.Value, this.result?.ScalarValue);
    }

    [Then(@"the properties should be Json")]
    public void ThenThePropertiesShouldBeJson()
    {
        Assert.Equal(this.mappedInput.Value, this.result?.Properties);
    }

    [Then(@"the ScalarValue should be (.*)")]
    public void ThenTheScalarValueShouldBe(int p0)
    {
        Assert.Equal(p0, this.result?.ScalarValue);
    }
}
