using Xunit.Abstractions;

namespace NotificationCore.Test.Infrastructure;
public class BaseInMemoryTest : BaseTest
{
    public const string RulesEngineAppName = "Willow Activate";
    public BaseInMemoryTest(ITestOutputHelper output) : base(output)
    {
    }

    protected override TestContext TestContext => new TestContext(Output, ConditionalDatabaseFixture.GetDatabaseInstance(false));
}
