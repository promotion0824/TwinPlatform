using System.Collections.Concurrent;
using Abodit.Graph;
using Abodit.Mutable;
using Willow.Rules.Model;

namespace OntologyGraphTool.Models;

public class EnhancedModel
{
    public static EnhancedModel None = new EnhancedModel()
    {
        id = "none",
        displayName = "None",
        description = "",
        BagOfWords = new HashSet<string>(),
        Ancestors = new List<string>()
    };

    public string id { get; set; }

    public List<string> Ancestors { get; set; }

    // public string type { get; set; }

    // public Content[] contents { get; set; }

    public TextLang description { get; set; }

    public TextLang displayName { get; set; }

    public HashSet<string> BagOfWords { get; set; }

    private static ConcurrentDictionary<string, int> termFrequency = new ConcurrentDictionary<string, int>();

    private static List<(string a, string b)> synonyms = new List<(string a, string b)>{
        ("Level", "Floor"),
        ("Command", "Actuator")
    };

    private static List<string> stopWords = new List<string>(){
        "the", "to", "with"  // NB not "On"
    };

    private static List<(string a, string b)> replacements = new List<(string a, string b)>(){
        ("Time Zone", "TimeZone"),
        ("Fan Powered" , "FanPowered")  // it's not a Fan
    };


    /// <summary>
    /// Creates an Enhanced Model from a model
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static EnhancedModel FromModel(DtdlModel model, Graph<DtdlModel, Relation> ontology)
    {
        string name = model.displayName?.en ?? "";

        foreach (var replacement in replacements)
        {
            if (name.Contains(replacement.a)) name = name.Replace(replacement.a, replacement.b);
        }

        var bagOfWords = name
                .Split(' ', '_', '-', '/', ':', '.', ';', '(', ')')
                .Except(stopWords)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

        foreach (string word in bagOfWords)
        {
            termFrequency.AddOrUpdate(word, 1, (key, y) => y + 1);
            // Override these two, common words, but very important matches
        }

        // Add synonyms AFTER updating TFIDF

        foreach (var synonym in synonyms)
        {
            if (bagOfWords.Contains(synonym.a)) bagOfWords.Add(synonym.b);
            if (bagOfWords.Contains(synonym.b)) bagOfWords.Add(synonym.a);
        }

        return new EnhancedModel
        {
            id = model.id,
            // type = model.type,
            // contents = model.contents,
            description = model.description,
            displayName = model.displayName ?? "",
            BagOfWords = new HashSet<string>(bagOfWords),
            Ancestors = ontology.DistanceToEverywhere(model, false, Relation.RDFSType)
                    .OrderBy(e => e.Item2)
                    .Select(y => y.Item1.id)
                    .Except(new[] { model.id })
                    .ToList()

        };
    }

    private double Tfidf(string word)
    {
        if (termFrequency.TryGetValue(word, out var frequency))
        {
            return 1.0 / (1 + frequency);
        }
        else
        {
            return 0.001;
        }
    }

    // Mapped Freezer is a room, Willow Freezer is a Space


    //  dtmi:org:brickschema:schema:Brick:Space;1

    /// <summary>
    /// Score one model against another based on words in common and (TODO) ontology graph
    /// </summary>
    public double Score(EnhancedModel other)
    {
        var wordsInCommon = this.BagOfWords.Intersect(other.BagOfWords);
        var wordsNotInCommon = this.BagOfWords.Union(other.BagOfWords).Except(wordsInCommon);

        if (!wordsInCommon.Any()) return 0.0;

        foreach (var illegal in Rules.IllegalCombinations)
        {
            if (this.BagOfWords.Contains(illegal.a) && other.BagOfWords.Contains(illegal.b)) return 0.0;
            if (this.BagOfWords.Contains(illegal.b) && other.BagOfWords.Contains(illegal.a)) return 0.0;
        }

        foreach (var illegal in Rules.IllegalAncestorCombinations)
        {
            if (this.id == "dtmi:org:brickschema:schema:Brick:Pressure_Sensor;1" && illegal.a == "dtmi:org:brickschema:schema:Brick:Pressure_Sensor;1")
            {
                Console.WriteLine(this.id + "  ->  " + illegal.b);
            }

            bool aMatches = (this.id == illegal.a) || this.Ancestors.Contains(illegal.a);
            bool bMatches = (other.id == illegal.b) || other.Ancestors.Contains(illegal.b);
            if (aMatches && bMatches) return 0.0;
        }

        int totalWords = wordsInCommon.Count() + wordsNotInCommon.Count();

        // Rarer words are worth more than common words
        // And overall propability is the product of all the individual probabilities
        double tfidfIncommon = wordsInCommon.Select(x => Tfidf(x)).Aggregate(1.0, (x, y) => x + y) /
            totalWords;

        // // OR the missing scores together
        // double tfidfMissing = wordsNotInCommon.Select(x => Tfidf(x)).Aggregate(0.0, (x, y) => (x + y) - (x * y));

        // // Missing words reduce score further
        // double missingScore = 1.0 - tfidfMissing; //Math.Pow(0.5, wordsNotInCommon.Count());

        return tfidfIncommon; // * missingScore;
    }

}
