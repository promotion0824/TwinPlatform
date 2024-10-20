using System.Text;

namespace Willow.AzureDataExplorer.Builders;
public interface IQueryBuilder
{
    string GetQuery();
}

public interface IQuerySelector : IQueryBuilder
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Since we are building a query selector here, the Select method is appropriate.")]
    IQueryWhere Select(string target);
    IQuerySelector Join(string target, string onSourceProperty, string onTargetProperty, string? kind = null);
    IQuerySelector Union();
    IQuerySelector Project(params string[] properties);
    IQuerySelector Extend(string name, string expression);
    IQuerySelector Expand(string name);
    IQuerySelector ProjectKeep(params string[] properties);
    IQueryFilterGroup Summarize();
    IQuerySelector Let(string variableName, string query);
    IQueryFilterGroup Materialize(string query);
}

public interface IQueryWhere : IQueryBuilder
{
    IQueryFilterGroup Where();

    IQueryFilterGroup Where(string filter);

}

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Since we are building a query filter here, the names are appropriate.")]
public interface IQueryFilterGroup : IQueryBuilder
{
    IQueryBuilder OpenGroupParentheses();
    IQueryBuilder CloseGroupParentheses();
    IQueryBuilder Property(string name, string value, bool equals = true);
    IQueryBuilder Contains(string propertyName, string value);
    IQueryFilterGroup PropertyIn(string name, IEnumerable<string> values);
    IQueryBuilder Between(string propertyName, int start, int end);
    IQueryBuilder BetweenDates(string propertyName, DateTimeOffset start, DateTimeOffset end);
    IQueryBuilder OnDate(string propertyName, DateTimeOffset date);
    IQueryBuilder IsNotEmpty(string propertyName);
    IQueryFilterGroup SetProperty(string propertyName);
    IQueryFilterGroup And();
    IQueryFilterGroup Or();
    IQueryFilterGroup Sort(string by, bool desc = false);
    IQueryFilterGroup OrderBy(OrderByParam[] orderByParams);
    IQueryFilterGroup MakeSet(bool multiple = false, params string[] from);
    IQueryFilterGroup TakeAny(bool multiple = false, params string[] from);
    IQueryFilterGroup MakeBag(bool multiple = false, params string[] from);
    IQueryFilterGroup By(params string[] properties);
    IQueryFilterGroup Pack(Dictionary<string, string> values);
    IQueryFilterGroup Compress(string val);
    IQueryFilterGroup CountDistinct(string val);
    IQueryFilterGroup Distinct(string val);
    IQueryFilterGroup Count(string? name = null);
    IQueryFilterGroup GetCount();

}

public class QueryBuilder : IQueryBuilder, IQuerySelector, IQueryWhere, IQueryFilterGroup
{
    private readonly StringBuilder _sb = new StringBuilder();

    public static IQuerySelector Create()
    {
        return new QueryBuilder();
    }

    public IQueryWhere Select(string target)
    {
        _sb.AppendLine(target);
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQuerySelector Join(
        string target,
        string onSourceProperty,
        string onTargetProperty,
        string? kind = null)
    {
        _sb.Append($"| join ");

        if (!string.IsNullOrWhiteSpace(kind))
        {
            _sb.Append($"kind={kind} ");
        }

        _sb.AppendLine($"({target}) on $left.{onSourceProperty} == $right.{onTargetProperty} ");

        return this;
    }

    public IQuerySelector Union()
    {
        _sb.AppendLine("| union");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQuerySelector Project(params string[] properties)
    {
        _sb.AppendLine($"| project {string.Join(',', properties)}");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQuerySelector ProjectKeep(params string[] properties)
    {
        _sb.AppendLine($"| project-keep {string.Join(',', properties)}");
        return this;
    }

    public IQueryFilterGroup Summarize()
    {
        _sb.AppendLine($"| summarize ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQuerySelector Let(string variableName, string query)
    {
        _sb.AppendLine($"let {variableName} = {query};");
        return this;
    }

    public IQueryFilterGroup Where()
    {
        _sb.AppendLine("| where ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup Where(string filter)
    {
        _sb.AppendLine($"| where {filter} ");
        return this;
    }

    public IQueryBuilder OpenGroupParentheses()
    {
        _sb.Append('(');
        return this;
    }

    public IQueryBuilder CloseGroupParentheses()
    {
        _sb.Append(')');
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryBuilder Property(string name, string value, bool equals = true)
    {
        if (equals)
            _sb.Append($"{name} == \"{value?.Trim()}\" ");
        else
            _sb.Append($"{name} != \"{value?.Trim()}\" ");

        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup SetProperty(string propertyName)
    {
        _sb.Append($"{propertyName} = ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryBuilder Contains(string propertyName, string value)
    {
        _sb.Append($"{propertyName} contains \"{value?.Trim()}\" ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup PropertyIn(string name, IEnumerable<string> values)
    {
        _sb.Append($"{name} in ({string.Join(',', values.Select(v => $"\"{v?.Trim()}\""))}) ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryBuilder IsNotEmpty(string propertyName)
    {
        _sb.Append($"isnotempty({propertyName})");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryBuilder Between(string propertyName, int start, int end)
    {
        _sb.Append($"{propertyName} between ({start} .. {end})");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryBuilder BetweenDates(string propertyName, DateTimeOffset start, DateTimeOffset end)
    {
        _sb.Append($"{propertyName} between (datetime(\"{start.ToString()}\") .. datetime(\"{end.ToString()}\"))");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryBuilder OnDate(string propertyName, DateTimeOffset date)
    {
        _sb.Append($"bin(todatetime({propertyName}), 1s) == bin(datetime(\"{date.ToString()}\"), 1s)");
        return this;
    }

    public IQueryFilterGroup And()
    {
        _sb.Append("and ");
        return this;
    }

    public IQueryFilterGroup Or()
    {
        _sb.Append("or ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup Sort(string by, bool desc = false)
    {
        var direction = desc ? "desc" : "asc";
        _sb.AppendLine($"| sort by {by} {direction} ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup OrderBy(params OrderByParam[] orderByParams)
    {
        _sb.AppendLine("| order by");
        for (var i = 0; i < orderByParams.Length; i++)
        {
            var orderBy = orderByParams[i];
            _sb.AppendFormat("{0}{1} {2}", i == 0 ? "" : ", ", orderBy.property, orderBy.order);
        }

        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup MakeSet(bool multiple = false, params string[] from)
    {
        var comma = multiple ? "," : "";
        _sb.Append($"make_set({string.Join(',', from)}){comma} ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup Pack(Dictionary<string, string> values)
    {
        _sb.Append($"pack({string.Join(", ", values.Select(x => $"\"{x.Key}\", {x.Value}"))}) ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup TakeAny(bool multiple = false, params string[] from)
    {
        var comma = multiple ? "," : "";
        _sb.Append($"take_any({string.Join(',', from)}){comma} ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup MakeBag(bool multiple = false, params string[] from)
    {
        var comma = multiple ? "," : "";
        _sb.Append($"make_bag({string.Join(',', from)}){comma} ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup By(params string[] properties)
    {
        _sb.Append($"by {string.Join(',', properties)}");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup Compress(string val)
    {
        _sb.Append($"zlib_compress_to_base64_string({val})");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup CountDistinct(string val)
    {
        _sb.Append($"dcount({val})");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup Distinct(string val)
    {
        _sb.Append($"| distinct {val}");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup Count(string? name = null)
    {
        _sb.Append($"{(name != null ? $"{name} = " : string.Empty)}count() ");
        return this;
    }

    public IQueryFilterGroup GetCount()
    {
        _sb.Append($"| count ");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQueryFilterGroup Materialize(string query)
    {
        _sb.Append($"materialize({query})");
        return this;
    }

    public string GetQuery()
    {
        return _sb.ToString();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQuerySelector Extend(string name, string expression)
    {
        _sb.Append($"| extend {name} = {expression}");
        return this;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Globalization to be addressed in the future if needed.")]
    public IQuerySelector Expand(string name)
    {
        _sb.Append($"| mv-expand {name}");
        return this;
    }
}

public record OrderByParam(string property, Order order);

public enum Order
{
    asc,
    desc
}

