using Xunit;

using PlatformPortalXL.Helpers;

namespace Willow.PlatformPortal.XL.UnitTests
{
    public class HttpHelperTests
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("   ", "")]
        [InlineData("foo", "myValue=foo")]
        [InlineData("#", "myValue=%23")]
        [InlineData("\\", "myValue=%5c")]
        [InlineData(1, "myValue=1")]
        public void ToQueryString_SingleKey(object value, string expected)
        {
            Assert.Equal(expected, HttpHelper.ToQueryString(new
            {
                myValue = value
            }));
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("blah", "foo", "keyA=blah&keyB=foo")]
        [InlineData("#", "#", "keyA=%23&keyB=%23")]
        [InlineData(1, 2, "keyA=1&keyB=2")]
        [InlineData(1, "boo", "keyA=1&keyB=boo")]
        public void ToQueryString_MultipleKeys(object valueA, object valueB, string expected)
        {
            Assert.Equal(expected, HttpHelper.ToQueryString(new
            {
                keyA = valueA,
                keyB = valueB,
                keyC = ""
            }));
        }

        [Fact]
        public void ToQueryString_MultipleKeysWithEnumerableValue()
        {
            Assert.Equal("value1=%23&value1=%24&value1=%26&value2=2", HttpHelper.ToQueryString(new
            {
                value1 = new string[] { "#", "$", "&" },
                value2 = 2,
                value3 = ""
            }));
        }

    }
}
