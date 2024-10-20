using System;
using System.Linq;

namespace Willow.Expressions.Visitor
{
    /// <summary>
    /// An argument for a <see cref="RegisteredFunction"/>
    /// </summary>
    public struct RegisteredFunctionArgument
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RegisteredFunctionArgument(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of parameter
        /// </summary>
        public Type Type { get; set; }
    }

    /// <summary>
    /// A RegisteredFunction is used in two places. Firstly to instruct the parser what additional functions to recognize
    /// e.g. DATETPART(DatePart,DateTime,int) and secondly to generate the output (e.g. a SQL string) from a TokenExpression
    /// using that RegisteredFunction.
    /// </summary>
    /// <remarks>
    /// These registered functions may have special handling for arguments, e.g. SQL DATEPART first argument is not
    /// quoted.
    /// </remarks>
    public struct RegisteredFunction
    {
        /// <summary>
        /// Name of the function
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Argument types
        /// </summary>
        public Type[] ArgumentTypes { get; set; }

        /// <summary>
        /// Arguments
        /// </summary>
        public RegisteredFunctionArgument[] Arguments { get; set; }

        /// <summary>
        /// The body of the function
        /// </summary>
        public TokenExpression? Body { get; set; }

        /// <summary>
        /// Result Type
        /// </summary>
        public Type ResultType { get; set; }

        /// <summary>
        /// A function that converts an array of children into a SQL string
        /// </summary>
        public Func<TokenExpression[], string>? Conversion { get; set; }

        /// <summary>
        /// Create a new RegisteredFunction passing in a Func with the same signature
        /// </summary>
        /// <typeparam name="T">Must be a Func of the required signature</typeparam>
        /// <param name="name">Name of the function (e.g. SQL "DATEPART")</param>
        /// <param name="conversion">Conversion function</param>
        /// <returns>A new RegisteredFunction</returns>
        public static RegisteredFunction Create<T>(string name, Func<TokenExpression[], string>? conversion = null)
        {
            if (!typeof(T).IsGenericType) throw new ArgumentException("Must be a generic Func type");
            if (!typeof(T).GetGenericTypeDefinition().Name.StartsWith("Func")) throw new ArgumentException("Must be a Func type");

            var types = typeof(T).GetGenericArguments();
            return new RegisteredFunction
            {
                ArgumentTypes = types.Take(types.Length - 1).ToArray(),
                ResultType = types.Last(),
                Name = name,
                Conversion = conversion
            };
        }

        /// <summary>
        /// Create a new RegisteredFunction passing configured values
        /// </summary>
        public static RegisteredFunction Create(string name, RegisteredFunctionArgument[] arguments, TokenExpression body)
        {
            return new RegisteredFunction
            {
                ArgumentTypes = arguments.Select(v => v.Type).ToArray(),
                Arguments = arguments,
                ResultType = body.Type,
                Name = name,
                Body = body
            };
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is RegisteredFunction rf && this.Name.Equals(rf.Name) && this.ResultType.Equals(rf.ResultType) &&
                this.ArgumentTypes.SequenceEqual(rf.ArgumentTypes);     // actual function doesn't matter, cannot define two the same
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            return (this.Name, this.ArgumentTypes.Length).GetHashCode();
        }
    }
}
