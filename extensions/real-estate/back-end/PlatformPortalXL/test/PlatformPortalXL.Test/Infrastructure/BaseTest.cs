using AutoFixture;
using PlatformPortalXL.Test.Infrastructure;
using Xunit.Abstractions;

namespace Willow.Tests.Infrastructure
{
    public abstract class BaseTest
    {
        protected ITestOutputHelper Output { get; }
        protected abstract TestContext TestContext { get;  }
        protected readonly Fixture Fixture = new();

        protected BaseTest(ITestOutputHelper output)
        {
            Output = output;
            Fixture.Customizations.Add(new RandomDateOnlySequenceGenerator());
            Fixture.Customizations.Add(new UtcRandomDateTimeSequenceGenerator());
        }

        public ServerFixture CreateServerFixture(ServerFixtureConfiguration serverConfiguration)
        {
            return new ServerFixture(serverConfiguration, TestContext);
        }
    }
}
