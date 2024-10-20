using System.Diagnostics;
using System.Linq;

namespace Willow.Expressions.Visitor;

/// <summary>
/// Optimises expressions that only contains const expressions, by replacing them with the calcualted result using <see cref="ConvertToValueVisitor{TSource}" />
/// </summary>
public class ConstOptimizerVisitor : TokenExpressionVisitor
{
    private ConvertToValueVisitor<Env> convertToValue;

    public ConstOptimizerVisitor(Env env)
    {
        this.convertToValue = new ConvertToValueVisitor<Env>(env, (e, s) =>
        {
            var expression = e.Get(s);

            if (expression is TokenExpressionConstant constant)
            {
                return constant.Value;
            }

            return double.NaN;
        });
    }

    public override TokenExpression DoVisit(TokenExpressionTernary input)
    {
        var visited = base.DoVisit(input);

        if (visited is TokenExpressionTernary ternary)
        {
            if (ternary.Conditional is TokenExpressionConstantBool constBool)
            {
                if (constBool.ValueBool)
                {
                    return ternary.Truth;
                }
                else
                {
                    return ternary.Falsehood;
                }
            }
        }

        return visited;
    }

    protected override TokenExpression OnVisited(TokenExpression input, TokenExpression expression)
    {
        if (CanOptimise(expression))
        {
            //dont visit the array, wait for the parent (e.g. MAX, MIN, etc)
            if (expression is TokenExpressionArray)
            {
                return expression;
            }

            var value = expression.Accept(convertToValue);

            if (ConvertToValueVisitor<Env>.IsNumber(value))
            {
                if (double.IsNaN(value.ToDouble(null)))
                {
                    return expression;
                }

                var result = TokenExpressionConstant.Create(value);

                if (expression is TokenExpressionUnaryMinus minus)
                {
                    //unary minus is special get the child's unit
                    result.Unit = minus.Child.Unit;
                }
                else
                {
                    result.Unit = expression.Unit;
                }

                return result;
            }

            if (ConvertToValueVisitor<Env>.IsBoolean(value))
            {
                return TokenExpressionConstant.Create(value);
            }
        }

        return expression;
    }

    private bool CanOptimise(TokenExpression expression)
    {
        if (!IsAllowedForVisitor(expression))
        {
            return false;
        }

        var children = expression.GetChildren();

        if (children.Any())
        {
            foreach (var child in children)
            {
                if (!IsAllowedForVisitor(child))
                {
                    return false;
                }

                bool isConst = child is TokenExpressionConstant;

                if (!isConst)
                {
                    if (child is TokenExpressionVariableAccess variable)
                    {
                        var value = convertToValue.DoVisit(variable);

                        if (value.GetTypeCode() == System.TypeCode.String && value != UndefinedResult.Undefined)
                        {
                            isConst = true;
                        }

                        if (ConvertToValueVisitor<Env>.IsNumber(value) && !double.IsNaN(value.ToDouble(null)))
                        {
                            isConst = true;
                        }
                    }
                }

                //if this expression is not a const, then all the children has to be consts
                if (!isConst)
                {
                    if (!CanOptimise(child))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        return false;
    }

    private static bool IsAllowedForVisitor(TokenExpression expression)
    {
        return !(expression is TokenExpressionFailed);
    }
}
