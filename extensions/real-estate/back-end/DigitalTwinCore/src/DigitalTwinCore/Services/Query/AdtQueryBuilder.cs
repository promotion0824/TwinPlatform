using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Services.Adx;

namespace DigitalTwinCore.Services.Query
{
	public interface IAdtQueryBuilder
    {
        
    }

    public interface IAdtQuerySelector : IAdtQueryBuilder
    {
        IAdtQueryFrom SelectSingle();
        IAdtQueryFrom SelectAll();
        IAdtQueryFrom SelectTop(int top);
        IAdtQueryFrom SelectCount();
        IAdtQueryFrom Select(params string[] entities);
    }

    public interface IAdtQueryFrom : IAdtQueryBuilder
    {
        IAdtQueryWhere FromDigitalTwins(string alias = "");
        IAdtQueryWhere FromRelationships(string alias = "");
    }

    public interface IAdtQueryWhere : IAdtQueryBuilder
    {
        IAdtQueryWhere Match(string[] relationships, string source = "", string target = "", string hops = "", string sourceDirection = "-", string targetDirection = "->");
        IAdtQueryWhere Match(params MatchExpression[] matches);
        IAdtQueryFilterGroup Where();
        string GetQuery();
        IAdtQueryWhere JoinRelated(string targetAlias, string sourceAlias, string relationshipName);
    }

    public interface IAdtQueryFilterGroup : IAdtQueryBuilder
    {
        IAdtQueryFilterGroup And();
        IAdtQueryFilterGroup Or();
        IAdtQueryFilterGroup Not();
        IAdtQueryFilterGroup OpenGroupParenthesis();
        IAdtQueryFilterGroup CloseGroupParenthesis();
        IAdtQueryFilterGroup CheckDefined(List<string> properties);
        IAdtQueryFilterGroup IsDefined(string property);
        IAdtQueryFilterGroup WithStringProperty(string name, string value);
        IAdtQueryFilterGroup WithIntProperty(string name, int value);
        IAdtQueryFilterGroup WithBoolProperty(string name, bool value);
        IAdtQueryFilterGroup WithPropertyIn(string name, IEnumerable<string> values);
        IAdtQueryFilterGroup WithAnyModel(IEnumerable<string> models, string alias = "");
        IAdtQueryFilterGroup Contains(string name, string value);
        string GetQuery();
    }

    public class AdtQueryBuilder : IAdtQuerySelector, IAdtQueryFrom, IAdtQueryWhere, IAdtQueryFilterGroup
    {
        private readonly StringBuilder queryBuilder;

        private AdtQueryBuilder()
        {
            queryBuilder = new StringBuilder();
        }

        public static IAdtQuerySelector Create()
        {
            return new AdtQueryBuilder();
        }

        public IAdtQueryFilterGroup And()
        {
            queryBuilder.Append("AND ");
            return this;
        }

        public IAdtQueryFilterGroup Not()
        {
            queryBuilder.Append("NOT ");
            return this;
        }

        public IAdtQueryFilterGroup CheckDefined(List<string> properties)
        {
            queryBuilder.Append(string.Join(" AND ", properties.Select(x => $"IS_DEFINED({x}) ")));
            return this;
        }

        public IAdtQueryFilterGroup IsDefined(string property)
        {
            queryBuilder.Append($"IS_DEFINED({property}) ");
            return this;
        }

        public IAdtQueryFilterGroup CloseGroupParenthesis()
        {
            queryBuilder.Append(") ");
            return this;
        }

        public IAdtQueryWhere FromDigitalTwins(string alias = "")
        {
            queryBuilder.Append("from DIGITALTWINS ");
            if (!string.IsNullOrEmpty(alias))
                queryBuilder.Append($"{alias} ");
            return this;
        }

        public IAdtQueryWhere FromRelationships(string alias = "")
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

        public IAdtQueryWhere Match(string[] relationships, string source = "", string target = "", string hops = "", string sourceDirection = "-", string targetDirection = "->")
        {
            var relationshipNamePrefix = relationships.Any() ? ":" : string.Empty;
            queryBuilder.Append($"match ({source}){sourceDirection}[{relationshipNamePrefix}{string.Join('|', relationships)}{hops}]{targetDirection}({target}) ");
            return this;
        }

        public IAdtQueryWhere Match(params MatchExpression[] matches)
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

        public IAdtQueryFilterGroup OpenGroupParenthesis()
        {
            queryBuilder.Append("( ");
            return this;
        }

        public IAdtQueryFilterGroup Or()
        {
            queryBuilder.Append("OR ");
            return this;
        }

        public IAdtQueryFrom SelectAll()
        {
            queryBuilder.Append("SELECT * ");
            return this;
        }

        public IAdtQueryFrom SelectCount()
        {
            queryBuilder.Append("SELECT count() ");
            return this;
        }

        public IAdtQueryFrom SelectSingle()
        {
            queryBuilder.Append("SELECT top(1) ");
            return this;
        }

        public IAdtQueryFrom SelectTop(int top)
        {
            queryBuilder.AppendFormat("SELECT top({0}) ", top);
            return this;
        }

        public IAdtQueryFrom Select(params string[] entities)
        {
            queryBuilder.AppendFormat($"select {string.Join(',', entities)} ");
            return this;
        }

        public IAdtQueryFilterGroup Where()
        {
            queryBuilder.Append("where ");
            return this;
        }

        public IAdtQueryFilterGroup WithAnyModel(IEnumerable<string> models, string alias = "")
        {
            var format = string.IsNullOrEmpty(alias) ? "IS_OF_MODEL('{0}')" : $"IS_OF_MODEL({alias}, '{{0}}')";
            queryBuilder.Append($"({string.Join(" OR ", models.Select(x => string.Format(format, x)))}) ");
            return this;
        }

        public IAdtQueryFilterGroup WithBoolProperty(string name, bool value)
        {
            queryBuilder.Append($"{name} = {(value ? "true" : "false")}" );
            return this;
        }

        public IAdtQueryFilterGroup WithIntProperty(string name, int value)
        {
            queryBuilder.Append($"{name} = {value}" );
            return this;
        }

        public IAdtQueryFilterGroup WithPropertyIn(string name, IEnumerable<string> values)
        {
            var statements = values.Split(100).Select(x => {
                if (x.Count() == 1)
                    return $"{name} = '{x.Single().Escape()}'";

                return $"{name} IN [{string.Join(',', x.Select(v => $"'{v.Escape()}'").ToArray())}]"; 
            });
            queryBuilder.Append($"({string.Join(" OR ", statements.ToArray())}) ");
            return this;
        }

        public IAdtQueryFilterGroup WithStringProperty(string name, string value)
        {
            queryBuilder.Append($"{name} = '{value.Escape()}' ");
            return this;
        }

        public IAdtQueryFilterGroup Contains(string name, string value)
        {
            queryBuilder.Append($"contains({name}, '{value.Escape()}') ");
            return this;
        }

        public IAdtQueryWhere JoinRelated(string targetAlias, string sourceAlias, string relationshipName)
        {
            queryBuilder.Append($"JOIN {targetAlias} RELATED {sourceAlias}.{relationshipName} ");
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
}
