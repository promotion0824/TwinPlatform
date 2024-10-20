namespace Willow.LiveData.TelemetryDataQuality.Tests;

using TechTalk.SpecFlow;
using Willow.LiveData.TelemetryDataQuality.Models.TimeSeries;

[Binding]
public class TimeSeriesStepDefinitions
{
    private TimeSeries? testTimeSeries;

    [Given(@"I have a temperature sensor with Unit (.*)")]
    public void GivenIHaveATemperatureSensorWithUnit(string unit)
    {
        this.testTimeSeries = new TimeSeries("test", unit)
        {
            ModelId = "TemperatureSensor;1",
            TrendInterval = 900,
        };

        this.testTimeSeries.EnableValidation();
    }

    [Given(@"I have a modeled capability in ADT")]
    public void GivenIHaveAModeledCapabilityInAdt()
    {
        this.testTimeSeries = new TimeSeries("test", "W")
        {
            ModelId = "ElectricalPowerSensor;1",
            TrendInterval = 900,
            DtId = "TestTwin-1",
        };

        this.testTimeSeries.EnableValidation();
    }

    [When(@"the incoming value is (.*)")]
    public void WhenTheIncomingValueIs(int p0)
    {
        this.testTimeSeries?.AddPoint(new TimedValue(DateTimeOffset.Now, p0));
        this.testTimeSeries?.SetStatus(DateTimeOffset.Now);
    }

    [When(@"invalid incoming value is ""(.*)""")]
    public void WhenInvalidIncomingValueIs(string someRandomString)
    {
        this.testTimeSeries?.AddPoint(new TimedValue(DateTimeOffset.Now, someRandomString));
        this.testTimeSeries?.SetStatus(DateTimeOffset.Now);
    }

    [When(@"there is no twin modelled")]
    public void WhenThereIsNoTwinModelled()
    {
        this.testTimeSeries?.SetStatus(DateTimeOffset.Now);
    }

    [When(@"a new valid value is received")]
    public void WhenANewValidValueIsReceived()
    {
        this.testTimeSeries?.AddPoint(new TimedValue(DateTimeOffset.Now, 1000));
        this.testTimeSeries?.SetStatus(DateTimeOffset.Now);
    }

    [When(@"no new value is received for more than (.*)x the trendInterval")]
    public void WhenNoNewValueIsReceivedForMoreThanXTheTrendInterval(int p0)
    {
        this.testTimeSeries?.AddPoint(new TimedValue(DateTimeOffset.Now, 30));
        this.testTimeSeries?.SetStatus(DateTimeOffset.Now.AddSeconds((double)(p0 * this.testTimeSeries.TrendInterval!)));
    }

    [Then(@"the out of range validation should be (.*)")]
    public void ThenTheOutOfRangeValidationShouldBe(bool result)
    {
        var status = this.testTimeSeries?.GetStatus();
        Assert.That(
            status & TimeSeriesStatus.ValueOutOfRange,
            result ? Is.EqualTo(TimeSeriesStatus.ValueOutOfRange) : Is.Not.EqualTo(TimeSeriesStatus.ValueOutOfRange));
    }

    [Then(@"the validation result should be (.*)")]
    public void ThenTheValidationResultShouldBe(bool result)
    {
        var status = this.testTimeSeries?.GetStatus();
        Assert.That(
            status & TimeSeriesStatus.Valid,
            result ? Is.EqualTo(TimeSeriesStatus.Valid) : Is.Not.EqualTo(TimeSeriesStatus.Valid));
    }

    [Then(@"the no twin validation should be (.*)")]
    public void ThenTheNoTwinValidationShouldBe(bool result)
    {
        var status = this.testTimeSeries?.GetStatus();
        Assert.That(
            status & TimeSeriesStatus.NoTwin,
            result ? Is.EqualTo(TimeSeriesStatus.NoTwin) : Is.Not.EqualTo(TimeSeriesStatus.NoTwin));
    }

    [Then(@"the offline validation result should be (.*)")]
    public void ThenTheOfflineValidationResultShouldBe(bool result)
    {
        var status = this.testTimeSeries?.GetStatus();
        Assert.That(
            status & TimeSeriesStatus.Offline,
            result ? Is.EqualTo(TimeSeriesStatus.Offline) : Is.Not.EqualTo(TimeSeriesStatus.Offline));
    }
}
