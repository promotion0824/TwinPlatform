namespace Willow.LiveData.Core.Features.Telemetry.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal interface IAdxQueryBuilder
{
    string GetQuery();
}

internal interface IAdxQuerySelector : IAdxQueryBuilder
{
    IAdxQueryWhere Select(string target);

    IAdxQuerySelector Project(params string[] properties);

    IAdxQueryFilterGroup Summarize();

    IAdxQuerySelector Let(string variableName, string query);
}

internal interface IAdxQueryWhere : IAdxQueryBuilder
{
    IAdxQueryFilterGroup Where();
}

internal interface IAdxQueryFilterGroup : IAdxQueryBuilder
{
    IAdxQueryFilterGroup OpenGroupParentheses();

    IAdxQueryFilterGroup CloseGroupParentheses();

    IAdxQueryFilterGroup Comma();

    IAdxQueryFilterGroup PropertyEquals(string name, string value);

    IAdxQueryFilterGroup Contains(string propertyName, string value);

    IAdxQueryFilterGroup PropertyIn(string propertyName, IEnumerable<string> values);

    IAdxQueryFilterGroup PropertyNotIn(string propertyName, IEnumerable<string> values);

    IAdxQueryFilterGroup DateTimeBetween(string propertyName, DateTime start, DateTime end);

    IAdxQueryFilterGroup IsNotEmpty(string propertyName);

    IAdxQueryFilterGroup IsEmpty(string propertyName);

    IAdxQueryFilterGroup And();

    IAdxQueryFilterGroup Or();

    IAdxQueryFilterGroup Order(string by, bool desc = false);

    IAdxQueryFilterGroup By(params string[] properties);

    IAdxQueryFilterGroup Take(int val);

    IAdxQueryFilterGroup Average(string fieldName);

    IAdxQueryFilterGroup Minimum(string fieldName);

    IAdxQueryFilterGroup Maximum(string fieldName);

    IAdxQueryFilterGroup State(string fieldName, string fieldValueName);

    IAdxQueryFilterGroup StateCount(string fieldName);

    IAdxQueryFilterGroup OnCount(string fieldName);

    IAdxQueryFilterGroup OffCount(string fieldName);

    IAdxQueryFilterGroup Sum(string fieldName);

    IAdxQueryFilterGroup Bin(string fieldName, string roundTo);

    IAdxQueryFilterGroup AddFields(params string[] fields);

    IAdxQueryFilterGroup ArgMax(string inputExpr, string outputExpr);
}

internal class AdxQueryBuilder : IAdxQueryBuilder, IAdxQuerySelector, IAdxQueryWhere, IAdxQueryFilterGroup
{
    private readonly StringBuilder sb = new StringBuilder();

    public static IAdxQuerySelector Create()
    {
        return new AdxQueryBuilder();
    }

    /// <inheritdoc/>
    public IAdxQueryWhere Select(string target)
    {
        sb.AppendLine(target);
        return this;
    }

    /// <inheritdoc/>
    public IAdxQuerySelector Project(params string[] properties)
    {
        sb.AppendLine($"| project {string.Join(',', properties)}");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Summarize()
    {
        sb.AppendLine("| summarize ");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQuerySelector Let(string variableName, string query)
    {
        sb.AppendLine($"let {variableName} = {query};");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Where()
    {
        sb.AppendLine("| where ");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup OpenGroupParentheses()
    {
        sb.Append('(');
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup CloseGroupParentheses()
    {
        sb.Append(')');
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Comma()
    {
        sb.Append(", ");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup PropertyEquals(string propertyName, string value)
    {
        sb.Append($"{propertyName} == \"{value}\" ");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Contains(string propertyName, string value)
    {
        sb.Append($"{propertyName} contains \"{value}\" ");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup PropertyIn(string propertyName, IEnumerable<string> values)
    {
        var valuesList = values.ToList();
        if (!valuesList.Any())
        {
            sb.Append("false ");
        }
        else
        {
            sb.Append($"{propertyName} in ({string.Join(',', valuesList.Select(v => $"\"{v}\""))})");
        }

        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup PropertyNotIn(string propertyName, IEnumerable<string> values)
    {
        var valuesList = values.ToList();
        if (!valuesList.Any())
        {
            sb.Append("true ");
        }
        else
        {
            sb.Append($"{propertyName} !in ({string.Join(',', valuesList.Select(v => $"\"{v}\""))})");
        }

        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup IsNotEmpty(string propertyName)
    {
        sb.Append($"isnotempty({propertyName})");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup IsEmpty(string propertyName)
    {
        sb.Append($"isempty({propertyName})");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup DateTimeBetween(string propertyName, DateTime start, DateTime end)
    {
        sb.Append($"{propertyName} between (datetime({start:s}) .. datetime({end:s}))");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup And()
    {
        sb.Append(" and ");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Or()
    {
        sb.Append(" or ");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Order(string by, bool desc = false)
    {
        var direction = desc ? "desc" : "asc";
        sb.AppendLine($"| sort by {by} {direction} ");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup By(params string[] properties)
    {
        sb.Append($"by {string.Join(',', properties)}");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Take(int val)
    {
        sb.AppendLine($"| take {val} ");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Average(string fieldName)
    {
        sb.Append($"Average = avg(todouble({fieldName}))");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Minimum(string fieldName)
    {
        sb.Append($"Minimum = min(todouble({fieldName}))");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Maximum(string fieldName)
    {
        sb.Append($"Maximum = max(todouble({fieldName}))");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup State(string fieldName, string fieldValueName)
    {
        sb.Append($"State = make_bag(bag_pack(tostring({fieldName}), {fieldValueName}))");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup StateCount(string fieldName)
    {
        sb.Append($"StateCountValue = count() by toint({fieldName})");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup OnCount(string fieldName)
    {
        sb.Append($"OnCount = countif(tobool({fieldName}) == true)");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup OffCount(string fieldName)
    {
        sb.Append($"OffCount = countif(tobool({fieldName}) == false)");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Sum(string fieldName)
    {
        sb.Append($"Sum = sum(todouble({fieldName}))");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup Bin(string fieldName, string roundTo)
    {
        sb.Append($" bin({fieldName}, {roundTo}) ");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup AddFields(params string[] fields)
    {
        sb.Append($"{string.Join(", ", fields)} ");
        return this;
    }

    /// <inheritdoc/>
    public IAdxQueryFilterGroup ArgMax(string inputExpr, string outputExpr)
    {
        sb.Append($"arg_max({inputExpr}, {outputExpr}) ");
        return this;
    }

    /// <inheritdoc/>
    public string GetQuery()
    {
        return sb.ToString();
    }
}
