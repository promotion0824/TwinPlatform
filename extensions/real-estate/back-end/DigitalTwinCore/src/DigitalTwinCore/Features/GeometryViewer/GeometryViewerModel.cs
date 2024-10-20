using DigitalTwinCore.Entities;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Features.GeometryViewer
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

        public static GeometryViewerModel MapFrom(GeometryViewerModelEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new GeometryViewerModel
            {
                Urn = entity.Urn,
                TwinId = entity.TwinId,
                Is3D = entity.Is3D,
                References = GeometryViewerReference.MapFrom(entity.References).ToList()
            };
        }

        public static IEnumerable<GeometryViewerModel> MapFrom(IEnumerable<GeometryViewerModelEntity> entities)
        {
            return entities?.Select(MapFrom);
        }

        public static GeometryViewerModelEntity MapTo(GeometryViewerModel model)
        {
            if (model == null)
            {
                return null;
            }

            return new GeometryViewerModelEntity
            {
                Urn = model.Urn,
                TwinId = model.TwinId,
                Is3D = model.Is3D,
                References = GeometryViewerReference.MapTo(model.References).ToList()
            };
        }

        public static IEnumerable<GeometryViewerModelEntity> MapTo(IEnumerable<GeometryViewerModel> models)
        {
            return models?.Select(MapTo);
        }
    }

    public class GeometryViewerReference
    {
        /// <summary>
        /// Object reference inside the model
        /// </summary>
        public string GeometryViewerId { get; set; }

        public static GeometryViewerReference MapFrom(GeometryViewerReferenceEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new GeometryViewerReference
            {
                GeometryViewerId = entity.GeometryViewerId
            };
        }

        public static IEnumerable<GeometryViewerReference> MapFrom(IEnumerable<GeometryViewerReferenceEntity> entities)
        {
            return entities?.Select(MapFrom);
        }

        public static GeometryViewerReferenceEntity MapTo(GeometryViewerReference reference)
        {
            if (reference == null)
            {
                return null;
            }

            return new GeometryViewerReferenceEntity
            {
                GeometryViewerId = reference.GeometryViewerId
            };
        }

        public static IEnumerable<GeometryViewerReferenceEntity> MapTo(IEnumerable<GeometryViewerReference> references)
        {
            return references?.Select(MapTo);
        }
    }
}
