namespace AssetCoreTwinCreator.Domain.Models
{
    public class CategoryGroup
    {
        public int CategoryId { get; set; }
        public int GroupId { get; set; }

        public virtual Category Category { get; set; }
        public virtual Group Group { get; set; }
    }
}
