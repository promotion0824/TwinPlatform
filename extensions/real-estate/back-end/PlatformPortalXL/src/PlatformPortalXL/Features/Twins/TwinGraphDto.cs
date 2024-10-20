using System;

namespace PlatformPortalXL.Features.Twins
{
    public class TwinGraphDto
    {
        public TwinNodeDto[] Nodes { get; set; }
        public TwinEdgeDto[] Edges { get; set; }
    }

    public struct TwinEdgeDto : IEquatable<TwinEdgeDto>
    {
        /// <summary>
        /// Relationship Id
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Relationship name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Relationship type
        /// </summary>
        public string Substance { get; set; }
        
        /// <summary>
        /// Source Id
        /// </summary>
        public string SourceId { get; set; }
        
        /// <summary>
        /// Target Id
        /// </summary>
        public string TargetId { get; set; }

        public bool Equals(TwinEdgeDto other) =>
            (Id, Name, Substance).Equals((other.Id, other.Name, other.Substance));
    }

    public struct TwinNodeDto : IEquatable<TwinNodeDto>
    {        
        /// <summary>
        /// Id of the twin
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Name of the twin
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Model Id like dtmi:...
        /// </summary>
        public string ModelId { get; set; }

        /// <summary>
        /// Count of twin incoming relationships
        /// </summary>
        public long? EdgeInCount { get; set; }

        /// <summary>
        /// Count of twin outgoing relationships
        /// </summary>
        public long? EdgeOutCount { get; set; }

        /// <summary>
        /// Count of twin relationships
        /// </summary>
        public long? EdgeCount { get; set; }

        public bool Equals(TwinNodeDto other) => (this.Id, this.Name, this.ModelId)
            .Equals((other.Id, other.Name, other.ModelId));
    }
}
