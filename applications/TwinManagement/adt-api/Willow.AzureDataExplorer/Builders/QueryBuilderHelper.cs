using Willow.Model.Adt;
using Willow.Model.Requests;

namespace Willow.AzureDataExplorer.Builders;

public static class QueryBuilderHelper
{
    public static IQueryWhere Create(string functionOrTableName)
    {
        return QueryBuilder.Create().Select(functionOrTableName);
    }
    public static void AppendModelsFilter(string[] modelIds, bool exactModelMatch, IQueryWhere query,
        IList<string> descendantIds)
    {
        if (descendantIds.Any() && !exactModelMatch)
            query.Where().PropertyIn(AdxConstants.ModelIdColumnName, descendantIds);
        else
        {
            query.Where().PropertyIn(AdxConstants.ModelIdColumnName, modelIds);
        }
    }

    public static void AppendSearchStringFilter(string searchString, IQueryWhere query)
    {
        if (string.IsNullOrWhiteSpace(searchString))
            return;

        var qfg = query.Where().OpenGroupParentheses();
        ((IQueryFilterGroup)qfg).Contains(AdxConstants.twinIdColumnName, searchString);
        ((IQueryFilterGroup)qfg).Or();
        ((IQueryFilterGroup)qfg).Contains("Name", searchString);
        ((IQueryFilterGroup)qfg).CloseGroupParentheses();
    }

    public static void AppendIdFilter(string id, IQueryWhere query)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        var qfg = query.Where().OpenGroupParentheses();
        ((IQueryFilterGroup)qfg).Property(AdxConstants.twinIdColumnName, id);
        ((IQueryFilterGroup)qfg).CloseGroupParentheses();
    }

    public static void AppendMultipleIdFilter(IQueryWhere query, params string[] ids)
    {
        if (ids is null || ids.Length == 0)
            return;

        var qfg = query.Where().OpenGroupParentheses();
        ((IQueryFilterGroup)qfg).PropertyIn(AdxConstants.twinIdColumnName, ids);
        ((IQueryFilterGroup)qfg).CloseGroupParentheses();
    }

    //Sample query for Location column search:
    //ActiveTwins | where ModelId in ("dtmi:com:willowinc:VAVBox;1","dtmi:com:willowinc:AutomaticTransferSwitch;1","dtmi:com:willowinc:FanCoilUnitReheat;1") | where 
    //Location['SiteId'] == "4e5fc229-ffd9-462a-882b-16b4a63b2a8a" or Location['SiteDtId'] == "4e5fc229-ffd9-462a-882b-16b4a63b2a8a" or
    //Location['SiteName'] == "4e5fc229-ffd9-462a-882b-16b4a63b2a8a" or Location['FloorId'] == "4e5fc229-ffd9-462a-882b-16b4a63b2a8a" or
    //Location['FloorDtId'] == "4e5fc229-ffd9-462a-882b-16b4a63b2a8a" or Location['FloorName'] == "4e5fc229-ffd9-462a-882b-16b4a63b2a8a"

    public static void AppendLocationFilter(string locationId, IQueryWhere query, IEnumerable<ExportColumn> locationColumns)
    {
        if (string.IsNullOrEmpty(locationId) || !locationColumns.Any())
            return;

        query.Where();
        int index = 0;
        foreach (var column in locationColumns)
        {
            if (column.SourceType == CustomColumnSource.Complex)
            {
                // we loop through all the children columns and add them to the query

                // There doesn't appear to be a way to directly say something like "id in parse(locations).Values",
                //  so we generate: P[k1]=id OR P[k2]=id ...
                // Another way would be to just say: string(location) CONTAINS "id"   (must incl. quotes to avoid substring matches)
                foreach (var child in column.Children)
                {
                    if (index > 0)
                        ((IQueryFilterGroup)query).Or();
                    ((IQueryFilterGroup)query).Property($"{column.Name}['{child.Name}']", locationId);
                    index++;
                }
            }
            else
            {
                if (index > 0)
                    ((IQueryFilterGroup)query).Or();
                ((IQueryFilterGroup)query).Property(column.Name, locationId);
                index++;
            }
        }
    }

    public static void AppendTimeFilter(DateTimeOffset? startTime, DateTimeOffset? endTime, string ingestionColumnName, IQueryWhere query)
    {
        if (!startTime.HasValue && !endTime.HasValue) return;
        DateTimeOffset start = startTime ?? DateTimeOffset.MinValue;
        DateTimeOffset end = endTime ?? DateTimeOffset.MaxValue;

        if (start > end) throw new InvalidDataException("StartTime is greater than EndTime");

        query.Where();
        ((IQueryFilterGroup)query).BetweenDates(ingestionColumnName, start, end);
    }

    public static void AppendRelationships(bool includeRelationships, bool includeIncomingRelationships, ExportColumn relationshipEntityColumn, IQueryWhere query)
    {
        if (!includeIncomingRelationships && !includeRelationships)
            return;

        if (includeRelationships)
        {
            ((IQuerySelector)query).Join(
                QueryBuilder.Create().Select(AdxConstants.RelationshipsFunctionName).GetQuery(),
                AdxConstants.twinIdColumnName,
                AdxConstants.SourceIdColumnName,
                "leftouter");
        }

        if (includeIncomingRelationships)
        {
            ((IQuerySelector)query).Join(
                QueryBuilder.Create().Select(AdxConstants.RelationshipsFunctionName).GetQuery(),
                AdxConstants.twinIdColumnName,
                AdxConstants.TargetIdColumnName,
                "leftouter");
        }

        ((IQuerySelector)query).Summarize().TakeAny(true, AdxConstants.twinColumnAlias, AdxConstants.exportTimeColumnAlias, AdxConstants.locationColumnAlias);

        if (includeRelationships)
            ((IQueryFilterGroup)query).SetProperty(AdxConstants.outgoingRelationshipsColumnAlias).MakeSet(includeIncomingRelationships, relationshipEntityColumn.Name);
        if (includeIncomingRelationships)
            ((IQueryFilterGroup)query).SetProperty(AdxConstants.incomingRelationshipsColumnAlias).MakeSet(false, $"{relationshipEntityColumn.Name}{(includeRelationships ? "1" : string.Empty)}");

        ((IQueryFilterGroup)query).By(AdxConstants.twinIdColumnName);
    }

    public static void AppendQueryFilter(string? filter, IQueryWhere query)
    {
        if (string.IsNullOrEmpty(filter))
            return;

        (query as IQueryWhere).Where(filter);
    }
}
