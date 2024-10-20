using Microsoft.Extensions.DependencyInjection;
using System;

namespace Willow.AzureDigitalTwins.Api.Extensions;

/// <summary>
/// Use this interface to inject optional dependencies, which is cleaner than the other way
/// which register a customer resolver for the interface at startup for every service has optional dependencies. 
/// </summary>
public interface IOptionalDependency<T>
{
    T? Value { get; }
}

public class OptionalDependency<T>(IServiceProvider serviceProvider) : IOptionalDependency<T>
{
    public T? Value { get; } = serviceProvider.GetService<T>();
}
