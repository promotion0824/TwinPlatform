using System;
using System.Linq.Expressions;

namespace Willow.Expressions.Visitor
{
    /// <summary>
    /// An undefined Expression result
    /// </summary>
    public class ExpressionUndefined : Expression
    {
        /// <summary>
        /// A Lambda for the Undefined Expression
        /// </summary>
        public static readonly LambdaExpression Instance = Lambda(new ExpressionUndefined());

        private static Expression throws = Expression.Block(typeof(bool),
            Expression.Throw(Expression.Constant(new Exception("Attempted to use an Undefined Expression"))),
            Expression.Constant(true));

        /// <summary>
        /// A DateTime,bool Lambda for the Undefined Expression
        /// </summary>
        public static readonly Expression<Func<DateTime, bool>> DateTimeBoolInstance =
            Lambda<Func<DateTime, bool>>(throws, Expression.Parameter(typeof(DateTime)));

        /// <summary>
        /// Gets the type of this Expression
        /// </summary>
        public override Type Type => typeof(object);

        /// <summary>
        /// Gets the NodeType
        /// </summary>
        public override ExpressionType NodeType => ExpressionType.Constant;

        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Undefined";
        }
    }
}
