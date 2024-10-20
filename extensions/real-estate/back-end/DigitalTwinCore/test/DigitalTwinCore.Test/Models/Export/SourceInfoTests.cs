using DigitalTwinCore.Models;
using FluentAssertions;
using Xunit;

namespace DigitalTwinCore.Test.Models
{
    public class SourceInfoTests
    {
        SourceInfo _sut = new SourceInfo();

        [Theory]
        [InlineData("All provided", "Account 1", "Contaner 1", "path 1")]
        [InlineData("Non provided", null, null, null)]
        public void IsValid_ValidData_Success(string caseName, string accountName, string containerName, string path)
        {
            _sut.AccountName = accountName;
            _sut.ContainerName = containerName;
            _sut.Path = path;

            _sut.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("empty account name", null, "Contaner 1", "path 1")]
        [InlineData("empty containerName name", "Account 1", null, "path 1")]
        [InlineData("empty path name", "Account 1", "Contaner 1", null)]
        public void IsValid_InvalidData_Fail(string caseName, string accountName, string containerName, string path)
        {
            _sut.AccountName = accountName;
            _sut.ContainerName = containerName;
            _sut.Path = path;

            _sut.IsValid.Should().BeFalse();
        }

        [Fact]
        public void IsEmpty_EmptyData_ReturnsTrue()
        {
            _sut.AccountName = null;
            _sut.ContainerName = null;
            _sut.Path = null;

            _sut.IsEmpty.Should().BeTrue();
        }


        [Theory]
        [InlineData("empty accountName", null, "Contaner 1", "path 1")]
        [InlineData("empty container and path", "Account 1", null, null)]
        public void IsEmpty_NotEmptyData_ReturnsFalse(string caseName, string accountName, string containerName, string path)
        {
            _sut.AccountName = accountName;
            _sut.ContainerName = containerName;
            _sut.Path = path;

            _sut.IsEmpty.Should().BeFalse();
        }
    }
}
