using System;
using System.Linq;
using Xunit;
using PlatformPortalXL;
using PlatformPortalXL.Extensions;
using System.Collections.Generic;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class ObjectExtensionsTests
    {
        public ObjectExtensionsTests()
        {
        }

        [Fact]
        public void ExpandoObject_FirstOrDefault_NullObject()
        {
            var obj = "".ToCamelCaseExpandoObject();
            var val = obj.FirstOrDefault<string>("key");
            Assert.Null(val);
        }

        [Fact]
        public void ExpandoObject_FirstOrDefault_ExistsInt()
        {
            var obj = "{ \"key\": 5 }".ToCamelCaseExpandoObject();
            var val = obj.FirstOrDefault<Int64>("key");
            Assert.Equal(5, val);
        }

        [Fact]
        public void ExpandoObject_FirstOrDefault_ExistsString()
        {
            var obj = "{ \"key\": \"test\" }".ToCamelCaseExpandoObject();
            var val = obj.FirstOrDefault<string>("key");
            Assert.Equal("test", val);
        }

        [Fact]
        public void ExpandoObject_FirstOrDefault_NotExists()
        {
            var obj = "{ \"key\": \"test\" }".ToCamelCaseExpandoObject();
            Assert.Null(obj.FirstOrDefault<string>("key1"));
            Assert.Equal(0, obj.FirstOrDefault<Int64>("key1"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("{ \"key\": \"test\" }")]
        [InlineData("{ \"control1\": \"\"}")]
        public void ExpandoObject_AllOrDefault_ReturnsEmpty(string json)
        {
            var obj = json.ToCamelCaseExpandoObject();

            Assert.False(obj.AllOrDefault("control").Any());
        }

        [Theory]
        [InlineData("{ \"control\": \"test\", \"test\": \"testvalue\" }", new string[] { "test" }, new string[] { "testvalue" } )]
        [InlineData("{ \"control1\": \"test\", \"test\": \"testvalue\" }", new string[] { "test" }, new string[] { "testvalue" })]
        [InlineData("{ \"control1\": \"test\", \"test\": \"\" }", new string[] { "test" }, new string[] { "" })]
        [InlineData("{ \"control1\": \"test\" }", new string[] { "test" }, new string[] { null })]
        [InlineData("{ \"control1\": \"test\", \"test\": \"testvalue\", \"control2\": \"test2\", }", new string[] { "test", "test2" }, new string[] { "testvalue", null })]
        [InlineData("{ \"control1\": \"test\", \"test\": \"testvalue\", \"control2\": \"test2\", \"test2\": \"test2value\"}", new string[] { "test", "test2" }, new string[] { "testvalue", "test2value" })]
        [InlineData("{ \"control1\": \"test\", \"test\": \"testvalue\", \"control2\": \"test\", }", new string[] { "test" }, new string[] { "testvalue" })]
        public void ExpandoObject_AllOrDefault_ReturnsOne(string json, string[] keys, string[] values)
        {
            var obj = json.ToCamelCaseExpandoObject();

            var result = obj.AllOrDefault("control");

            Assert.True(result.Count() == keys.Count());
            Assert.Equal(keys, result.Select(x => x.Key).ToArray());
            Assert.Equal(values, result.Select(x => x.Value).ToArray());

        }
    }
}