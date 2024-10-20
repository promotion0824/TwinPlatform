using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Willow.Expressions.Visitor;

namespace Willow.Expressions
{
    /// <summary>
    /// An Array of token expressions
    /// </summary>
    public class TokenExpressionArray : TokenExpressionNary, IConvertible, IEquatable<TokenExpressionArray>
    {
        /// <summary>
        /// Priority is used to enforce precedence rules
        /// </summary>
        public override int Priority => 100;

        /// <summary>
        /// Are the children unordered?
        /// </summary>
        protected override bool IsUnordered { get => false; }

        /// <summary>
        /// The .NET Type of this <see cref="TokenExpression"/>
        /// </summary>
        public override Type Type => typeof(IEnumerable<double>);   // Maybe ... what about others?

        /// <summary>
        /// Create a new instance of the <see cref="TokenExpressionArray"/> class
        /// </summary>
        public TokenExpressionArray(params TokenExpression[] children) : base(children)
        {
        }

        /// <summary>
        /// Accepts the visitor
        /// </summary>
        public override T Accept<T>(ITokenExpressionVisitor<T> visitor)
        {
            return visitor.DoVisit(this);
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            return $"{{{string.Join(",", this.Children.AsEnumerable())}}}";
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(TokenExpression? other)
        {
            if (!(other is TokenExpressionArray array)) return false;
            if (this.Children is null) return array.Children is null;
            return this.Children.SequenceEqual(array.Children);
        }

        /// <summary>
        /// Equals
        /// </summary>
        public override bool Equals(object? other)
        {
            return (other is TokenExpressionArray tea) && Equals(tea);
        }

        /// <summary>
        /// GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            if (this.Children is null) return 1;
            return this.Children.Select(x => x.GetHashCode()).DefaultIfEmpty(27).First();
        }

        /// <summary>
        /// Get the TypeCode for this conversion
        /// </summary>
        /// <returns></returns>
        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public bool ToBoolean(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to boolean");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public char ToChar(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to char");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public sbyte ToSByte(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to SByte");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public byte ToByte(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to byte");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public short ToInt16(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to Int16");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public ushort ToUInt16(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to UInt16");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public int ToInt32(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to Int32");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public uint ToUInt32(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to UInt32");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public long ToInt64(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to Int64");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public ulong ToUInt64(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to UInt64");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public float ToSingle(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to float");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public double ToDouble(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to double");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public decimal ToDecimal(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to decimal");
        }

        /// <summary>
        /// IConvertible
        /// </summary>
        public DateTime ToDateTime(IFormatProvider? provider)
        {
            throw new NotImplementedException($"Cannot convert to DateTime");
        }

        /// <summary>
        /// ToString
        /// </summary>
        public string ToString(IFormatProvider? provider)
        {
            return $"[{string.Join(",", this.Children.Select(x => x.Serialize()))}]";
        }

        /// <summary>
        /// Convert to a given type (IConvertible)
        /// </summary>
        public object ToType(Type conversionType, IFormatProvider? provider)
        {
            if (conversionType == typeof(int[]))
            {
                return this.Children.Select(c => (int)(c.ToDouble(CultureInfo.InvariantCulture) ?? 0.0))
                    .ToArray();
            }
            if (conversionType == typeof(double[]))
            {
                return this.Children.Select(c => c.ToDouble(CultureInfo.InvariantCulture)).ToArray();
            }
            if (conversionType == typeof(string[]))
            {
                return this.Children.Select(c => c.ToString(CultureInfo.InvariantCulture)).ToArray();
            }
            throw new NotImplementedException($"Cannot convert to {conversionType.Name}");
        }

        /// <summary>
        /// Equals
        /// </summary>
        public bool Equals(TokenExpressionArray? other)
        {
            // Check each element matches
            return other is TokenExpressionArray &&
                this.Children.Zip(other.Children, (x, y) => x.Equals(y)).All(x => x);
        }
    }

    /// <summary>
    /// A TypeConvertor for TokenExpressionArray values
    /// </summary>
    public class ArrayConverter : TypeConverter
    {
        /// <summary>
        /// Returns true if conversion is possible
        /// </summary>
        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        {
            return (destinationType == typeof(string)) || base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        /// Converts to the given destination type
        /// </summary>
        public override object? ConvertTo(ITypeDescriptorContext? context,
                 CultureInfo? culture,
                 object? value,
                 Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                TokenExpression te = (TokenExpression)value!;
                return te!.Serialize();
            }
            return base.ConvertTo(context!, culture!, value!, destinationType!);
        }

        /// <summary>
        /// Returns true if conversion is possible
        /// </summary>
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType)
        {
            return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType!);
        }

        /// <summary>
        /// Converts from the given type
        /// </summary>
        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is TokenExpressionArray)
                return value;
            if (value is string s && s.TrimStart().StartsWith("[") && s.TrimEnd().EndsWith("]"))
            {
                s = s.Trim().TrimStart('[').TrimEnd(']');
                if (s == "") return new TokenExpressionArray();
                var parts = s.Split(',').Select<string, IConvertible>(p =>
                {
                    if (double.TryParse(p, out double r)) return r;
                    else return p;
                });
                // ReSharper disable once ConvertClosureToMethodGroup
                return new TokenExpressionArray(parts.Select(p => TokenExpressionConstant.Create(p)).ToArray());
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
