namespace Willow.Expressions
{
    /// <summary>
    /// A bound value is the result of a variable assignment
    /// </summary>
    public readonly struct BoundValue<T>
        where T : notnull
    {
        /// <summary>
        /// The variable name
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// The units for the value
        /// </summary>
        public string? Units { get; }

        /// <summary>
        /// The value of the variable
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="BoundValue{T}"/> class
        /// </summary>
        public BoundValue(string name, T value, string? units)
        {
            this.VariableName = name;
            this.Value = value;
            this.Units = units;
        }

        public override string ToString() => $"{this.VariableName}:={this.Value}";

        public override bool Equals(object? obj)
        {
            return obj is BoundValue<T> bc && this.VariableName.Equals(bc.VariableName) && this.Value.Equals(bc.Value);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(VariableName, Value);
        }

        public static bool operator ==(BoundValue<T> left, BoundValue<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BoundValue<T> left, BoundValue<T> right)
        {
            return !(left == right);
        }
    }
}
