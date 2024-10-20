using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Willow.AzureDigitalTwins.Services.Builders;

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
    IQueryFilterGroup Where(string filter);
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

    public IQueryFilterGroup Where(string filter)
    {
        if (string.IsNullOrEmpty(filter))
            return this;

        if (!_whereAdded)
        {
            queryBuilder.Append("where ");
            queryBuilder.Append($" {filter}");
            _whereAdded = true;
        }
        else
        {
            queryBuilder.Append($" and {filter}");
        }
        return this;
    }

    public IQueryFilterGroup WithAnyModel(IEnumerable<string> models, string alias = "", bool exact = false)
    {
        var format = $"IS_OF_MODEL({(!string.IsNullOrEmpty(alias) ? $"{alias}, " : string.Empty)}'{{0}}'{(exact ? ", exact" : string.Empty)})";
        queryBuilder.Append($"({string.Join(" OR ", models.Select(x => string.Format(format, x?.Trim())))}) ");
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
        var statements = values.Chunk(maxItemsPerQuery).Select(x =>
        {
            if (x.Length == 1)
                return $"{name} = '{SafeParameter(x.Single())}'";

            return $"{name} IN [{string.Join(',', x.Select(v => $"'{SafeParameter(v.Trim())}'"))}]";
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

    public static IQueryBuilder BuildTwinsQuery(
        IQueryBuilder query,
        IEnumerable<string>? twinIds = null,
        IEnumerable<string>? modelIds = null,
        string? locationId = null,
        string[]? relationshipToTraverse = null,
        string? searchString = null,
        bool modelExactMatch = false,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        string? queryFilter = null,
        bool isCountQuery = false)
    {
        const string twinCollectionName = "twins";
        const string maxNumberOfHopsPattern = "*..6";
        bool includeAnd = false;
        const string sourceName = twinCollectionName;
        const string targetName = "location";
        var hasLocation = !string.IsNullOrEmpty(locationId);

        searchString = searchString?.Trim();
        queryFilter = queryFilter?.Trim();
        locationId = locationId?.Trim();

        if (hasLocation)
        {
            var spaceCollectionModels = new List<string>() { "dtmi:com:willowinc:Space;1", "dtmi:com:willowinc:Collection;1" };
            if ((relationshipToTraverse?.Length ?? 0) == 0)
            {
                relationshipToTraverse = new[] { "isPartOf", "locatedIn", "hostedBy", "isCapabilityOf" };
            }
            if (!isCountQuery)
                query = Create().Select(twinCollectionName).FromDigitalTwins();

            ((IQueryWhere)query).Match(
            relationshipToTraverse,
            sourceName, targetName,
            maxNumberOfHopsPattern, "-", "->");

            ((IQueryWhere)query).Where().WithStringProperty($"{targetName}.$dtId", locationId);
            ((IQueryFilterGroup)query).And();
            if (modelIds?.Count() > 0)
            {
                ((IQueryWhere)query).Where().WithAnyModel(
                modelIds,
                sourceName,
                exact: modelExactMatch);
                ((IQueryFilterGroup)query).And();
            }
        ((IQueryWhere)query).Where().WithAnyModel(
        spaceCollectionModels,
        targetName,
        exact: modelExactMatch);

            includeAnd = true;
        }

        if (twinIds?.Any() == true)
        {
            ((IQueryWhere)query).Where().WithPropertyIn(FieldTwinId, twinIds);
            includeAnd = true;
        }

        if (modelIds?.Any() == true)
        {
            if (includeAnd) ((IQueryFilterGroup)query).And();
            ((IQueryWhere)query).Where().WithAnyModel(
                                                                        modelIds,
                                                                        hasLocation ? sourceName : string.Empty,
                                                                        exact: modelExactMatch);
            includeAnd = true;
        }
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            if (includeAnd) ((IQueryFilterGroup)query).And();
            if (!hasLocation) SearchQueryBuilder(searchString, query);
            else SearchQueryBuilder(searchString, query, sourceName);
        }
        if (startTime.HasValue && endTime.HasValue)
        {
            var timeSearchString = "$metadata.$lastUpdateTime";

            if (startTime > endTime) throw new InvalidDataException("StartTime is greater than EndTime");
            if (hasLocation) timeSearchString = $"{targetName}.$metadata.$lastUpdateTime";


            if (includeAnd)
                ((IQueryFilterGroup)query).And();
            else
                ((IQueryWhere)query).Where();

            ((IQueryFilterGroup)query).BetweenDates(timeSearchString, startTime.Value, endTime.Value);
        }
        if (queryFilter is not null)
            ((IQueryWhere)query).Where(queryFilter);

        return query;
    }

    private static void SearchQueryBuilder(string searchString, IQueryBuilder query, string? searchdestination = null)
    {
        Regex regex = new(@"^[a-zA-Z0-9\s-_.]*$", RegexOptions.Compiled);
        if (!regex.IsMatch(searchString))
            throw new ArgumentException(@"Search string contains unsupported character(s). Supported string is alphanumeric with space or -");
        //As ADT search is case sensitive, adding title case check
        TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
        string titleCaseString = myTI.ToTitleCase(searchString);
        string searchStringLower = searchString.ToLower();
        string searchStringUpper = searchString.ToUpper();
        var dtIdString = "$dtId";
        var nameString = "name";
        if (searchdestination != null)
        {
            dtIdString = string.Concat(searchdestination, ".", dtIdString);
            nameString = string.Concat(searchdestination, ".", nameString);
        }

        ((IQueryWhere)query).Where()
            .OpenGroupParenthesis()
                .Contains(dtIdString, searchString)
            .Or()
                .Contains(nameString, searchString)
            .Or()
                .Contains(dtIdString, titleCaseString)
            .Or()
                .Contains(nameString, titleCaseString)
            .Or()
                .Contains(dtIdString, searchStringLower)
            .Or()
                .Contains(nameString, searchStringLower)
            .Or()
                .Contains(dtIdString, searchStringUpper)
            .Or()
                .Contains(nameString, searchStringUpper)
            .CloseGroupParenthesis();
    }
}

public class MatchExpression
{
    public MatchExpression(
        string entity,
        string[]? relationships = null,
        string hops = "")
    {
        Entity = entity;
        Relationships = relationships;
        Hops = hops;
    }

    public string Entity { get; set; }
    public string[]? Relationships { get; set; }
    public string Hops { get; set; }
}
