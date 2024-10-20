namespace ConnectorCore.Entities
{
    using System;

    internal class TagCategoryLinkEntity
    {
        public Guid TagId { get; set; }

        public Guid CategoryId { get; set; }
    }
}
