using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Units;

namespace Willow.Expressions.Visitor
{
    /// <summary>
    /// Simplifies any expression where it can convert math expressions into constants or
    /// boolean expressions that combine with true or false
    /// </summary>
    public class TokenExpressionSimplifier : TokenExpressionVisitor
    {
        /// <summary>
        /// Creates a new <see cref="TokenExpressionSimplifier"/>
        /// </summary>
        public TokenExpressionSimplifier()
            : base()
        {
        }

        /// <summary>
        /// Visit a binary double math expression
        /// </summary>
        protected TokenExpression VisitBinaryDoubleMath(TokenExpressionBinary input,
            Func<double, double, double> calc,
            Func<TokenExpression, TokenExpression, TokenExpression> create)
        {
            var left = input.Left.Accept(this);
            var right = input.Right.Accept(this);

            var leftDouble = left as TokenDouble;
            var rightDouble = right as TokenDouble;
            if (leftDouble != null && rightDouble != null)
            {
                return new TokenDouble(calc(leftDouble.ValueDouble, rightDouble.ValueDouble));
            }

            // a - a or a / a
            var leftVariable = left as TokenExpressionVariableAccess;
            var rightVariable = right as TokenExpressionVariableAccess;
            if (leftVariable != null && rightVariable != null &&
                (leftVariable.VariableName == rightVariable.VariableName))
            {
                if (input is TokenExpressionSubtract)
                    return new TokenDouble(0.0);
                if (input is TokenExpressionDivide)
                    return new TokenDouble(1.0);
            }

            // a + b - a
            var leftSum = left as TokenExpressionAdd;
            if (input is TokenExpressionSubtract && leftSum != null && rightVariable != null &&
                leftSum.Children.OfType<TokenExpressionVariableAccess>()
                    .Any(c => c.VariableName == rightVariable.VariableName))
            {
                return TokenExpressionAdd.Create(leftSum.Children.ExceptFirst(rightVariable).ToArray());
            }

            // a * b / a
            var leftMultiply = left as TokenExpressionMultiply;
            if (input is TokenExpressionDivide && leftMultiply != null && rightVariable != null &&
                leftMultiply.Children.OfType<TokenExpressionVariableAccess>()
                    .Any(c => c.VariableName == rightVariable.VariableName))
            {
                return TokenExpressionMultiply.Create(leftMultiply.Children.ExceptFirst(rightVariable).ToArray());
            }

            // NEED TO GENERALIZE THIS ... Make it recursive too so it knows when bits are equal ...?
            // Or have the minus distribute into the plus?
            ////// a + b + c - (b + a)
            ////var rightSum = right as TokenExpressionAdd;
            ////if (input is TokenExpressionSubtract && leftSum != null && rightSum != null)
            ////{
            ////    return TokenExpressionAdd.Create(leftSum.Children.ExceptFirst(rightVariable).ToArray());
            ////}

            // Handle (a * 0.01) / 0.01
            // Handle (a + 32) - 32
            //// Handle ((a * 0.01) / 0.1) / 0.1  -- i.e. neither side is constant at each node

            // GOAL: Convert division to multiplication and subtraction to addition
            // i.e. non-commutative to commutative

            if (input is TokenExpressionDivide && rightDouble != null)
            {
                var oneOverX = new TokenExpressionDivide(new TokenDouble(1.0), rightDouble);
                return new TokenExpressionMultiply(left, oneOverX).Simplify();
            }

            var rightMultiply = right as TokenExpressionMultiply;
            if (input is TokenExpressionDivide && rightMultiply != null)
            {
                // a / (b * c) ==> a * (1 / b) * (1 / c)
                return new TokenExpressionMultiply(Enumerable.Repeat(left, 1)
                    .Concat(rightMultiply.Children
                        .Select(c => new TokenExpressionDivide(new TokenDouble(1.0), c))).ToArray())
                    .Simplify();
            }

            if (input is TokenExpressionSubtract && rightDouble != null)
            {
                // x - 7 ==> x + (-7)
                var minus = new TokenDouble(-rightDouble.ValueDouble);
                return new TokenExpressionAdd(left, minus).Simplify();
            }

            var rightAdd = right as TokenExpressionAdd;
            if (input is TokenExpressionSubtract && rightAdd != null)
            {
                // a - (b + c + d) ==> a + (-b) + (-c) + (-d)
                return new TokenExpressionAdd(Enumerable.Repeat(left, 1)
                    .Concat(rightAdd.Children
                        .Select(c => new TokenExpressionUnaryMinus(c))).ToArray()).Simplify();
            }

            return create(left, right);
        }

        // A comparison a * 0.01 > b * 0.01 for instance
        /// <summary>
        /// Visit binary operators where constants can move from one side to another provided inverse operation is applied to other side
        /// </summary>
        protected TokenExpression VisitBinaryDoubleLogic(TokenExpressionBinary input,
            Func<double, double, bool> calc,
            Func<TokenExpression, TokenExpression, TokenExpression> create)
        {
            var left = input.Left.Accept(this);
            var right = input.Right.Accept(this);

            var leftDouble = left as TokenDouble;
            var rightDouble = right as TokenDouble;
            if (leftDouble != null && rightDouble != null)
            {
                // Purely a constant, can simplify
                return TokenExpressionConstantBool.Create(calc(leftDouble.ValueDouble, rightDouble.ValueDouble));
            }

            // Handle a * 0.01 < b * 0.01

            int stupidLimit = 100000;
            bool changed = true;
            while (changed)
            {
                changed = false;
                // Move constants to the right
                if (stupidLimit-- < 0) throw new Exception($"Infinite loop in {nameof(TokenExpressionSimplifier)}");

                if (left is TokenExpressionAdd leftAdd)
                {
                    var leftConstantChildren = leftAdd.Children.OfType<TokenDouble>().ToArray();
                    var leftNonConstantChildren = leftAdd.Children.Where(c => !(c is TokenDouble)).ToArray();

                    double result = leftConstantChildren.Aggregate(0.0, (c, v) => c + v.ValueDouble);

                    left = TokenExpressionAdd.Create(leftNonConstantChildren);
                    right = new TokenExpressionSubtract(right, result).Accept(this);
                    changed = changed || leftConstantChildren.Any();
                }

                if (left is TokenExpressionMultiply leftMultiply)
                {
                    var leftConstantChildren = leftMultiply.Children.OfType<TokenDouble>().ToArray();
                    var leftNonConstantChildren = leftMultiply.Children.Where(c => !(c is TokenDouble)).ToArray();

                    double result = leftConstantChildren.Aggregate(1.0, (c, v) => c * v.ValueDouble);

                    left = TokenExpressionMultiply.Create(leftNonConstantChildren);
                    right = new TokenExpressionDivide(right, result).Accept(this);
                    changed = changed || leftConstantChildren.Any();
                }

                // Have another look at LHS to see if it's a Math.Pow in hiding

                var leftMultiply2 = left as TokenExpressionMultiply;
                var rightDouble2 = right as TokenDouble;
                if (leftMultiply2 != null && leftMultiply2.Children.Count() > 1 && rightDouble2 != null)
                {
                    var first = leftMultiply2.Children.First();
                    bool allTheSame = leftMultiply2.Children.All(c => c.Equals(first));
                    int countMultiplyChildren = leftMultiply2.Children.Count();

                    if (allTheSame)
                    {
                        // Rewrite LHS as x and RHS as the square root, cube root, ... of the constant value
                        left = first;
                        right = new TokenDouble(Math.Pow(rightDouble2.ValueDouble, 1.0 / countMultiplyChildren));
                        changed = true;
                    }
                }

                var leftPower = left as TokenExpressionPower;
                var power = leftPower?.Right as TokenDouble;
                // a ^ constant = b
                if (leftPower != null && power != null && rightDouble2 != null)
                {
                    // Rewrite LHS as x and RHS as the square root, cube root, ... of the constant value
                    left = leftPower.Left;
                    right = new TokenDouble(Math.Pow(rightDouble2.ValueDouble, 1.0 / power.ValueDouble));
                    changed = true;
                }

                // TODO: Subtract and Divide
                // Harder because 2 - x > .. needs to become -x + 2 > ..
                ////////var leftSubtract = left as TokenExpressionSubtract;
                ////////if (leftSubtract != null)
                ////////{
                ////////    var children = new[] {leftSubtract.Left, leftSubtract.Right};
                ////////    var leftConstantChildren =  children.Where(c => c is TokenDouble).ToArray();
                ////////    var leftNonConstantChildren = children.Where(c => !(c is TokenDouble)).ToArray();

                ////////    left = TokenExpressionAdd.Create(leftNonConstantChildren);
                ////////    right = new TokenExpressionAdd(right, TokenExpressionAdd.Create(leftConstantChildren));
                ////////}
            }
            //    var leftBinary = leftMath as TokenExpressionBinary;
            //    if (leftBinary is TokenExpressionAdd && leftBinary.Right is TokenExpressionConstant)
            //    {
            //        leftMath = leftBinary.Left;
            //        rightMath = new TokenExpressionSubtract(rightMath, leftBinary.Right).Simplify();
            //    }

            //    if (leftBinary is TokenExpressionSubtract && leftBinary.Right is TokenExpressionConstant)
            //    {
            //        leftMath = leftBinary.Left;
            //        rightMath = new TokenExpressionAdd(rightMath, leftBinary.Right).Simplify();
            //    }

            //    if (leftBinary is TokenExpressionMultiply && leftBinary.Right is TokenExpressionConstant)
            //    {
            //        leftMath = leftBinary.Left;
            //        rightMath = new TokenExpressionDivide(rightMath, leftBinary.Right).Simplify();
            //    }

            //    if (leftBinary is TokenExpressionDivide && leftBinary.Right is TokenExpressionConstant)
            //    {
            //        leftMath = leftBinary.Left;
            //        rightMath = new TokenExpressionMultiply(rightMath, leftBinary.Right).Simplify();
            //    }
            //}

            return create(left, right);
        }

        /// <summary>
        /// Visit binary string logic
        /// </summary>
        protected TokenExpression VisitBinaryStringLogic(TokenExpressionBinary input, Func<string, string, bool> calc,
            Func<TokenExpression, TokenExpression, TokenExpression> create)
        {
            var left = input.Left.Accept(this);
            var right = input.Right.Accept(this);

            var leftString = left as TokenExpressionConstantString;
            var rightString = right as TokenExpressionConstantString;
            if (leftString != null && rightString != null)
            {
                return TokenExpressionConstantBool.Create(calc(leftString.ValueString, rightString.ValueString));
            }
            return create(left, right);
        }

        /// <summary>
        /// Visit commutative math
        /// </summary>
        protected TokenExpression VisitCommutativeMath(TokenExpressionNary input,
            double seed,
            Func<double, double, double> calc,
            Func<IEnumerable<TokenExpression>, TokenExpression> create)
        {
            var children = input.Children.Select(c => c.Accept(this)).ToList();

            // Pull up any children that are of the same type, e.g. multiply x multiply or add + add
            // And pull up opposites too but flip them
            var promotedChildren = children.SelectMany(x =>
            {
                if (x is null) throw new NullReferenceException("One of the children was null");
                if (x.GetType() == input.GetType())
                {
                    var y = x as TokenExpressionNary;
                    if (y is null) throw new NullReferenceException($"Type was {x.GetType().Name} wasn't n-ary");
                    // Use the element's children
                    return y.Children;
                }
                else if (x is TokenExpressionSubtract && input is TokenExpressionAdd)
                {
                    var y = (TokenExpressionSubtract)x;
                    return new[] { y.Left, new TokenExpressionUnaryMinus(y.Right).Simplify() };
                }
                else if (x is TokenExpressionDivide && input is TokenExpressionMultiply)
                {
                    // ...* a/b ==> ...* a * 1/b
                    var y = (TokenExpressionDivide)x;
                    return new[] { y.Left, new TokenExpressionDivide(new TokenDouble(1), y.Right).Simplify() };
                }
                else
                {
                    // Use the element
                    return Enumerable.Repeat(x, 1);
                }
            }).ToList();

            var childrenDouble = promotedChildren.OfType<TokenDouble>().ToList();

            var nonConstantChildren = promotedChildren.Where(x => !(x is TokenDouble)).ToList();

            // Handle (1 + 2 + (3 + 4)) by promoting children with the same operator

            // Combine all the constant ones
            double result = childrenDouble.Aggregate(seed, (c, v) => calc(c, v.ValueDouble));

            if (nonConstantChildren.Any())
            {
                // If result != seed then any non-constant children do not matter (0 + ...)
                // If result == seed then the result piece does not matter (0 * ...)

                if (result.Equals(seed))
                    // The constants made no difference
                    return create(nonConstantChildren.ToArray());
                else
                    // TODO : Handle DISTRIBUTIVE and get multiply to go into additions
                    return create(nonConstantChildren.Concat(Enumerable.Repeat(new TokenDouble(result), 1)));
            }
            return new TokenDouble(result);
        }

        /// <summary>
        /// Visit boolean logic
        /// </summary>
        protected TokenExpression VisitBoolLogic(TokenExpressionNary input,
            bool seed,
            Func<bool, bool, bool> calc,
            Func<IEnumerable<TokenExpression>, TokenExpression> create)
        {
            var children = input.Children.Select(c => c.Accept(this)).ToList();

            var childrenBool = children.OfType<TokenExpressionConstantBool>().ToList();
            var nonConstantChildren = children.Where(c => !(c is TokenExpressionConstantBool)).ToList();

            var promotedChildren = nonConstantChildren.SelectMany(x =>
            {
                if (x is null) throw new NullReferenceException("One of the children was null");
                if (x.GetType() == input.GetType())
                {
                    var y = x as TokenExpressionNary;
                    if (y is null) throw new NullReferenceException($"Type was {x.GetType().Name} wasn't nary");
                    return y.Children;
                }
                else
                {
                    return Enumerable.Repeat(x, 1);
                }
            }).ToArray();

            // Combine all the constant ones
            // false | A | B | C ....   vs true & A & B & C ....
            bool result = childrenBool.Aggregate(seed, (c, v) => calc(c, v.ValueBool));

            if (promotedChildren.Any())
            {
                // If result != seed then any non-constant children do not matter (false & ...)
                // If result == seed then the result piece does not matter (true & ...)
                // NOTE: Not quite the same as C# left to right evaluation with side-effects

                if (result == seed)
                {
                    // The constants made no difference
                    // But if there's only one now, skip wrapping it
                    if (promotedChildren.Length == 1) return promotedChildren[0];
                    return create(promotedChildren);
                }
                else
                {
                    // The constants win, e.g. a false in an & expression
                    return TokenExpressionConstantBool.Create(result);
                }
            }
            return TokenExpressionConstantBool.Create(result);
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionConvertToLocalDateTime input)
        {
            return new TokenExpressionConvertToLocalDateTime(input.Child.Accept(this));
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionMatches input)
        {
            var left = input.Left.Accept(this);
            var right = input.Right.Accept(this);

            return new TokenExpressionMatches(left, right);
        }

        // MATH

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionAdd input)
        {
            return this.VisitCommutativeMath(input, 0.0, (x, y) => x + y, (l) => TokenExpressionAdd.Create(l.ToArray()));
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionPower input)
        {
            return this.VisitBinaryDoubleMath(input, Math.Pow, (l, r) => new TokenExpressionPower(l, r));
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionSubtract input)
        {
            return this.VisitBinaryDoubleMath(input, (x, y) => x - y, (l, r) => new TokenExpressionSubtract(l, r));
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionMultiply input)
        {
            return this.VisitCommutativeMath(input, 1.0,
                (x, y) => x * y,
                (l) => TokenExpressionMultiply.Create(l.ToArray()));
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionDivide input)
        {
            return this.VisitBinaryDoubleMath(input, (x, y) => x / y, (l, r) => new TokenExpressionDivide(l, r));
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionUnaryMinus input)
        {
            var child = input.Child.Accept(this);
            // negative negative = positive
            if (child is TokenExpressionUnaryMinus childUnary)
            {
                return childUnary.Child;
            }
            // negative constant
            if (child is TokenDouble childDouble)
            {
                return new TokenDouble(-childDouble.ValueDouble);
            }
            return new TokenExpressionUnaryMinus(child);
        }

        // COMPARISONS

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionIs input)
        {
            return DoVisit(new TokenExpressionEquals(input.Left, input.Right));
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionEquals input)
        {
            if (input.Left is TokenExpressionConstant constantLeft && input.Right is TokenExpressionConstant constantRight)
            {
                if (constantLeft.Value.Equals(constantRight.Value)) return TokenExpression.True; else return TokenExpression.False;
            }

            // bool == False => !(bool)
            if (input.Left.Type == typeof(bool) && input.Right == TokenExpression.False)
            {
                return new TokenExpressionNot(input.Left.Accept(this));
            }

            // bool == True => (bool)
            if (input.Left.Type == typeof(bool) && input.Right == TokenExpression.True)
            {
                return input.Left.Accept(this);
            }

            // False == bool => !(bool)
            if (input.Right.Type == typeof(bool) && input.Left == TokenExpression.False)
            {
                return new TokenExpressionNot(input.Right.Accept(this));
            }

            // True == bool => (bool)
            if (input.Right.Type == typeof(bool) && input.Left == TokenExpression.True)
            {
                return input.Right.Accept(this);
            }

            // Can now call Equals on the whole thing, which should match arbitrary lefts and rights
            if (input.Left.Equals(input.Right)) return TokenExpression.True;

            return this.VisitBinaryDoubleLogic(input, (x, y) => x.Equals(y), (l, r) => new TokenExpressionEquals(l, r));
        }

        // TODO: Simplify unions and intersections

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionSetUnion input)
        {
            return new TokenExpressionSetUnion(input.Children.Select(c => c.Accept(this)).ToArray());
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionIntersection input)
        {
            return new TokenExpressionIntersection(input.Children.Select(c => c.Accept(this)).ToArray());
        }

        // TEMPORAL SET SIMPLIFIERS SHOULD ALL MOVE TO TEMPORAL SET SIMPLIFIER BASE

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionNotEquals input)
        {
            return this.VisitBinaryDoubleLogic(input, (x, y) => !x.Equals(y),
                (l, r) => new TokenExpressionNotEquals(l, r));
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionGreater input)
        {
            return this.VisitBinaryDoubleLogic(input, (x, y) => x > y, (l, r) => new TokenExpressionGreater(l, r));
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionGreaterOrEqual input)
        {
            return this.VisitBinaryDoubleLogic(input,
                (x, y) => x >= y,
                (l, r) => new TokenExpressionGreaterOrEqual(l, r));
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionLess input)
        {
            return this.VisitBinaryDoubleLogic(input, (x, y) => x < y, (l, r) => new TokenExpressionLess(l, r));
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionLessOrEqual input)
        {
            return this.VisitBinaryDoubleLogic(input, (x, y) => x <= y, (l, r) => new TokenExpressionLessOrEqual(l, r));
        }

        // LOGIC

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionNot input)
        {
            var child = input.Child.Accept(this);

            if (child is TokenExpressionNot childNot)
            {
                return childNot.Child;      // has already been visited
            }

            if (child is TokenExpressionConstantBool childBool)
            {
                return TokenExpressionConstantBool.Create(!childBool.ValueBool);
            }

            // TODO: Do we really need to call Accept again on these? We already simplified each child.

            if (child is TokenExpressionEquals childEquals)
            {
                return new TokenExpressionNotEquals(childEquals.Left, childEquals.Right).Accept(this);
            }

            if (child is TokenExpressionNotEquals childNotEquals)
            {
                return new TokenExpressionEquals(childNotEquals.Left, childNotEquals.Right).Accept(this);
            }

            if (child is TokenExpressionGreater childGreater)
            {
                return new TokenExpressionLessOrEqual(childGreater.Left, childGreater.Right).Accept(this);
            }

            if (child is TokenExpressionLess childLess)
            {
                return new TokenExpressionGreaterOrEqual(childLess.Left, childLess.Right).Accept(this);
            }

            if (child is TokenExpressionGreaterOrEqual childGreaterOrEqual)
            {
                return new TokenExpressionLess(childGreaterOrEqual.Left, childGreaterOrEqual.Right).Accept(this);
            }

            if (child is TokenExpressionLessOrEqual childLessOrEqual)
            {
                return new TokenExpressionGreater(childLessOrEqual.Left, childLessOrEqual.Right).Accept(this);
            }

            // Cannot simplify further than we already have ... except maybe for some other cases (distribute into an AND?)
            return new TokenExpressionNot(child);
        }

        /// <summary>
        /// Does children list contain two values that are opposites?
        /// </summary>
        /// <remarks>
        /// For an AND this would mean False, for an OR it would mean True
        /// </remarks>
        private bool MutuallyExclusive(TokenExpression[] children)
        {
            if (children.Count() < 2) return false;
            foreach (var child in children)
            {
                foreach (var child2 in children)
                {
                    if (child.Equals(child2)) continue;
                    if (child is TokenExpressionNot not && child2.Equals(not.Child)) return true;

                    // Both binary expressions and both use the same left AND right values
                    // Does not handle overlaps like A >=5 and A < 7
                    if (child is TokenExpressionBinary cbin1 && child2 is TokenExpressionBinary cbin2 &&
                        cbin1.Left.Equals(cbin2.Left) && cbin1.Right.Equals(cbin2.Right))
                    {
                        // A > 6 && A <= 6
                        if (child is TokenExpressionGreater && child2 is TokenExpressionLessOrEqual) return true;
                        if (child is TokenExpressionGreaterOrEqual && child2 is TokenExpressionLess) return true;
                        // Reverse is handled by double loop
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionAnd input)
        {
            var result = this.VisitBoolLogic(input, true, (x, y) => x && y, (c) => new TokenExpressionAnd(c.ToArray()));

            // A and not A
            if (result is TokenExpressionAnd tand && MutuallyExclusive(tand.Children)) return TokenExpression.False;

            return result;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionOr input)
        {
            var result = this.VisitBoolLogic(input, false, (x, y) => x || y, (c) => new TokenExpressionOr(c.ToArray()));

            // A or not A
            if (result is TokenExpressionOr tor && MutuallyExclusive(tor.Children)) return TokenExpression.True;

            return result;
        }

        /// <summary>
        /// Visit
        /// </summary>
        public override TokenExpression DoVisit(TokenExpressionIdentity input)
        {
            // Remove parentheses
            return input.Child.Accept(this);
        }
    }

    /// <summary>
    /// Enumerable extension for enumerating a sequence except for a value
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Enumerate a sequence skipping the first instance of a given item
        /// </summary>
        public static IEnumerable<T> ExceptFirst<T>(this IEnumerable<T> sequence, T itemToRemove)
            where T : IEquatable<T>
        {
            bool seen = false;
            foreach (var item in sequence)
            {
                if (!seen && item.Equals(itemToRemove))
                {
                    seen = true;
                }
                else
                {
                    yield return item;
                }
            }
        }
    }

    /// <summary>
    /// A power group expression
    /// </summary>
    public class PowerGroup
    {
        /// <summary>
        /// The TokenExpression
        /// </summary>
        public TokenExpression TokenExpression { get; set; }

        /// <summary>
        /// The Exponent
        /// </summary>
        public int Exponent { get; set; }

        /// <summary>
        /// Creates a new <see cref="PowerGroup"/>
        /// </summary>
        public PowerGroup(TokenExpression tokenExpression, int exponent)
        {
            this.TokenExpression = tokenExpression;
            this.Exponent = exponent;
        }
    }

    /// <summary>
    /// A Quadratic or higher expression
    /// </summary>
    /// <remarks>
    /// NOT CURRENTLY IN USE
    /// Simplifies abac to a^2 b c
    /// </remarks>
    internal class QuadraticOrHigher
    {
        private List<PowerGroup> items = new List<PowerGroup>();

        /// <summary>
        /// Create a new instance of the <see cref="QuadraticOrHigher"/> class
        /// </summary>
        public QuadraticOrHigher(TokenExpressionMultiply input)
        {
            foreach (var child in input.Children)
            {
                bool found = false;
                foreach (var item in items)
                {
                    if (item.TokenExpression == child)
                    {
                        item.Exponent++;
                        found = true;
                        break;
                    }
                    if (!found)
                    {
                        items.Add(new PowerGroup(child, 1));
                    }
                }
            }
        }
    }
}
