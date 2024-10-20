namespace Connector.XL.Common.Models;

internal class CategoryEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Guid ClientId { get; set; }

    public Guid SiteId { get; set; }

    public Guid ParentId { get; set; }
}
