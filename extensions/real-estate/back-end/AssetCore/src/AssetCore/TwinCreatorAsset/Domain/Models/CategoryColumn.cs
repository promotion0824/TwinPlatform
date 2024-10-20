namespace AssetCoreTwinCreator.Domain.Models
{
    public class CategoryColumn : Column
    {
        public int CategoryId { get; set; }
        public int DbColumnId { get; set; }
        public string DbColumnName { get; set; }
        public int? PropertyKeyId { get; set; }
    }
}