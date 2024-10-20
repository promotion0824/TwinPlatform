namespace Willow.LiveData.Core.Tests.UnitTests
{
    using System;
    using FluentAssertions;
    using NUnit.Framework;
    using Willow.LiveData.Core.Common;
    using Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;

    public class AdxCTokenProviderTests
    {
        private readonly IAdxContinuationTokenProvider<string, int> continuationTokenProvider;

        public AdxCTokenProviderTests()
        {
            continuationTokenProvider = new AdxCTokenProvider();
        }

        [TestCase("", null)]
        [TestCase("sdf", null)]
        [TestCase("sdf", 0)]
        [TestCase("xvxcv", 234234)]
        [TestCase("sdf", -123123)]
        public void AdxStoredQueryResultTokenProvider_GetToken_Returns_Correctly_Concatenated_Valid_Tokens(string item1, int item2)
        {
            //arrange

            //act
            var uniqueToken = continuationTokenProvider.GetToken(item1, item2);

            //assert
            $"{item1}-{Math.Abs(item2)}".Should().Be(uniqueToken);
        }

        [TestCase("")]
        [TestCase("sdf-0")]
        [TestCase("xvxcv-234234")]
        [TestCase("sdf-123123")]
        public void AdxStoredQueryResultTokenProvider_Parse_Returns_Correctly_Parses_Valid_Tokens(string token)
        {
            //arrange

            //act
            (var part1, var part2) = continuationTokenProvider.ParseToken(token);

            //assert
            if (token.IndexOf("-") != -1)
            {
                var parts = token.Split("-");
                parts[0].Should().Be(part1);
                parts[1].Should().Be(part2.ToString());
            }
            else
            {
                part1.Length.Should().Be(0);
                part2.Should().Be(0);
            }
        }

        [TestCase("sdf")]
        public void AdxStoredQueryResultTokenProvider_Parse_Throws_Exception_When_Invalid_Tokens(string token)
        {
            //arrange

            //act
            Action action = () => continuationTokenProvider.ParseToken(token);

            //assert
            action.Should().Throw<InvalidCastException>();
        }
    }
}
