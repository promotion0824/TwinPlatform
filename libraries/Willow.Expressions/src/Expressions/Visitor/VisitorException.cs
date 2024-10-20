using System;

namespace Willow.Expressions.Visitor;

/// <summary>
/// An Exception that occured during visiting an expression
/// </summary>
public class VisitorException : Exception
{
    /// <summary>
    /// Creates a new instance of the <see cref="VisitorException"/> class
    /// </summary>
    /// <param name="message"></param>
    public VisitorException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="VisitorException"/> class
    /// </summary>
    /// <param name="message"></param>
    public VisitorException(string message, Exception ex) : base(message, ex)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="VisitorException"/> class
    /// </summary>
    /// <param name="message"></param>
    public VisitorException(string expression, string message, Exception ex) : base(message, ex)
    {
        Expression = expression;
    }

    /// <summary>
    /// The expression that caused the exception
    /// </summary>
    public string Expression { get; } = "";
}
