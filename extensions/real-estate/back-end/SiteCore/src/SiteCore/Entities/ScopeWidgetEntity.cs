using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SiteCore.Entities
{
    [Table("ScopeWidgets")]
    public class ScopeWidgetEntity
    {
        public Guid Id { get; set; }
        public string ScopeId { get; set; }
        public Guid WidgetId { get; set; }

        public string Position { get; set; }
        public virtual WidgetEntity Widget { get; set; }

        public static Widget MapToDomainObject(ScopeWidgetEntity scopeWidgetEntity)
        {
            if (scopeWidgetEntity == null)
            {
                return null;
            }

            return new Widget
            {
                Id = scopeWidgetEntity.WidgetId,
                Type = scopeWidgetEntity.Widget.Type,
                Metadata = scopeWidgetEntity.Widget.Metadata,
                Positions = new List<WidgetPosition>() { new WidgetPosition()
                {
                    ScopeId = scopeWidgetEntity.ScopeId,
                    Position = int.Parse(scopeWidgetEntity.Position)
                } }
            };
        }

        public static List<Widget> MapToDomainObjects(IEnumerable<ScopeWidgetEntity> entities)
        {
            return entities.Select(MapToDomainObject).ToList();
        }
    }
}
