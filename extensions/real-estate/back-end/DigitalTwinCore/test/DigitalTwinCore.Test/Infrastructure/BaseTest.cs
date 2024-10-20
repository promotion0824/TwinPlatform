using System;
using AutoFixture;
using AutoFixture.Kernel;
using Xunit.Abstractions;

namespace Willow.Tests.Infrastructure
{
    public abstract class BaseTest
    {
        protected ITestOutputHelper Output { get; }
        protected abstract TestContext TestContext { get;  }
        public Fixture Fixture = new Fixture();

        protected BaseTest(ITestOutputHelper output)
        {
            Output = output;
            Fixture.Customizations.Add(new UtcRandomDateTimeSequenceGenerator());
        }

        public ServerFixture CreateServerFixture(ServerFixtureConfiguration serverConfiguration)
        {
            return new ServerFixture(serverConfiguration, TestContext);
        }

        class UtcRandomDateTimeSequenceGenerator : ISpecimenBuilder
        {
            private readonly ISpecimenBuilder innerRandomDateTimeSequenceGenerator;

            internal UtcRandomDateTimeSequenceGenerator()
            {
                this.innerRandomDateTimeSequenceGenerator = 
                    new RandomDateTimeSequenceGenerator();
            }

            public object Create(object request, ISpecimenContext context)
            {
                var result = 
                    this.innerRandomDateTimeSequenceGenerator.Create(request, context);
                if (result is NoSpecimen)
                    return result;

                return ((DateTime)result).ToUniversalTime();
            }
        }

    }
}
