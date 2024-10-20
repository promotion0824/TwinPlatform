namespace Willow.CognitiveSearch
{
    using Azure.Search.Documents.Indexes.Models;

    /// <summary>
    /// Standard Willow Custom Analyzers.
    /// </summary>
    internal class CustomAnalyzers
    {
        public static readonly string LowerCaseKeywordCustomAnalyzerName = "lower-case-keyword-custom-analyzer";

        /// <summary>
        /// By itself, the built-in keyword analyzer doesn't lower-case any upper-case text, which can cause queries to fail.
        /// A custom analyzer gives you a mechanism for adding the lower-case token filter.
        /// </summary>
        public static readonly CustomAnalyzer LowerCaseKeywordCustomAnalyzer = new CustomAnalyzer(LowerCaseKeywordCustomAnalyzerName, LexicalTokenizerName.Keyword)
        {
            TokenFilters = { TokenFilterName.Lowercase },
        };
    }
}
