using SiteCore.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SiteCore.Entities
{
    [Table("PortfolioWidgets")]
    public class PortfolioWidgetEntity
    {
        public Guid PortfolioId { get; set; }
        public Guid WidgetId { get; set; }
        public int Position { get; set; }
        public virtual WidgetEntity Widget { get; set; }

        public static Widget MapToDomainObject(PortfolioWidgetEntity portfolioWidgetEntity)
        {
            if (portfolioWidgetEntity == null)
            {
                return null;
            }

            return new Widget
            {
                Id = portfolioWidgetEntity.WidgetId,
                Type = portfolioWidgetEntity.Widget.Type,
                Metadata = portfolioWidgetEntity.Widget.Metadata,
                Positions = new List<WidgetPosition>() { new WidgetPosition() { PortfolioId = portfolioWidgetEntity.PortfolioId, Position = portfolioWidgetEntity.Position } }
            };
        }

        public static List<Widget> MapToDomainObjects(IEnumerable<PortfolioWidgetEntity> entities)
        {
            return entities.Select(MapToDomainObject).ToList();
        }
    }
}
