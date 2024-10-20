namespace Willow.ExpressionParser
{
    public partial class Parser
    {
        /// <summary>
        /// A Token recognized by the <see cref="Parser"/>
        /// </summary>
        private abstract class Token
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            internal static readonly FixedToken Comma = new FixedToken(",");
            internal static readonly FixedToken LParen = new FixedToken("(");
            internal static readonly FixedToken RParen = new FixedToken(")");

            internal static readonly FixedToken LSquareParen = new FixedToken("[");
            internal static readonly FixedToken RSquareParen = new FixedToken("]");

            internal static readonly FixedToken LCurlyParen = new FixedToken("{");
            internal static readonly FixedToken RCurlyParen = new FixedToken("}");

            // Postfix operator
            internal static readonly OperatorToken Percentage = new OperatorToken("%", 1);

            // Unary operators
            internal static readonly OperatorToken UnaryNot = new OperatorToken("!", 7);
            internal static readonly OperatorToken UnaryMinus = new OperatorToken("-", 7);

            // Variable.Property has a higher precedence than any numeric operator
            internal static readonly OperatorToken Dot = new OperatorToken(".", 8);

            internal static readonly OperatorToken SemiColon = new OperatorToken(";", 3);
            internal static readonly OperatorToken Plus = new OperatorToken("+", 4);
            internal static readonly OperatorToken Minus = new OperatorToken("-", 4);
            internal static readonly OperatorToken Multiply = new OperatorToken("*", 5);
            internal static readonly OperatorToken Divide = new OperatorToken("/", 5);
            internal static readonly OperatorToken Power = new OperatorToken("^", 6);

            internal static readonly OperatorToken Equal = new OperatorToken("=", 3);
            internal static readonly OperatorToken NotEqual = new OperatorToken("!=", 3);
            internal static readonly OperatorToken Less = new OperatorToken("<", 3);
            internal static readonly OperatorToken LessEqual = new OperatorToken("<=", 3);
            internal static readonly OperatorToken Greater = new OperatorToken(">", 3);
            internal static readonly OperatorToken GreaterEqual = new OperatorToken(">=", 3);
            internal static readonly OperatorToken Is = new OperatorToken("is", 3);

            internal static readonly OperatorToken MemberOf = new OperatorToken("∈", 2);
            internal static readonly OperatorToken Intersection = new OperatorToken("∩", 2);
            internal static readonly OperatorToken Union = new OperatorToken("∪", 2);

            internal static readonly OperatorToken And = new OperatorToken("&", 1);
            internal static readonly OperatorToken Or = new OperatorToken("|", 1);

            // Future, math symbols
            internal static readonly OperatorToken ForAll = new OperatorToken("∀", 2);
            internal static readonly OperatorToken ThereExists = new OperatorToken("∃", 2);
            internal static readonly OperatorToken AsymptoticallyEqual = new OperatorToken("≃", 2);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// A fixed token
        /// </summary>
        private class FixedToken : Token
        {
            /// <summary>
            /// The string for this Token (used for debugging only)
            /// </summary>
            public string Value { get; }

            /// <summary>
            /// Creates a new instance of the <see cref="FixedToken"/> class
            /// </summary>
            public FixedToken(string value)
            {
                this.Value = value;
            }

            /// <summary>
            /// ToString
            /// </summary>
            public override string ToString()
            {
                return this.Value;
            }
        }

        /// <summary>
        /// A Token representing an operator
        /// </summary>
        private class OperatorToken : FixedToken
        {
            /// <summary>
            /// The precedence level for this Token
            /// </summary>
            public int Precedence { get; }

            /// <summary>
            /// Creates a new instance of the <see cref="OperatorToken"/> class
            /// </summary>
            public OperatorToken(string value, int precedence)
                : base(value)
            {
                this.Precedence = precedence;
            }
        }

        /// <summary>
        /// A Token representing a constant number
        /// </summary>
        private class NumberConstantToken : Token
        {
            /// <summary>
            /// The numeric value of the token
            /// </summary>
            public double Value { get; }

            /// <summary>
            /// Creates a new instance of the <see cref="NumberConstantToken"/> class
            /// </summary>
            public NumberConstantToken(double value)
            {
                this.Value = value;
            }

            /// <summary>
            /// ToString
            /// </summary>
            public override string ToString()
            {
                return this.Value.ToString();
            }
        }

        /// <summary>
        /// A Token representing a string value
        /// </summary>
        private abstract class StringValuedToken : Token
        {
            /// <summary>
            /// The value
            /// </summary>
            public string Value { get; }

            /// <summary>
            /// Creates a new instance of of the <see cref="StringValuedToken"/> class
            /// </summary>
            protected StringValuedToken(string value)
            {
                this.Value = value;
            }
        }

        /// <summary>
        /// A token representing an identifier
        /// </summary>
        private class IdentifierToken : StringValuedToken
        {
            /// <summary>
            /// Creates a new instance of the <see cref="IdentifierToken"/> class
            /// </summary>
            public IdentifierToken(string identifier) : base(identifier)
            {
            }

            /// <summary>
            /// ToString
            /// </summary>
            public override string ToString()
            {
                return $"{this.Value}";
            }
        }

        /// <summary>
        /// A quoted string token
        /// </summary>
        private class QuotedStringToken : StringValuedToken
        {
            /// <summary>
            /// Creates a new instance of the <see cref="QuotedStringToken"/> class
            /// </summary>
            /// <param name="value"></param>
            public QuotedStringToken(string value) : base(value)
            {
            }

            /// <summary>
            /// ToString
            /// </summary>
            public override string ToString()
            {
                return $"\"{this.Value}\"";
            }
        }
    }
}
