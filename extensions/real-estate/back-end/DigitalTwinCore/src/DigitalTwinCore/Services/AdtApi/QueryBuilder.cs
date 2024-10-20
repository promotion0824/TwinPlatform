// These are copied from the ADT API /tree endpoint and will be removed once we switch to single-tenant.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System;

namespace DigitalTwinCore.Services.AdtApi;

public interface IQueryBuilder
{

}

public interface IQuerySelector : IQueryBuilder
{
    IQueryFrom SelectSingle();
    IQueryFrom SelectAll();
    IQueryFrom SelectTop(int top);
    IQueryFrom SelectCount();
    IQueryFrom Select(params string[] entities);
}

public interface IQueryFrom : IQueryBuilder
{
    IQueryWhere FromDigitalTwins(string alias = "");
    IQueryWhere FromRelationships(string alias = "");
}

public interface IQueryWhere : IQueryBuilder
{
    IQueryWhere Match(string[] relationships, string source = "", string target = "", string hops = "", string sourceDirection = "-", string targetDirection = "->");
    IQueryWhere Match(params MatchExpression[] matches);
    IQueryFilterGroup Where();
    string GetQuery();
    IQueryWhere JoinRelated(string targetAlias, string sourceAlias, string relationshipName);
}

public interface IQueryFilterGroup : IQueryBuilder
{
    IQueryFilterGroup And();
    IQueryFilterGroup Or();
    IQueryFilterGroup Not();
    IQueryFilterGroup OpenGroupParenthesis();
    IQueryFilterGroup CloseGroupParenthesis();
    IQueryFilterGroup CheckDefined(List<string> properties);
    IQueryFilterGroup IsDefined(string property);
    IQueryFilterGroup WithStringProperty(string name, string value);
    IQueryFilterGroup WithIntProperty(string name, int value);
    IQueryFilterGroup WithBoolProperty(string name, bool value);
    IQueryFilterGroup WithPropertyIn(string name, IEnumerable<string> values, int maxItemsPerQuery = 100);
    IQueryFilterGroup WithAnyModel(IEnumerable<string> models, string alias = "", bool exact = false);
    IQueryFilterGroup Contains(string name, string value);
    IQueryFilterGroup BetweenDates(string name, DateTimeOffset startTime, DateTimeOffset endTime);
    string GetQuery();
}

public class QueryBuilder : IQuerySelector, IQueryFrom, IQueryWhere, IQueryFilterGroup
{
    private readonly StringBuilder queryBuilder;
    private bool _whereAdded;

    public const string FieldTwinId = "$dtId";
    public const string FieldRelationshipSourceId = "$sourceId";
    public const string FieldRelationshipTargetId = "$targetId";
    public const string FieldRelationshipId = "$relationshipId";

    private QueryBuilder()
    {
        queryBuilder = new StringBuilder();
    }

    public static IQuerySelector Create()
    {
        return new QueryBuilder();
    }

    public IQueryFilterGroup And()
    {
        queryBuilder.Append("AND ");
        return this;
    }

    public IQueryFilterGroup Not()
    {
        queryBuilder.Append("NOT ");
        return this;
    }

    public IQueryFilterGroup CheckDefined(List<string> properties)
    {
        queryBuilder.Append(string.Join(" AND ", properties.Select(x => $"IS_DEFINED({x}) ")));
        return this;
    }

    public IQueryFilterGroup IsDefined(string property)
    {
        queryBuilder.Append($"IS_DEFINED({property}) ");
        return this;
    }

    public IQueryFilterGroup CloseGroupParenthesis()
    {
        queryBuilder.Append(") ");
        return this;
    }

    public IQueryWhere FromDigitalTwins(string alias = "")
    {
        queryBuilder.Append("from DIGITALTWINS ");
        if (!string.IsNullOrEmpty(alias))
            queryBuilder.Append($"{alias} ");
        return this;
    }

    public IQueryWhere FromRelationships(string alias = "")
    {
        queryBuilder.Append("from RELATIONSHIPS ");
        if (!string.IsNullOrEmpty(alias))
            queryBuilder.Append($"{alias} ");
        return this;
    }

    public string GetQuery()
    {
        return queryBuilder.ToString();
    }

    public IQueryWhere Match(string[] relationships, string source = "", string target = "", string hops = "", string sourceDirection = "-", string targetDirection = "->")
    {
        var relationshipNamePrefix = relationships.Any() ? ":" : string.Empty;
        queryBuilder.Append($"match ({source}){sourceDirection}[{relationshipNamePrefix}{string.Join('|', relationships)}{hops}]{targetDirection}({target}) ");
        return this;
    }

    public IQueryWhere Match(params MatchExpression[] matches)
    {
        queryBuilder.Append("match ");
        foreach (var match in matches)
        {
            queryBuilder.Append($"({match.Entity})");
            if (match.Relationships?.Any() == true)
            {
                queryBuilder.Append($"-[:{string.Join('|', match.Relationships)}{match.Hops}]-");
            }
        }
        return this;
    }

    public IQueryFilterGroup OpenGroupParenthesis()
    {
        queryBuilder.Append("( ");
        return this;
    }

    public IQueryFilterGroup Or()
    {
        queryBuilder.Append("OR ");
        return this;
    }

    public IQueryFrom SelectAll()
    {
        queryBuilder.Append("SELECT * ");
        return this;
    }

    public IQueryFrom SelectCount()
    {
        queryBuilder.Append("SELECT count() ");
        return this;
    }

    public IQueryFrom SelectSingle()
    {
        queryBuilder.Append("SELECT top(1) ");
        return this;
    }

    public IQueryFrom SelectTop(int top)
    {
        queryBuilder.AppendFormat("SELECT top({0}) ", top);
        return this;
    }

    public IQueryFrom Select(params string[] entities)
    {
        queryBuilder.AppendFormat($"select {string.Join(',', entities)} ");
        return this;
    }

    public IQueryFilterGroup Where()
    {
        if (!_whereAdded)
        {
            queryBuilder.Append("where ");
            _whereAdded = true;
        }
        return this;
    }

    public IQueryFilterGroup WithAnyModel(IEnumerable<string> models, string alias = "", bool exact = false)
    {
        var format = $"IS_OF_MODEL({(!string.IsNullOrEmpty(alias) ? $"{alias}, " : string.Empty)}'{{0}}'{(exact ? ", exact" : string.Empty)})";
        queryBuilder.Append($"({string.Join(" OR ", models.Select(x => string.Format(format, x)))}) ");
        return this;
    }

    public IQueryFilterGroup WithBoolProperty(string name, bool value)
    {
        queryBuilder.Append($"{name} = {(value ? "true" : "false")}");
        return this;
    }

    public IQueryFilterGroup WithIntProperty(string name, int value)
    {
        queryBuilder.Append($"{name} = {value}");
        return this;
    }

    public IQueryFilterGroup WithPropertyIn(string name, IEnumerable<string> values, int maxItemsPerQuery = 100)
    {
        var statements = values.Chunk(maxItemsPerQuery).Select(x => {
            if (x.Length == 1)
                return $"{name} = '{SafeParameter(x.Single())}'";

            return $"{name} IN [{string.Join(',', x.Select(v => $"'{SafeParameter(v)}'"))}]";
        });
        queryBuilder.Append($"({string.Join(" OR ", statements)}) ");
        return this;
    }

    public IQueryFilterGroup WithStringProperty(string name, string value)
    {
        queryBuilder.Append($"{name} = '{SafeParameter(value)}' ");
        return this;
    }

    public IQueryFilterGroup Contains(string name, string value)
    {
        queryBuilder.Append($"contains({name}, '{SafeParameter(value)}') ");
        return this;
    }

    public IQueryWhere JoinRelated(string targetAlias, string sourceAlias, string relationshipName)
    {
        queryBuilder.Append($"JOIN {targetAlias} RELATED {sourceAlias}.{relationshipName} ");
        return this;
    }

    private string SafeParameter(string parameter)
    {
        return parameter.Replace("'", "\\'");
    }

    public IQueryFilterGroup BetweenDates(string name, DateTimeOffset startTime, DateTimeOffset endTime)
    {
        var start = startTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
        var end = endTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);

        queryBuilder.Append($"{name} >= '{start}' and {name} <= '{end}'");
        return this;
    }
}

public class MatchExpression
{
    public MatchExpression(
        string entity,
        string[] relationships = null,
        string hops = "")
    {
        Entity = entity;
        Relationships = relationships;
        Hops = hops;
    }

    public string Entity { get; set; }
    public string[] Relationships { get; set; }
    public string Hops { get; set; }
}
