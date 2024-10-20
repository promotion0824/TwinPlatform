using Xunit.Abstractions;

namespace Willow.Tests.Infrastructure
{
    public class BaseInMemoryTest : BaseTest
    {
        public const string RulesEngineAppName = "Willow Activate";
        public const string RulesEngineAppId = "aaf0a355-739d-4dfc-92b9-01da4aabe9e9";
        public const string MappedAppName = "Mapped";
        public const string MappedAppId = "aaf0a355-739d-4dfc-92b9-01da4aabe9e8";
        public BaseInMemoryTest(ITestOutputHelper output) : base(output)
        {
        }

        protected override TestContext TestContext => new TestContext(Output, ConditionalDatabaseFixture.GetDatabaseInstance(false));
    }
}
