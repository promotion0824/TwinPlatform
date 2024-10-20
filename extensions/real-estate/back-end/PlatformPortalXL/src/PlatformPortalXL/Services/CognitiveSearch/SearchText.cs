using System;
using System.Text;
using Azure.Search.Documents.Indexes.Models;

namespace PlatformPortalXL.Services.CognitiveSearch;

public static class SearchText
{
    /// <summary>
    /// Escape special characters in the input search term.
    /// </summary>
    /// <remarks>
    /// In order to use certain special characters in a search query, they must be escaped by preceding them with a
    /// backslash. Lucene and Simple query syntax have different special characters that need to be escaped.
    /// https://learn.microsoft.com/en-us/azure/search/query-lucene-syntax#bkmk_syntax
    /// https://learn.microsoft.com/en-us/azure/search/query-simple-syntax#escaping-search-operators
    /// </remarks>
    public static string Escape(string input, string lexicalAnalyzer)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        StringBuilder inputEscaped = new();

        var specialCharacters = lexicalAnalyzer switch
        {
            LexicalAnalyzerName.Values.EnLucene => "+-&|!(){}[]^\"~*?:\\/",
            LexicalAnalyzerName.Values.Simple => "+|()\"\\",
            _ => throw new ArgumentException($"Unknown lexical analyzer: {lexicalAnalyzer}")
        };

        foreach (var c in input)
        {
            if (specialCharacters.Contains(c))
            {
                inputEscaped.Append($@"\{c}");
            }
            else if ('\'' == c)
            {
                inputEscaped.Append("''");
            }
            else
            {
                inputEscaped.Append(c);
            }
        }

        return inputEscaped.ToString();
    }
}
