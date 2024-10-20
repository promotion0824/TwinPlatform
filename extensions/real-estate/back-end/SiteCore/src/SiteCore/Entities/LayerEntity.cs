using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SiteCore.Entities
{
    [Table("Layers")]
    public class LayerEntity
    {

        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LayerGroupId { get; set; }
        public string TagName { get; set; }
        public int SortOrder { get; set; }
        public virtual LayerGroupEntity LayerGroup { get; set; }

        public static Layer MapToDomainObject(LayerEntity layerEntity)
        {
            if (layerEntity == null)
            {
                return null;
            }

            return new Layer
            {
                Id = layerEntity.Id,
                Name = layerEntity.Name,
                LayerGroupId = layerEntity.LayerGroupId,
                SortOrder = layerEntity.SortOrder,
                TagName = layerEntity.TagName
            };
        }

        public static List<Layer> MapToDomainObjects(IEnumerable<LayerEntity> layerEntities)
        {
            return layerEntities.Select(MapToDomainObject).ToList();
        }
    }
}
