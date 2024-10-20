using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Common
{
    public static class StringExtensions
    {
        public static string SubstringBefore(this string str, string substr)
        {
            if(str == null)
            { 
                return str;
            }

            if(string.IsNullOrEmpty(substr))
            { 
                return str;
            }

            var index = str.IndexOf(substr);

            if(index == -1)
            { 
                return str;
            }

            return str.Substring(0, index);
        }

        public static string Substitute(this string str, object data, string before = "{", string after = "}")
        {
            if(str == null || data == null)
            { 
                return str;
            }

            if(string.IsNullOrEmpty(str))
            { 
                return str;
            }

            var dData = data.ToDictionary();

            if(dData.Count == 0)
            { 
                return str;
            }

            var result = str;

            foreach(var kv in dData)
            {
                var replace = before + kv.Key + after;

                result = result.Replace(replace, kv.Value?.ToString() ?? "", StringComparison.InvariantCultureIgnoreCase);
            }

            return result;
        }

        public static IList<IList<string>> ParseCSV(this string csvFile, char separator = ',')
        {
            var lines = csvFile.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var result = new List<IList<string>>();

            foreach(var line in lines)
            { 
                var fields = line.ParseCSVLine(separator);

                result.Add(fields);
            }

            return result;
        }

        public static IList<string> ParseCSVLine(this string csvLine, char separator = ',')
        {
            if(!csvLine.Contains('"'))
            { 
                return new List<string>(csvLine.Split(new[] { separator } ));
            }

            var result   = new List<string>();
            var inQuotes = false;
            var sb       = new StringBuilder();
            var inDoubleDoubleQuotes = false;

            for(var i = 0; i < csvLine.Length; ++i)
            {
                var ch = csvLine[i];

                if(ch == '"')
                {
                    if(inQuotes)
                    { 
                        if(inDoubleDoubleQuotes)
                        {
                            // Already added quotes above
                            inDoubleDoubleQuotes = false;
                            continue;
                        }

                        if(i != csvLine.Length - 1) // Not last char
                        {
                            // Double double quotes get added as quotes
                            if(csvLine[i+1] == '"')
                            {
                                sb.Append(ch);
                                inDoubleDoubleQuotes = true;
                                continue;
                            }
                        }
                    }

                    inDoubleDoubleQuotes = false;
                    inQuotes = !inQuotes;

                    continue;
                }
                else if(ch == separator && !inQuotes)
                {
                    result.Add(sb.ToString().Trim());
                    sb.Clear();

                    continue;
                }

                sb.Append(ch);
            }

            result.Add(sb.ToString().Trim());

            return result;        
        }
    }
}
