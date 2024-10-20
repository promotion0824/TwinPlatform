using System;
using AutoFixture;
using AutoFixture.Kernel;

namespace PlatformPortalXL.Test.Infrastructure;

internal class UtcRandomDateTimeSequenceGenerator : ISpecimenBuilder
{
    private readonly RandomDateTimeSequenceGenerator _innerRandomDateTimeSequenceGenerator = new ();

    public object Create(object request, ISpecimenContext context)
    {
        var result = _innerRandomDateTimeSequenceGenerator.Create(request, context);
        return result is NoSpecimen ? result : ((DateTime)result).ToUniversalTime();
    }
}
