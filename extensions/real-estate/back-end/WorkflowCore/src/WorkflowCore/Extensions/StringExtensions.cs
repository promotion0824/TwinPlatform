using System.Collections.Generic;

namespace WorkflowCore
{
    public static class StringExtensions
    {
        public static string SubstringBefore(this string str, string substr)
        {
            if(str == null)
                return str;

            if(string.IsNullOrEmpty(substr))
                return str;

            var index = str.IndexOf(substr);

            if(index == -1)
                return str;

            return str.Substring(0, index);
        }

        public static IDictionary<string, string> CsvToDictionary(this string csv, string recordSeparator = ",", string keySeparator = " ")
        {
            var dictionary = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(csv))
            {
                var records = csv.Split(recordSeparator);

                foreach (var record in records)
                {
                    var recordPair = record.Trim().Split(keySeparator);

                    if (recordPair.Length > 0 && !dictionary.ContainsKey(recordPair[0]))
                    {
                        dictionary.Add(recordPair[0].Trim(), (recordPair.Length > 1) ? recordPair[1].Trim() : string.Empty);
                    }
                }
            }

            return dictionary;
        }
    }
}
