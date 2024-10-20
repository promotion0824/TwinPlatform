namespace AssetCoreTwinCreator.Models
{
    public class AssetParameter
    {
        /// <summary>
        /// DbColumnName of the relevant category column
        /// </summary>
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public object Value { get; set; }
    }
}
