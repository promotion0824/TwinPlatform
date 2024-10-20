namespace PlatformPortalXL.ServicesApi.DirectoryApi
{
    /// <summary>
    /// Used to restrict the wait time for DirectoryApiService Sign In endpoints
    /// </summary>
    public class DirectoryApiServiceOptions
    {
        /// <summary>
        /// The sign in timeout in seconds
        /// </summary>
        public int SignInTimeoutSeconds { get; set; }
    }
}
