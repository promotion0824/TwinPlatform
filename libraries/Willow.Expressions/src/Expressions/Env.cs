using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Willow.Units;

namespace Willow.Expressions
{
    /// <summary>
    /// An environment is a chain of linked stack frames containing variables
    /// or functions and their definitions
    /// It also handles undefining a value in the current stack frame
    /// </summary>
    public class Env : IEquatable<Env>
    {
        // This part is for DateTimes - TBD if this goes away

        /// <summary>
        /// You can override the default time provider to evaluate an expression at a different point in time
        /// e.g. when someone last week said next week they mean this week
        /// </summary>
        public Willow.Units.TimeProvider TimeProvider { get; set; }

        // This part of the environment is for variables to IConvertible

        private readonly Dictionary<string, BoundValue<object>> boundValues = new(StringComparer.OrdinalIgnoreCase);

        private readonly Env? parent = null;

        private IEnumerable<Env> SelfAndAncestors()
        {
            var current = this;
            while (current != null)
            {
                yield return current;
                current = current.parent;
            }
        }

        private IEnumerable<BoundValue<object>> GetBoundValues()
        {
            // ReSharper disable LoopCanBePartlyConvertedToQuery - performance
            var seen = new HashSet<string>();
            foreach (var env in this.SelfAndAncestors())
            {
                foreach (var boundValue in env.boundValues.Values)
                {
                    if (!seen.Contains(boundValue.VariableName))
                    {
                        yield return boundValue;
                        seen.Add(boundValue.VariableName);
                    }
                }
            }
            // ReSharper restore LoopCanBePartlyConvertedToQuery
        }

        private static readonly Lazy<Env> emptyValue = new(() => new Env());

        /// <summary>
        /// The empty environment
        /// </summary>
        public static Env Empty => emptyValue.Value;

        /// <summary>
        /// Is there a definition for a given variable name
        /// </summary>
        public bool IsDefined(string variableName)
        {
            return this.GetInternal(variableName).HasValue;
        }

        /// <summary>
        /// Get a given value
        /// </summary>
        public object? Get(string variableName)
        {
            var maybe = this.GetInternal(variableName);
            return maybe?.Value;
        }

        /// <summary>
        /// Get a given value
        /// </summary>
        public BoundValue<object>? GetBoundValue(string variableName)
        {
            return this.GetInternal(variableName);
        }

        /// <summary>
        /// Get and cast a given value
        /// </summary>
        public bool TryGet<T>(string variableName, out T? value)
        {
            var maybe = this.GetInternal(variableName);
            if (!maybe.HasValue)
            {
                value = default;
                return false;
            }
            if (maybe.Value.Value is T t)
            {
                value = t;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        /// <summary>
        /// Get the type of a given variable
        /// </summary>
        public Type? GetVariableType(string variableName)
        {
            var maybe = this.GetInternal(variableName);
            return maybe?.Value?.GetType();
        }

        /// <summary>
        /// Get a given value (or null)
        /// </summary>
        private BoundValue<object>? GetInternal(string variableName)
        {
            if (string.IsNullOrEmpty(variableName)) return null;

            Env? current = this;

            while (current is not null)
            {
                if (current.boundValues.TryGetValue(variableName, out var boundValue))
                {
                    return boundValue;
                }

                // otherwise, go up a level in the stack and try there
                current = current.parent;
            }
            // Did not find it
            return null;
        }

        /// <summary>
        /// Get the variable names
        /// </summary>
        public IEnumerable<string> Variables => this.GetBoundValues().Select(x => x.VariableName).ToList();

        /// <summary>
        /// Gets the bound values
        /// </summary>
        public IEnumerable<BoundValue<object>> BoundValues => this.GetBoundValues();

        private Env()
        {
            this.parent = null!;
            this.TimeProvider = Willow.Units.TimeProvider.Current;
        }

        private Env(Env parent)
        {
            this.parent = parent;
            this.TimeProvider = parent.TimeProvider;
        }

        /// <summary>
        /// Assign mutates the current stack frame
        /// </summary>
        public Env Assign(string name, object value, string? units = null)
        {
            if (this.parent is null) throw new Exception("Please push a stack frame before assigning");

            this.boundValues[name] = new BoundValue<object>(name, value, units);
            return this;
        }

        /// <summary>
        /// Push a new stack frame
        /// </summary>
        public Env Push()
        {
            return new Env(this);
        }

        /// <summary>
        /// Pop a stack frame
        /// </summary>
        /// <returns></returns>
        public Env? Pop()
        {
            return this.parent;
        }

        /// <summary>
        /// Equals
        /// </summary>
        public bool Equals(Env? other)
        {
            if (other is null) return false;

            // Must match stackframe by stackframe (used to be just variables were same)

            var currentThis = this;
            var currentOther = other;

            while (currentThis != null && currentOther != null)
            {
                if (!currentThis.boundValues.SequenceEqual(currentOther.boundValues)) return false;
                currentThis = currentThis.parent;
                currentOther = currentOther.parent;
            }

            return currentOther is null && currentThis is null;
        }

        private static volatile int count = 0;

        private readonly int identity = Interlocked.Increment(ref count);

        public override int GetHashCode()
        {
            // Not great, but it will do - IEquatable needs a Hashcode definition
            return identity;
        }

        public override bool Equals(object? obj)
        {
            return obj is Env e && this.Equals(e);
        }

        public override string ToString()
        {
            return "Env:" + string.Join("; ", this.BoundValues);
        }

        /// <summary>
        /// Merge values and create a new Env with them both
        /// </summary>
        public Env Merge(Env env)
        {
            Env result = Env.Empty.Push();
            foreach (var v in this.GetBoundValues())
            {
                result.Assign(v.VariableName, v.Value, v.Units);
            }
            foreach (var v in env.GetBoundValues())
            {
                result.Assign(v.VariableName, v.Value, v.Units);
            }
            return result;
        }

        /// <summary>
        /// Dump the bound values to a string
        /// </summary>
        public string DebugDump => string.Join(Environment.NewLine, this.BoundValues);
    }
}
