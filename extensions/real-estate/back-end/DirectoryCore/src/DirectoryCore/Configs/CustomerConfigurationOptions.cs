namespace DirectoryCore.Configs
{
    /// <summary>
    /// Customer info. These values will be the same for different customer instances
    /// for the same customer.
    /// </summary>
    public class CustomerConfigurationOptions
    {
        /// <summary>
        /// Display name, displayable in the UI.
        /// </summary>
        public string DisplayName { get; set; }
    }
}
