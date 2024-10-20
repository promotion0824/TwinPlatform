using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SiteCore.Entities
{
    [Table("Widgets")]
    public class WidgetEntity
    {
        public Guid Id { get; set; }
        public WidgetType Type { get; set; }
        public string Metadata { get; set; }

        public ICollection<ScopeWidgetEntity> ScopeWidgets { get; set; }
        public ICollection<SiteWidgetEntity> SiteWidgets { get; set; }
        public ICollection<PortfolioWidgetEntity> PortfolioWidgets { get; set; }

        public static WidgetEntity MapFrom(Widget widget)
        {
            if (widget == null)
            {
                return null;
            }

            return new WidgetEntity
            {
                Id = widget.Id,
                Type = widget.Type,
                Metadata = widget.Metadata
            };
        }

        public static Widget MapToDomainObject(WidgetEntity widgetEntity)
        {
            if (widgetEntity == null)
            {
                return null;
            }

            return new Widget
            {
                Id = widgetEntity.Id,
                Type = widgetEntity.Type,
                Metadata = widgetEntity.Metadata,
                Positions = MapToPositions(widgetEntity)
            };
        }

        public static List<WidgetPosition> MapToPositions(WidgetEntity widgetEntity)
        {
            var widgetPositions = new List<WidgetPosition>();

            if (widgetEntity.ScopeWidgets != null && widgetEntity.ScopeWidgets.Any())
            {
                widgetPositions.AddRange(widgetEntity.ScopeWidgets?.Select(x => new WidgetPosition { ScopeId = x.ScopeId, Position = int.Parse(x.Position) }));
            }

            if (widgetEntity.SiteWidgets != null && widgetEntity.SiteWidgets.Any())
            {
                widgetPositions.AddRange(widgetEntity.SiteWidgets?.Select(x => new WidgetPosition { SiteId = x.SiteId, Position = int.Parse(x.Position) }));
            }

            if (widgetEntity.PortfolioWidgets != null && widgetEntity.PortfolioWidgets.Any())
            {
                widgetPositions.AddRange(widgetEntity.PortfolioWidgets?.Select(x => new WidgetPosition { PortfolioId = x.PortfolioId, Position = x.Position }));
            }

            return widgetPositions; 
        }

        public static List<Widget> MapToDomainObjects(IEnumerable<WidgetEntity> entities)
        {
            return entities.Select(MapToDomainObject).ToList();
        }
    }
}
