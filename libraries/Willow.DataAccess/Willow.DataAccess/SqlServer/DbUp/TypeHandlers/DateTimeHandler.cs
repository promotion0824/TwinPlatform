namespace Willow.DataAccess.SqlServer.DbUp.TypeHandlers;

using System.Data;
using Dapper;

/// <summary>
/// A type handler for DateTime.
/// </summary>
public class DateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    /// <summary>
    /// Sets the value of the parameter.
    /// </summary>
    /// <param name="parameter">The database parameter to set.</param>
    /// <param name="value">A date-time value to set in the parameter.</param>
    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    /// <summary>
    /// Parses the value from the database.
    /// </summary>
    /// <param name="value">A value stored in the database of type datetime that is stored as an object.</param>
    /// <returns>The datetime value.</returns>
    public override DateTime Parse(object value)
    {
        return DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
    }
}
