using DigitalTwinCore.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using DigitalTwinCore.Constants;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using DigitalTwinCore.Exceptions;

namespace DigitalTwinCore.Test.Models
{
    public class TwinTests : BaseInMemoryTest
    {
        public TwinTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void WhenTwinHasUniqueIdCustomProperty_ThatUniqueIdIsReturned()
        {
            var guid = Guid.NewGuid();
            var twin = new Twin
            {
                Id = "TWIN_ID",
                CustomProperties = new Dictionary<string, object>(new KeyValuePair<string, object>[] { new KeyValuePair<string, object>(Properties.UniqueId, guid.ToString()) })
            };

            twin.UniqueId.Should().Be(guid);
        }

        [Fact]
        public void WhenTwinHasNoUniqueIdCustomProperty_GeneratedUniqueIdIsReturned()
        {
            var twin = new Twin
            {
                Id = "TWIN_ID"
            };

            // twin.UniqueId.Should().Be(new Guid("{c62cfdd1-8e45-a479-1e16-32cc7f0ac04f}"));
            Action act = () => { var _ = twin.UniqueId; };
            act.Should().Throw<DigitalTwinCoreException>().WithMessage("Bad request. No UniqueId found for twin: TWIN_ID");
        }

    }
}
