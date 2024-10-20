using System;

namespace Willow.ExpressionParser
{
    /// <summary>
    /// An Exception that occured during parsing of an expression
    /// </summary>
    public class ParserException : Exception
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ParserException"/> class
        /// </summary>
        /// <param name="message"></param>
        public ParserException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ParserException"/> class
        /// </summary>
        /// <param name="message"></param>
        public ParserException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}
