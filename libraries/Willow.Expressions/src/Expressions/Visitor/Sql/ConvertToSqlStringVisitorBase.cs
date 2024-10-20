using System;
using System.Collections.Generic;

namespace Willow.Expressions.Visitor.Sql
{
    /// <summary>
    /// Base class for conversion to Sql strings, contains non-generic parts
    /// </summary>
    public class ConvertToSqlStringVisitorBase
    {
        /// <summary>
        /// Represents the special DatePart component of SQL commands (which is a string without quotes on it)
        /// </summary>
        public class DatePart
        {
        }

        /// <summary>
        /// A list of all SQL functions that can be used
        /// </summary>
        public static readonly List<RegisteredFunction> RegisteredFunctions = new List<RegisteredFunction>()
        {
            //other
            RegisteredFunction.Create<Func<bool, bool>>("NOT"),
            RegisteredFunction.Create<Func<DatePart, int, DateTime, DateTime>>("DATEADD"),
            RegisteredFunction.Create<Func<DatePart, DateTime, int>>("DATEPART"),
            RegisteredFunction.Create<Func<DatePart, DateTime, string>>("DATENAME"),
            RegisteredFunction.Create<Func<int, int, int, DateTime>>("DATEFROMPARTS"),
            RegisteredFunction.Create<Func<int, int, int, int, int, int, int, DateTime>>("DATETIMEFROMPARTS"),
            RegisteredFunction.Create<Func<int, int, int, int, int, int, int, int, int, int, DateTime>>("DATETIMEOFFSETFROMPARTS"),
            RegisteredFunction.Create<Func<int, int, int, int, int, int, int, int, DateTime>>("DATETIME2FROMPARTS"),
            RegisteredFunction.Create<Func<DatePart, DateTime, DateTime, int>>("DATEDIFF"),
            RegisteredFunction.Create<Func<DatePart, DateTime, DateTime, int>>("DATEDIFF_BIG"),
            RegisteredFunction.Create<Func<DateTime, string>>("DAY"),
            RegisteredFunction.Create<Func<string, string, int>>("DIFFERENCE"),
            RegisteredFunction.Create<Func<DateTime, int>>("MONTH"),
            RegisteredFunction.Create<Func<DateTime, string>>("YEAR"),

            //string functions
            RegisteredFunction.Create<Func<string, int>>("ASCII"), // char or varchar
            RegisteredFunction.Create<Func<int, string>>("CHAR"), // returns char
            RegisteredFunction.Create<Func<string, string, int, int>>("CHARINDEX"),
            RegisteredFunction.Create<Func<string, string, int>>("CHARINDEX"),
            RegisteredFunction.Create<Func<string, string, string>>("CONCAT"),
            RegisteredFunction.Create<Func<string, string, string, string>>("CONCAT"),
            RegisteredFunction.Create<Func<string, string, string, string, string>>("CONCAT"),
            RegisteredFunction.Create<Func<object, string>>("FORMAT"),
            RegisteredFunction.Create<Func<object, string, string>>("FORMAT"),
            RegisteredFunction.Create<Func<string, int, string>>("LEFT"),
            RegisteredFunction.Create<Func<string, int>>("LEN"),
            RegisteredFunction.Create<Func<string, string, bool>>("LIKE"),
            RegisteredFunction.Create<Func<string, string>>("LOWER"),
            RegisteredFunction.Create<Func<string, string>>("LTRIM"),
            RegisteredFunction.Create<Func<int, string>>("NCHAR"),
            //// First string is %...%
            RegisteredFunction.Create<Func<string, string>>("PATINDEX"),
            RegisteredFunction.Create<Func<string, string>>("QUOTENAME"),
            RegisteredFunction.Create<Func<string, char, string>>("QUOTENAME"),
            RegisteredFunction.Create<Func<string, string, string>>("REPLACE"),
            RegisteredFunction.Create<Func<string, int, string>>("REPLICATE"),
            RegisteredFunction.Create<Func<string, string>>("REVERSE"),
            RegisteredFunction.Create<Func<string, int, string>>("RIGHT"),
            RegisteredFunction.Create<Func<string, string>>("RTRIM"),
            RegisteredFunction.Create<Func<string, int>>("SOUNDEX"),
            RegisteredFunction.Create<Func<int, string>>("SPACE"),
            RegisteredFunction.Create<Func<double, string>>("STR"),
            RegisteredFunction.Create<Func<double, int, string>>("STR"),
            RegisteredFunction.Create<Func<double, int, int, string>>("STR"),
            //// not supported ["STRING_AGG"]=typeof(Func<string, string>),
            //// STRING_ESCAPE(expression, 'json')
            RegisteredFunction.Create<Func<string, string, string>>("STRING_ESCAPE"),
            RegisteredFunction.Create<Func<string, char, string[]>>("STRING_SPLIT"),
            RegisteredFunction.Create<Func<string, string, string[]>>("STRING_SPLIT"),

            RegisteredFunction.Create<Func<string, int, int, string, string>>("STUFF"),
            RegisteredFunction.Create<Func<string, int, int, string>>("SUBSTRING"),
            RegisteredFunction.Create<Func<string, int>>("UNICODE"),
            RegisteredFunction.Create<Func<string, string>>("UPPER"),

            RegisteredFunction.Create<Func<string, string, string, string>>("TRANSLATE"),
            //RegisteredFunction.Create<Func<string, string, nvarchar>("STRING_AGG"),
            RegisteredFunction.Create<Func<string, string, string>>("TRIM"),
            RegisteredFunction.Create<Func<string, int>>("DIFFERENCE"),

            //math functions
            RegisteredFunction.Create<Func<double, double>>("ABS"),
            RegisteredFunction.Create<Func<float, float>>("ACOS"),
            RegisteredFunction.Create<Func<float, float>>("ASIN"),
            RegisteredFunction.Create<Func<float, float>>("ATAN"),
            RegisteredFunction.Create<Func<float, float>>("ATN2"),
            RegisteredFunction.Create<Func<double, double>>("CEILING"),
            RegisteredFunction.Create<Func<float, float>>("COS"),
            RegisteredFunction.Create<Func<float, float>>("COT"),
            RegisteredFunction.Create<Func<double, double>>("DEGREES"),
            RegisteredFunction.Create<Func<float, float>>("EXP"),
            RegisteredFunction.Create<Func<double, double>>("FLOOR"),
            RegisteredFunction.Create<Func<float, float>>("LOG"),
            RegisteredFunction.Create<Func<float, float>>("LOG10"),
            RegisteredFunction.Create<Func<float>>("PI"),
            RegisteredFunction.Create<Func<float, float>>("POWER"),
            RegisteredFunction.Create<Func<double, double>>("RADIANS"),
            RegisteredFunction.Create<Func<int, float>>("RAND"),
            RegisteredFunction.Create<Func<float>>("RAND"),
            RegisteredFunction.Create<Func<double, int, int, double>>("ROUND"),
            RegisteredFunction.Create<Func<double, int, double>>("ROUND"),
            RegisteredFunction.Create<Func<double, double>>("SIGN"),
            RegisteredFunction.Create<Func<float, float>>("SIN"),
            RegisteredFunction.Create<Func<float, float>>("SQRT"),
            RegisteredFunction.Create<Func<float, float>>("SQUARE"),
            RegisteredFunction.Create<Func<float, float>>("TAN"),

            //logical functions
            RegisteredFunction.Create<Func<int, double, double>>("CHOOSE"),
            RegisteredFunction.Create<Func<int, double, double, double>>("CHOOSE"),
            RegisteredFunction.Create<Func<int, double, double, double, double>>("CHOOSE"),
            RegisteredFunction.Create<Func<int, double[], double>>("CHOOSE"),
            RegisteredFunction.Create<Func<bool, double, double, double>>("IIF"),

            // full text functions

            RegisteredFunction.Create<Func<string, string, bool>>("CONTAINS"),  // CONTAINS(Name, "Mountain")
            RegisteredFunction.Create<Func<string, string, bool>>("FREETEXT"),  // FREETEXT(Document, "vital safety functions')
            // CONTAINSTABLE and FREETEXTTABLE require a different table name, so a more complex EDMX like setup

            //cast and convert functions
            // TODO: Look at CAST and CONVERT
            //RegisteredFunction.Create<Func<string, string, int, int, string>>("CAST and CONVERT"),
            //RegisteredFunction.Create<Func<string, double, int, int, double>>("CAST and CONVERT"),
            //second and last: data type
            //look at PARSE
            //look at TRY_CAST
            //look at TRY_CONVERT
            //look at TRY_PARSE

            // TODO: Everything else at https://msdn.microsoft.com/en-us/library/ms187813.aspx
            // TODO: Intern-done

            // ### Cast and convert functions - all of these https://docs.microsoft.com/en-us/sql/t-sql/functions/conversion-functions-transact-sql
        };
    }
}
