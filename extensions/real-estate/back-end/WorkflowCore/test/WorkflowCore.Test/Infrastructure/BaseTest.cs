using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using WorkflowCore.Entities;
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
            Fixture.Customizations.Add(new IgnorePropertyCustomization(nameof(TicketEntity),nameof(TicketEntity.EntityLifeCycleState)));
            Fixture.Customizations.Add(new IgnorePropertyCustomization(nameof(TicketEntity), nameof(TicketEntity.SubStatus)));
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
       /// ignore property customization
       /// </summary>
        public class IgnorePropertyCustomization: ISpecimenBuilder
        {
            private readonly IEnumerable<string> _names;
            private readonly string _className;
            /// <summary>
            /// 
            /// </summary>
            /// <param name="className">the name of the class that we want to ignore some of its properties </param>
            /// <param name="names"> list of property names we want to ignore </param>
            internal IgnorePropertyCustomization(string className ,params string[] names)
            {
                _names = names;
                _className = className;
            }

            public object Create(object request, ISpecimenContext context)
            {
                var propInfo = request as PropertyInfo;
                if (propInfo != null && _names.Contains(propInfo.Name) && propInfo?.DeclaringType?.Name == _className)
                    return new OmitSpecimen();

                return new NoSpecimen();
            }
        }
    }
}
