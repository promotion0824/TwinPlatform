using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalTwinCore.Services.Adx
{
    public interface IAdxQueryBuilder
    {
        string GetQuery();
    }

    public interface IAdxQuerySelector : IAdxQueryBuilder
    {
        IAdxQueryWhere Select(string target);
        IAdxQuerySelector Join(string target, string onSourceProperty, string onTargetProperty, string kind = null);
        IAdxQuerySelector Union();
        IAdxQuerySelector Project(params string[] properties);
        IAdxQuerySelector ProjectKeep(params string[] properties);
        IAdxQueryFilterGroup Summarize();
        IAdxQuerySelector Let(string variableName, string query);
        IAdxQueryFilterGroup Materialize(string query);
    }

    public interface IAdxQueryWhere : IAdxQueryBuilder
    {
        IAdxQueryFilterGroup Where();
    }

    public interface IAdxQueryFilterGroup : IAdxQueryBuilder
    {
        IAdxQueryFilterGroup OpenGroupParentheses();
        IAdxQueryFilterGroup CloseGroupParentheses();
        IAdxQueryFilterGroup Property(string name, string value);
        IAdxQueryFilterGroup Contains(string propertyName, string value);
        IAdxQueryFilterGroup PropertyIn(string name, IEnumerable<string> values);
        IAdxQueryFilterGroup Between(string propertyName, int start, int end);
        IAdxQueryFilterGroup IsNotEmpty(string propertyName);
        IAdxQueryFilterGroup IsEmpty(string propertyName);
        IAdxQueryFilterGroup SetProperty(string propertyName);
        IAdxQueryFilterGroup And();
        IAdxQueryFilterGroup Or();
        IAdxQueryFilterGroup Sort(string by, bool desc = false);
        IAdxQueryFilterGroup MakeSet(bool multiple = false, params string[] from);
        IAdxQueryFilterGroup TakeAny(bool multiple = false, params string[] from);
        IAdxQueryFilterGroup MakeBag(bool multiple = false, params string[] from);
        IAdxQueryFilterGroup By(params string[] properties);
        IAdxQueryFilterGroup Pack(Dictionary<string, string> values);
        IAdxQueryFilterGroup Compress(string val);
        IAdxQueryFilterGroup CountDistinct(string val);
        IAdxQueryFilterGroup Take(int val);
    }

    public class AdxQueryBuilder : IAdxQueryBuilder, IAdxQuerySelector, IAdxQueryWhere, IAdxQueryFilterGroup
    {
        private readonly StringBuilder _sb = new StringBuilder();

        public static IAdxQuerySelector Create()
        {
            return new AdxQueryBuilder();
        }

        public IAdxQueryWhere Select(string target)
        {
            _sb.AppendLine(target);
            return this;
        }

        public IAdxQuerySelector Join(
            string target,
            string onSourceProperty,
            string onTargetProperty,
            string kind = null)
        {
            _sb.Append($"| join ");

            if (!string.IsNullOrWhiteSpace(kind))
            {
                _sb.Append($"kind={kind} ");
            }

            _sb.AppendLine($"({target}) on $left.{onSourceProperty} == $right.{onTargetProperty} ");

            return this;
        }

        public IAdxQuerySelector Union()
        {
            _sb.AppendLine("| union");
            return this;
        }

        public IAdxQuerySelector Project(params string[] properties)
        {
            _sb.AppendLine($"| project {string.Join(',', properties)}");
            return this;
        }

        public IAdxQuerySelector ProjectKeep(params string[] properties)
        {
            _sb.AppendLine($"| project-keep {string.Join(',', properties)}");
            return this;
        }

        public IAdxQueryFilterGroup Summarize()
        {
            _sb.AppendLine($"| summarize ");
            return this;
        }

        public IAdxQuerySelector Let(string variableName, string query)
        {
            _sb.AppendLine($"let {variableName} = {query};");
            return this;
        }

        public IAdxQueryFilterGroup Where()
        {
            _sb.AppendLine("| where ");
            return this;
        }

        public IAdxQueryFilterGroup OpenGroupParentheses()
        {
            _sb.Append("(");
            return this;
        }

        public IAdxQueryFilterGroup CloseGroupParentheses()
        {
            _sb.Append(")");
            return this;
        }

        public IAdxQueryFilterGroup Property(string propertyName, string value)
        {
            _sb.Append($"{propertyName} == \"{value}\" ");
            return this;
        }

        public IAdxQueryFilterGroup SetProperty(string propertyName)
        {
            _sb.Append($"{propertyName} = ");
            return this;
        }

        public IAdxQueryFilterGroup Contains(string propertyName, string value)
        {
            _sb.Append($"{propertyName} contains \"{value}\" ");
            return this;
        }

        public IAdxQueryFilterGroup PropertyIn(string propertyName, IEnumerable<string> values)
        {
            _sb.Append($"{propertyName} in ({string.Join(',', values.Select(v => $"\"{v}\""))}) ");
            return this;
        }

        public IAdxQueryFilterGroup IsNotEmpty(string propertyName)
        {
            _sb.Append($"isnotempty({propertyName})");
            return this;
        }

        public IAdxQueryFilterGroup IsEmpty(string propertyName)
        {
            _sb.Append($"isempty({propertyName})");
            return this;
        }

        public IAdxQueryFilterGroup Between(string propertyName, int start, int end)
        {
            _sb.Append($"{propertyName} between ({start} .. {end})");
            return this;
        }

        public IAdxQueryFilterGroup And()
        {
            _sb.Append("and ");
            return this;
        }

        public IAdxQueryFilterGroup Or()
        {
            _sb.Append("or ");
            return this;
        }

        public IAdxQueryFilterGroup Sort(string by, bool desc = false)
        {
            var direction = (desc) ? "desc" : "asc";
            _sb.AppendLine($"| sort by {by} {direction} ");
            return this;
        }

		public IAdxQueryFilterGroup MakeSet(bool multiple = false, params string[] from)
		{
            var comma = (multiple) ? "," : "";
            _sb.Append($"make_set({string.Join(',', from)}){comma} ");
            return this;
        }

        public IAdxQueryFilterGroup Pack(Dictionary<string, string> values)
        {
            _sb.Append($"pack({string.Join(", ", values.Select(x => $"\"{x.Key}\", {x.Value}"))}) ");
            return this;
        }

        public IAdxQueryFilterGroup TakeAny(bool multiple = false, params string[] from)
        {
            var comma = (multiple) ? "," : "";
            _sb.Append($"take_any({string.Join(',', from)}){comma} ");
            return this;
        }

        public IAdxQueryFilterGroup MakeBag(bool multiple = false, params string[] from)
		{
            var comma = (multiple) ? "," : "";
            _sb.Append($"make_bag({string.Join(',', from)}){comma} ");
            return this;
        }

		public IAdxQueryFilterGroup By(params string[] properties)
		{
            _sb.Append($"by {string.Join(',', properties)}");
            return this;
        }

        public IAdxQueryFilterGroup Compress(string val)
        {
            _sb.Append($"zlib_compress_to_base64_string({val})");
            return this;
        }

        public IAdxQueryFilterGroup CountDistinct(string val)
        {
            _sb.Append($"dcount({val})");
            return this;
        }

        public IAdxQueryFilterGroup Materialize(string query)
        {
            _sb.Append($"materialize({query})");
            return this;
        }

        public IAdxQueryFilterGroup Take(int val)
        {
            _sb.AppendLine($"| take {val} ");
            return this;
        }

        public string GetQuery()
        {
            return _sb.ToString();
        }
    }
}
