using AutoFixture;
using AutoFixture.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace NotificationCore.Test.Infrastructure;

public abstract class BaseTest
{
    protected ITestOutputHelper Output { get; }
    protected abstract TestContext TestContext { get; }
    public Fixture Fixture = new Fixture();

    protected BaseTest(ITestOutputHelper output)
    {
        Output = output;
        Fixture.Customizations.Add(new UtcRandomDateTimeSequenceGenerator());
        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                    .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

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
    /// <summary>
    /// Set Default value for Enum
    /// when object is created using AutoFixture
    /// this behavior can be overridden using `With`
    /// </summary>
    public class EnumSpecimenBuilder<TEnum> : ISpecimenBuilder where TEnum : Enum
    {
        private readonly TEnum _value;

        public EnumSpecimenBuilder(TEnum value)
        {
            _value = value;
        }

        public object Create(object request, ISpecimenContext context)
        {
            var pi = request as PropertyInfo;
            var type = pi?.PropertyType;
            if (type == null || type != typeof(TEnum))
            {
                return new NoSpecimen();
            }

            return _value;
        }
    }

}
