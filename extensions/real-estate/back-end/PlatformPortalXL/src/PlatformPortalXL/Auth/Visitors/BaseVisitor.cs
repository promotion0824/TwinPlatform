using System;
using Willow.Expressions;
using Willow.Expressions.Visitor;

namespace PlatformPortalXL.Auth.Visitors;

public abstract class BaseVisitor<T> : ITokenExpressionVisitor<T>
{
    public virtual T DoVisit(TokenExpressionFunctionCall input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionCount input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionAverage input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionAny input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionIdentity input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionEach input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionAll input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionPropertyAccess input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionVariableAccess input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionConstant input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionConstantNull input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionConstantDateTime input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionConstantString input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionArray input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionConstantBool input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionConstantColor input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenDouble input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionConvertToLocalDateTime input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionAdd input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionMatches input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionDivide input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionUnaryMinus input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionMultiply input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionPower input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionSubtract input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionNot input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionAnd input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionOr input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionTernary input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionIntersection input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionSetUnion input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionIs input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionEquals input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionGreater input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionGreaterOrEqual input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionLess input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionLessOrEqual input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionNotEquals input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionTuple input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionSum input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionParameter input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionWrapped input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionFirst input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionMin input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionMax input)
    {
        throw new NotImplementedException();
    }

    public virtual T DoVisit(TokenExpressionFailed input)
    {
        throw new NotImplementedException();
    }

    public T DoVisit(TokenExpressionTemporal input)
    {
        throw new NotImplementedException();
    }

    public T DoVisit(TokenExpressionTimer input)
    {
        throw new NotImplementedException();
    }

    public T Visit(TokenExpression source)
    {
        return source.Accept(this);
    }
}
