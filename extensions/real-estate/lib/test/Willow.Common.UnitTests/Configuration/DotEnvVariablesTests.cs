using System.IO;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Willow.Common.Configuration;
using Xunit;

namespace Willow.Common.UnitTests.Configuration;

public class DotEnvVariablesTests
{
    [Fact(Skip = "Need to figure out how to load env files in the CI")]
    public void GetsConfigFromEnvFileWithCustomPath()
    {
        var config = new ConfigurationBuilder();
        config.AddDotEnvVariablesFile(Path.Combine(Directory.GetCurrentDirectory(), "custom.env"));

        var root = config.Build();
        root["TestValue"].Should().Be("A");
    }

    [Fact(Skip = "Need to figure out how to load env files in the CI")]
    public void GetsConfigFromEnvFile()
    {
        var config = new ConfigurationBuilder();
        config.AddDotEnvVariablesFile();

        var root = config.Build();
        root["TestValue"].Should().Be("A");
    }

    [Fact(Skip = "Need to figure out how to load env files in the CI")]
    public void GetsConfigFromEnvFileWithEqualsSignInValue()
    {
        var config = new ConfigurationBuilder();
        config.AddDotEnvVariablesFile();

        var root = config.Build();
        root["TestValueWithEquals"].Should().Be("B=C");
    }

    [Fact(Skip = "Need to figure out how to load env files in the CI")]
    public void HandlesKeyDelimiterInVariableNames()
    {
        var config = new ConfigurationBuilder();
        config.AddDotEnvVariablesFile();

        var root = config.Build();
        root["Object:Value"].Should().Be("D");
    }

    [Fact(Skip = "Need to figure out how to load env files in the CI")]
    public void GetsSectionsViaDelimiters()
    {
        var config = new ConfigurationBuilder();
        config.AddDotEnvVariablesFile();

        var root = config.Build();
        root.GetSection("Object")["Value"].Should().Be("D");
    }
}
