using System;
using AutoFixture.Xunit2;
using DigitalTwinCore.Extensions;
using FluentAssertions;
using Xunit;

namespace DigitalTwinCore.Test.Extensions
{
    public class DataReaderExtensionsTests
    {
        [Theory]
        [AutoData]
        public void DataReaderParser_Can_Parse_DifferentTypes(StringParser[] data)
        {
            var reader = Helpers.CreateDataReader(data);

            var result = reader.Object.Parse<StringParser>();

            result.Should().BeEquivalentTo(data);
        }

        public class StringParser
        {
            public string StringValue { get; set; }
            public Guid GuidValue { get; set; }
            public bool BooleanValue { get; set; }
            public DateTime DateTimeValue { get; set; }
            public int IntValue { get; set; }
        }
    }
}
