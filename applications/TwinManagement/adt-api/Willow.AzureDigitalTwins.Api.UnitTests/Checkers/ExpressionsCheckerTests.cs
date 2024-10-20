using Willow.DataQuality.Execution.Checkers;

namespace Willow.AzureDigitalTwins.Api.UnitTests.Checkers;

public class ExpressionsCheckerTests
{
    private readonly ExpressionsChecker _expressionsChecker;
    public ExpressionsCheckerTests()
    {
        _expressionsChecker = new ExpressionsChecker();
    }
}
