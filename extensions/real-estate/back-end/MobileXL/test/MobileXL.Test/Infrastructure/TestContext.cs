using Xunit.Abstractions;

namespace Willow.Tests.Infrastructure
{
    public class TestContext
    {
        public ITestOutputHelper Output { get; }

        public TestContext(ITestOutputHelper output)
        {
            Output = output;
        }
    }
}
