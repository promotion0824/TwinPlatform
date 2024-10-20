namespace DirectoryCore.Enums
{
    public enum CustomerStatus
    {
        None = 0,
        Active = 1,
        Inactive = 2,
        TransferredToSingleTenant = 3
    }

    public enum SiteStatus
    {
        Unknown = 0,
        Operations = 1,
        Construction = 2,
        Design = 3,
        Selling = 4,
        Deleted = 10
    }

    public enum UserStatus
    {
        Pending = -1,
        Deleted = 0,
        Active = 1,
        Inactive = 2
    }
}
