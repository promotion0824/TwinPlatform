using AutoFixture;
using AutoFixture.Xunit2;

namespace PlatformPortalXL.Test.Infrastructure;

/// <summary>
/// <see cref="T:AutoDataAttribute"/> with a customised <see cref="T:Fixture"/>.
/// </summary>
public class CustomAutoDataAttribute : AutoDataAttribute
{
    public CustomAutoDataAttribute() : base(CreateFixture) { }

    private static IFixture CreateFixture()
    {
        var fixture = new Fixture();

        fixture.Customizations.Add(new RandomDateOnlySequenceGenerator());

        return fixture;
    }
}
