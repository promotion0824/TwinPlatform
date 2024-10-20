using System;

namespace Willow.Expressions
{
    /// <summary>
    /// Structure returned from GetUnboundVariables
    /// </summary>
    public struct UnboundVariableOrFunction : IEquatable<UnboundVariableOrFunction>
    {
        /// <summary>
        /// Get the name of this unbound variable or function
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Is this unbound object a function?
        /// </summary>
        public bool IsFunction { get; }

        /// <summary>
        /// Is this unbound object a field?
        /// </summary>
        public bool IsField { get; }

        /// <summary>
        /// Create a new instance of the <see cref="UnboundVariableOrFunction"/> class
        /// </summary>
        public UnboundVariableOrFunction(string name, bool isFunction)
        {
            this.Name = name;
            this.IsFunction = isFunction;
            this.IsField = false;
        }

        /// <summary>
        /// Equals
        /// </summary>
        public bool Equals(UnboundVariableOrFunction other)
        {
            return (this.Name, this.IsField, this.IsFunction) ==
                (other.Name, other.IsField, other.IsFunction);
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is UnboundVariableOrFunction u && Equals(u);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
