namespace Willow.Api.Common.Domain;

//https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-value-objects

/// <summary>
/// An class that defines the base class for value objects.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Defines the == operator.
    /// </summary>
    /// <param name="left">The Left instance of the Value Object.</param>
    /// <param name="right">The Right instance of the Value Object.</param>
    /// <returns>True if Left equals Right. False otherwise.</returns>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Defines the != operator.
    /// </summary>
    /// <param name="obj1">The Left instance of the Value Object.</param>
    /// <param name="obj2">The Right instance of the Value Object.</param>
    /// <returns>True if Left not equals Right. False otherwise.</returns>
    public static bool operator !=(ValueObject? obj1, ValueObject? obj2)
    {
        return !(obj1 == obj2);
    }

    /// <summary>
    /// Defines the Equal operator.
    /// </summary>
    /// <param name="left">The Left instance of the Value Object.</param>
    /// <param name="right">The Right instance of the Value Object.</param>
    /// <returns>True if reference to the Left object is the same as the reference to the right object. False otherwise.</returns>
    protected static bool EqualOperator(ValueObject? left, ValueObject? right)
    {
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
        {
            return false;
        }

        return ReferenceEquals(left, null) || left.Equals(right);
    }

    /// <summary>
    /// Defines the NotEqual operator.
    /// </summary>
    /// <param name="left">The Left instance of the Value Object.</param>
    /// <param name="right">The Right instance of the Value Object.</param>
    /// <returns>True if reference to the Left object is not the same as the reference to the right object. False otherwise.</returns>
    protected static bool NotEqualOperator(ValueObject? left, ValueObject? right)
    {
        return !EqualOperator(left, right);
    }

    /// <summary>
    /// Gets the equality components.
    /// </summary>
    /// <returns>A list of the equality components.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare to.</param>
    /// <returns>True if equal. False otherwise.</returns>
    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return this.GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Gets the hash code.
    /// </summary>
    /// <returns>Creates a unique hash code for a Value Object.</returns>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
    }
}
