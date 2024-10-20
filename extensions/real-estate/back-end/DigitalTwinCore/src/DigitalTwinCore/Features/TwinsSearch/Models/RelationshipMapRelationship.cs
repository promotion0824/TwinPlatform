using Newtonsoft.Json.Linq;

namespace DigitalTwinCore.Features.TwinsSearch.Models
{
    public class RelationshipMapRelationship
    {
        /// <summary>
        /// Id of twin
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Name of twin
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// ModelId of twin
        /// </summary>
        public string ModelId { get; set; }
        /// <summary>
        /// Id of source twin of the relationship
        /// </summary>
        public string SourceId { get; set; }
        /// <summary>
        /// Id of target twin of the relationship
        /// </summary>
        public string TargetId { get; set; }
        /// <summary>
        /// Id of relationship
        /// </summary>
        public string RelId { get; set; }
        /// <summary>
        /// Name of the relationship
        /// </summary>
        public string RelName { get; set; }
        /// <summary>
        /// Raw of the relationship
        /// </summary>
        public JObject RelRaw { get; set; }
        /// <summary>
        /// Substance in the custom properties of the relationship
        /// </summary>
        public string Substance
        {
            get
            {
                var customProperties = RelRaw.SelectToken("customProperties.substance");
                var value = customProperties?.ToString();
                return value;
            }
        }
        /// <summary>
        /// Id of counterpart twin to the given twin in the relationship
        /// </summary>
        public string OpponentId { get; set; }
        /// <summary>
        /// Name of counterpart twin to the given twin in the relationship
        /// </summary>
        public string OpponentName { get; set; }
        /// <summary>
        /// ModelId of counterpart twin to the given twin in the relationship
        /// </summary>
        public string OpponentModelId { get; set; }
        /// <summary>
        /// Number of incoming relationship of the counterpart twin to the given twin in the relationship
        /// </summary>
        public long In { get; set; }
        /// <summary>
        /// Number of outgoing relationship of the counterpart twin to the given twin in the relationship
        /// </summary>
        public long Out { get; set; }
        /// <summary>
        /// Number of relationship of the counterpart twin to the given twin in the relationship
        /// </summary>
        public long OpponentRelationshipCount { get; set; }
    }
}
