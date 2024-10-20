using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.ExpressionParser;
using Willow.Expressions;

namespace WillowExpressions.Test
{
    /// <summary>
    /// Tests to make sure current culture does not affect serialization.
    /// </summary>
    [TestClass]
    public class CultureTests
    {
        private void CheckSerialized(TokenExpression expression, string str)
        {
            expression.Serialize().Should().Be(str);
        }

        [TestMethod]
        public void ListAllCulturesThatUseCommaDecimalSeparator()
        {
            CultureInfo[] availableCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo cultureInfo in availableCultures)
            {
                if (cultureInfo.NumberFormat.NumberDecimalSeparator != ",")
                {
                    continue;
                }
                Console.WriteLine($"{cultureInfo.DisplayName}, {cultureInfo.Name}, {cultureInfo.LCID}");
            }
        }

        [TestMethod]
        public void CanParseNumbers()
        {
            var culture = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .FirstOrDefault(x => x.NumberFormat.NumberDecimalSeparator.Equals(","));

            if (culture is null)
            {
                Console.WriteLine("No culture found with decimal separator comma, cannot run test");
                return;
            }

            var savedCulture = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = culture;

            culture.NumberFormat.NumberDecimalSeparator.Should().Be(",");

            TokenExpression expr1 = Parser.Deserialize("1");
            CheckSerialized(expr1, "1");

            TokenExpression expr2 = Parser.Deserialize("-200");
            CheckSerialized(expr2, "-200");

            TokenExpression expr3 = Parser.Deserialize("-202.2");
            CheckSerialized(expr3, "-202.2");

            Thread.CurrentThread.CurrentCulture = savedCulture;
        }
    }
}
