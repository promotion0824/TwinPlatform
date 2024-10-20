using OntologyGraphTool.Models;

namespace OntologyGraphTool.Services;

public class MappingService
{
    public MappedOntology Source { get; set; }

    public WillowOntology Destination { get; set; }

    public MappingService(WillowOntology ontologyTwo, MappedOntology ontologyOne)
    {
        this.Source = ontologyOne;
        this.Destination = ontologyTwo;
        this.lazyMappings = new Lazy<List<Mapping>>(() => CreateMappings());
    }

    public List<Mapping> Mappings => lazyMappings.Value;

    private Lazy<List<Mapping>> lazyMappings;

    public List<Mapping> CreateMappings()
    {
        double bestScore = 0.001;

        List<Mapping> mappingList = new List<Mapping>();

        // Must create all these up front to populate the TFIDF model
        List<EnhancedModel> source = Source.Models.Select(x => EnhancedModel.FromModel(x, Source.Models)).ToList();
        List<EnhancedModel> dest = Destination.Models.Select(x => EnhancedModel.FromModel(x, Destination.Models)).ToList();

        List<EnhancedModel> nonMappedDestinations = new List<EnhancedModel>();

        foreach (var s in source)
        {
            List<Mapping> oneSet = new List<Mapping>();

            double bestScoreForMatch = 0.0;

            foreach (var d in dest)
            {
                double score = s.Score(d);

                if (score == 0) continue;

                if (score > bestScore) bestScore = score;
                if (score > bestScoreForMatch) bestScoreForMatch = score;
                // Filter out low quality scores
                // if (score > 0.0005 * bestScore)
                // {
                var newMapping = new Mapping(s, d, score, 0);
                oneSet.Add(newMapping);
                // }
            }

            int index = 0;
            foreach (var m in oneSet.OrderByDescending(x => x.Score)
                .Where(x => x.Score > 0.7 * bestScoreForMatch) // eliminate low quality
                .Take(10)) // but still limit it
            {
                m.Index = index++;   // overwrite the index now we know the order
                mappingList.Add(m);
            };
        }

        void AddMapping(string mappedId, string willowId)
        {
            var mapped = source.FirstOrDefault(x => x.id == mappedId);
            var willow = dest.FirstOrDefault(x => x.id == willowId);
            if ((mapped is not null) && (willow is not null))
            {
                // Overwrite any already there
                mappingList.RemoveAll(x => x.Source.id == mappedId);
                mappingList.Add(new Mapping(mapped, willow, 0.9, 0) { AncestorScore = 1.0 });
            }
        }

        // Add some known mappings to the mix
        foreach (var knownMapping in Rules.KnownMappings)
        {
            AddMapping(knownMapping.a, knownMapping.b);
        }

        // Add any missing forward mappings W->M
        foreach (var mapped in source)
        {
            if (!mappingList.Any(x => x.Source.id == mapped.id))
            {
                mappingList.Add(new Mapping(mapped, EnhancedModel.None, 0.0, 1));
            }
        }

        // Add any missing reverse mappings W->M
        foreach (var willow in dest)
        {
            if (!mappingList.Any(x => x.Destination.id == willow.id))
            {
                mappingList.Add(new Mapping(EnhancedModel.None, willow, 0.0, 1));
            }
        }

        // We have the first pass, now add the ancestry checks
        // which will use the first pass probabilities

        Dictionary<string, Mapping> mappings = mappingList.ToDictionary(x => x.id, x => x);

        List<Mapping> finalMappings = new List<Mapping>();

        foreach (var mapping in mappings.Values)
        {
            if (mapping.AncestorScore == 0.0)
            {
                var sourceAncestors = mapping.Source.Ancestors;
                var destAncestors = mapping.Destination.Ancestors;

                double score = 0.0;
                int countMissing = 0;
                foreach (var sourceAncestor in sourceAncestors)
                    foreach (var destAncestor in destAncestors)
                    {
                        if (mappings.TryGetValue(sourceAncestor + "_" + destAncestor, out var parentMapping))
                        {
                            score = score + parentMapping.Score - (score * parentMapping.Score);
                        }
                        else
                        {
                            countMissing++;
                        }
                    }

                // Score reduced for missing mappings in ancestry
                mapping.AncestorScore = score * Math.Pow(0.96, countMissing);
            }
            finalMappings.Add(mapping);
        }

        return finalMappings;
    }
}
