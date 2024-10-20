namespace Willow.TopologyIngestion
{
    using System.Diagnostics.Metrics;
    using System.Text.Json;
    using DTDLParser;
    using global::Mapped.Ontologies.Mappings.OntologyMapper;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Willow.Telemetry;
    using Willow.TopologyIngestion.Interfaces;
    using Willow.TopologyIngestion.Mapped;

    /// <summary>
    /// Loads a building graph from a Mapped input source to a Willow Target.
    /// The logic here is specific to the way Mapped stores its topology and if a new
    /// Input Graph Provider is added, this logic will likely have to be customized.
    /// </summary>
    /// <typeparam name="TOptions">Anything that inherits from the base class of IngestionManagerOptions.</typeparam>
    public class WillowGraphIngestionProcessor<TOptions> : MappedGraphIngestionProcessor<TOptions>
        where TOptions : IngestionManagerOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WillowGraphIngestionProcessor{TOptions}"/> class.
        /// </summary>
        /// <param name="logger">An instance of an <see cref="ILogger">ILogger</see> used to log status as needed.</param>
        /// <param name="inputGraphManager">An instance of an <see cref="IInputGraphManager">IInputGraphManager</see> used to load a graph from the input source.</param>
        /// <param name="ontologyMappingManager">An instance of an <see cref="IOntologyMappingManager">IOntologyMappingManager</see> used to map the input ontology to the output ontology.</param>
        /// <param name="outputGraphManager">An instance of an <see cref="IOutputGraphManager">IOutputGraphManager</see> used to save a graph to the output target.</param>
        /// <param name="graphNamingManager">An instance of an <see cref="IGraphNamingManager">IGraphNamingManager</see> used to allow naming of certain properties.</param>
        /// <param name="options">An instance of the Ingestion Manager Options class.</param>
        /// <param name="meterFactory">The meter factory for creating meters.</param>
        /// <param name="metricsAttributesHelper">Instance of metrics helper.</param>
        public WillowGraphIngestionProcessor(ILogger<MappedGraphIngestionProcessor<TOptions>> logger,
                                             IInputGraphManager inputGraphManager,
                                             IOntologyMappingManager ontologyMappingManager,
                                             IOutputGraphManager outputGraphManager,
                                             IGraphNamingManager graphNamingManager,
                                             IOptions<TOptions> options,
                                             IMeterFactory meterFactory,
                                             MetricsAttributesHelper metricsAttributesHelper)
            : base(logger,
                   inputGraphManager,
                   ontologyMappingManager,
                   outputGraphManager,
                   graphNamingManager,
                   options,
                   meterFactory,
                   metricsAttributesHelper)
        {
        }

        /// <summary>
        /// This method is called for each twin in the graph to add properties to the twin based on inputs.
        /// </summary>
        /// <param name="inputDtmi">The DTMI of the input twin.</param>
        /// <returns>A dictionary of strings and objects to add to the contents of the twin.</returns>
        protected override IDictionary<string, object> GetTargetSpecificContents(Dtmi inputDtmi)
        {
            var contentDictionary = new Dictionary<string, object>();

            // Add the alternate classification to the twin (brickSchema)
            var val = new { code = inputDtmi.AbsoluteUri, version = "1.3" };
            var alternateClassification = new { brickSchema = val };
            contentDictionary.Add("alternateClassification", JsonSerializer.SerializeToDocument(alternateClassification).RootElement);

            return contentDictionary;
        }
    }
}
