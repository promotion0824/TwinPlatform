using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SiteCore.Entities
{
    [Table("SiteWidgets")]
    public class SiteWidgetEntity
    {
        public Guid SiteId { get; set; }
        public Guid WidgetId { get; set; }

        public string Position { get; set; }

        public virtual SiteEntity Site { get; set; }
        public virtual WidgetEntity Widget { get; set; }

        public static Widget MapToDomainObject(SiteWidgetEntity siteWidgetEntity)
        {
            if (siteWidgetEntity == null)
            {
                return null;
            }

            return new Widget
            {
                Id = siteWidgetEntity.WidgetId,
                Type = siteWidgetEntity.Widget.Type,
                Metadata = siteWidgetEntity.Widget.Metadata,
                Positions = new List<WidgetPosition>() { new WidgetPosition() 
                { 
                    SiteId = siteWidgetEntity.SiteId, 
                    SiteName = siteWidgetEntity.Site.Name,
                    Position = int.Parse(siteWidgetEntity.Position) 
                } }
            };
        }

        public static List<Widget> MapToDomainObjects(IEnumerable<SiteWidgetEntity> entities)
        {
            return entities.Select(MapToDomainObject).ToList();
        }
    }
}
