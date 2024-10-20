using System;
using System.Collections.Generic;

namespace PlatformPortalXL.ServicesApi.GeometryViewerApi
{
    public class GeometryViewerModel
    {
        /// <summary>
        /// Urn of the model
        /// </summary>
        public string Urn { get; set; }

        /// <summary>
        /// Twin id associated with the model
        /// </summary>
        public string TwinId { get; set; }

        /// <summary>
        /// Whether the model is a 3D model
        /// </summary>
        public bool Is3D { get; set; }

        /// <summary>
        /// List of model object references
        /// </summary>
        public List<GeometryViewerReference> References { get; set; }
    }

    public class GeometryViewerReference
    {
        /// <summary>
        /// Object reference inside the model
        /// </summary>
        public string GeometryViewerId { get; set; }
    }
}
