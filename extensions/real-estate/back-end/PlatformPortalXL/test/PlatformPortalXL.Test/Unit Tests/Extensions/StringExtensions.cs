using System.Linq;
using Xunit;
using PlatformPortalXL;
using System.Collections.Generic;
using System.Dynamic;
using System;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class StringExtensionsTests
    {
        public StringExtensionsTests()
        {
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ToCamelCaseExpandoObject_NullObject(string json)
        {
            var obj = json.ToCamelCaseExpandoObject();

            Assert.NotNull(obj);
            Assert.False(obj.Any());
        }

        [Theory]
        [InlineData("{ \"keyTest\": 5 }", 5)]
        [InlineData("{ \"KeyTest\": 5 }", 5)]
        [InlineData("{ \"keyTest\": \"Test\" }", "Test")]
        [InlineData("{ \"KeyTest\": \"Test\" }", "Test")]
        public void ToCamelCaseExpandoObject_SimpleJson(string json, object value)
        {
            var obj = json.ToCamelCaseExpandoObject();

            Assert.True(obj.All(x => x.Key == "keyTest"));
            Assert.True(obj.All(x => x.Value.ToString() == value.ToString()));
        }

        [Theory]
        [InlineData("{ \"objTest\": {\"name\": \"Test\"}}", 0, 0, "Test")]
        [InlineData("{ \"groupTest\": [{\"name\": \"Test\"}]}", 0, 0, "Test")]
        [InlineData("{ \"groupTest\": [{\"name\": \"Test\", \"position\": 5 }, {\"position\": 6 }]}", 0, 1, (Int64)5)]
        [InlineData("{ \"groupTest\": [{\"name\": \"Test\", \"position\": 5 }, {\"position\": 6 }]}", 1, 0, (Int64)6)]
        public void ToCamelCaseExpandoObject_NestedJson(string json, int arrIdx, int propIdx, object value)
        {
            var obj = json.ToCamelCaseExpandoObject();

            var v = obj.First().Value;

            if (v is IEnumerable<object>)
            {
                v = (v as IEnumerable<object>).ElementAt(arrIdx);
            }

            Assert.Equal(value, (v as ExpandoObject).ElementAt(propIdx).Value);
        }
    }
}