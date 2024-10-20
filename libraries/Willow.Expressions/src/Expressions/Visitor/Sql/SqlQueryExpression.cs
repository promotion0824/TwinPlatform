using System.Linq;

namespace Willow.Expressions.Visitor.Sql
{
    /// <summary>
    /// A Sql query created by parsing natural language with respect to a metamodel
    /// </summary>
    public class SqlQueryExpression
    {
        /// <summary>
        /// The raw SQL command
        /// </summary>
        public string QueryString { get; }

        /// <summary>
        /// The parameters
        /// </summary>
        public SqlParameter[] Parameters { get; }

        /// <summary>
        /// Optional order by clause
        /// </summary>
        public string OrderByClause { get; set; } = "";

        /// <summary>
        /// How many records to skip
        /// </summary>
        public long Skip { get; set; }

        /// <summary>
        /// How many records to take
        /// </summary>
        public long Take { get; set; }

        /// <summary>
        /// An Empty SqlQueryExpression
        /// </summary>
        public static readonly SqlQueryExpression Empty = new SqlQueryExpression("", new SqlParameter[0]);

        /// <summary>
        /// Creates a new instance of the <see cref="SqlQueryExpression"/> class
        /// </summary>
        public SqlQueryExpression(string queryString, SqlParameter[] parameters)
        {
            this.QueryString = queryString;
            this.Parameters = parameters;
        }

        /// <summary>
        /// Creates a new SqlQueryExpression from a combined query string and two sets of parameters
        /// </summary>
        public SqlQueryExpression(string combinedQueryString, SqlParameter[] parameters1, SqlParameter[] parameters2)
        {
            this.QueryString = combinedQueryString;
            this.Parameters = parameters1.Concat(parameters2).ToArray();
        }

        /// <summary>
        /// Implicit conversion from string to make it easier for commands without parameters
        /// </summary>
        public static implicit operator SqlQueryExpression(string sqlQueryText)
        {
            return new SqlQueryExpression(sqlQueryText, new SqlParameter[0]);
        }

        /// <summary>
        /// Concatenate two SqlQueryExpressions
        /// </summary>
        public static SqlQueryExpression operator +(string queryText, SqlQueryExpression query)
        {
            return new SqlQueryExpression(queryText + query.QueryString, query.Parameters);
        }

        /// <summary>
        /// Concatenate two SqlQueryExpressions
        /// </summary>
        public static SqlQueryExpression operator +(SqlQueryExpression query, string queryText)
        {
            return new SqlQueryExpression(query.QueryString + queryText, query.Parameters);
        }

        /// <summary>
        /// Concatenate two SqlQueryExpressions
        /// </summary>
        public static SqlQueryExpression operator +(SqlQueryExpression query1, SqlQueryExpression query2)
        {
            return new SqlQueryExpression(query1.QueryString + " " + query2.QueryString, query1.Parameters, query2.Parameters);
        }

        /// <summary>
        /// Prepend a string to the query expression text
        /// </summary>
        public SqlQueryExpression Prepend(string s)
        {
            return new SqlQueryExpression(s + this.QueryString, this.Parameters);
        }

        /// <summary>
        /// Append a string to the query expression text
        /// </summary>
        public SqlQueryExpression Append(string s)
        {
            return new SqlQueryExpression(this.QueryString + s, this.Parameters);
        }

        /// <summary>
        /// ToString
        /// </summary>
        public override string ToString()
        {
            var queryString = this.QueryString == "1=1" ? "" : this.QueryString;

            if (!string.IsNullOrEmpty(this.OrderByClause))
                queryString = (queryString + " " + this.OrderByClause).TrimStart();

            if (this.Skip > 0)
            {
                queryString = (queryString + $" skip {this.Skip}").TrimStart();
            }

            if (this.Take > 0)
            {
                queryString = (queryString + $" take {this.Take}").TrimStart();
            }

            var parameterstring = string.Join(", ",
                this.Parameters.Select(p => p.ParameterValue is string ?
                    $"{p.ParameterName}=\"{p.ParameterValue}\"" :
                    $"{p.ParameterName}={p.ParameterValue}"));

            if (!string.IsNullOrWhiteSpace(parameterstring))
                parameterstring = " with parameters (" + parameterstring + ")";

            return queryString + parameterstring;
        }
    }
}
