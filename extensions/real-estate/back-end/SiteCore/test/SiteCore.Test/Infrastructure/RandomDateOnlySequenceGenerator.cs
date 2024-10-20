using System;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;

namespace SiteCore.Test.Infrastructure;

/// <summary>
/// Creates random <see cref="T:System.DateOnly" /> specimens.
/// </summary>
/// <remarks>
/// The generated <see cref="T:System.DateOnly" /> values will be within
/// a range of ± two years from today's date, unless a different range
/// has been specified in the constructor.
/// AutoFixture does not natively support <see cref="T:System.DateOnly" />
/// generation.
/// </remarks>
public class RandomDateOnlySequenceGenerator : ISpecimenBuilder
{
    private readonly RandomNumericSequenceGenerator _randomizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:RandomDateOnlySequenceGenerator" /> class.
    /// </summary>
    public RandomDateOnlySequenceGenerator() : this(
        DateOnly.FromDateTime(DateTime.Today.AddYears(-2)),
        DateOnly.FromDateTime(DateTime.Today.AddYears(2)))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:RandomDateOnlySequenceGenerator" /> class
    /// for a specific range of dates.
    /// </summary>
    /// <param name="minDate">The lower bound of the date range.</param>
    /// <param name="maxDate">The upper bound of the date range.</param>
    /// <exception cref="T:System.ArgumentException">
    /// <paramref name="minDate" /> is greater than <paramref name="maxDate" />.
    /// </exception>
    public RandomDateOnlySequenceGenerator(DateOnly minDate, DateOnly maxDate)
    {
        if (minDate >= maxDate)
        {
            throw new ArgumentException("The 'minDate' argument must be less than the 'maxDate'.");
        }

        _randomizer = new RandomNumericSequenceGenerator([
            minDate.ToDateTime(TimeOnly.MinValue).Ticks,
            maxDate.ToDateTime(TimeOnly.MinValue).Ticks
        ]);
    }

    /// <summary>
    /// Creates a new <see cref="T:System.DateOnly" /> specimen based on a request.
    /// </summary>
    /// <param name="request">The request that describes what to create.</param>
    /// <param name="context">Not used.</param>
    /// <returns>
    /// A new <see cref="T:System.DateOnly" /> specimen, if <paramref name="request" /> is a request for a
    /// <see cref="T:System.DateOnly" /> value; otherwise, a <see cref="T:AutoFixture.Kernel.NoSpecimen" /> instance.
    /// </returns>
    public object Create(object request, ISpecimenContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return IsDateOnlyRequest(request) ? CreateRandomDate(context) : new NoSpecimen();
    }

    private static bool IsDateOnlyRequest(object request)
    {
        return typeof(DateOnly).GetTypeInfo().IsAssignableFrom(request as Type);
    }

    private object CreateRandomDate(ISpecimenContext context)
    {
        var randomDateTime = new DateTime(GetRandomNumberOfTicks(context));
        return DateOnly.FromDateTime(randomDateTime);
    }

    private long GetRandomNumberOfTicks(ISpecimenContext context)
    {
        return (long)_randomizer.Create(typeof(long), context);
    }
}
