namespace Willow.DataAccess.SqlServer.DbUp.TypeHandlers;

using System.Data;
using Dapper;

/// <summary>
/// A type handler for Enums.
/// </summary>
public class EnumHandler : SqlMapper.ITypeHandler
{
    /// <summary>
    /// Parses the value from the database.
    /// </summary>
    /// <param name="destinationType">The destination enum type.</param>
    /// <param name="value">The incoming value from the database.</param>
    /// <returns>The enum value.</returns>
    public object? Parse(Type destinationType, object value)
    {
        if (destinationType.IsEnum)
        {
            return Enum.Parse(destinationType, (string)value);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the value of the parameter.
    /// </summary>
    /// <param name="parameter">A database parameter to set.</param>
    /// <param name="value">The value to set the parameter to.</param>
    public void SetValue(IDbDataParameter parameter, object value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = (string)((dynamic)value);
    }
}
