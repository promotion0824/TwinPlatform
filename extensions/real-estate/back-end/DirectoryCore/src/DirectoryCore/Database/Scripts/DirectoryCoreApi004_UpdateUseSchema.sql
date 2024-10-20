IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Type'
          AND Object_ID = Object_ID(N'dbo.Customers'))
BEGIN
    ALTER TABLE dbo.Customers
    DROP COLUMN [Type];
END
GO
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'LogoUrl'
          AND Object_ID = Object_ID(N'dbo.Customers'))
BEGIN
    ALTER TABLE dbo.Customers
    DROP COLUMN LogoUrl;
END
GO
ALTER TABLE dbo.Customers
ADD LogoId UNIQUEIDENTIFIER NULL;
GO
ALTER TABLE dbo.Users
ADD CustomerId UNIQUEIDENTIFIER NULL;
GO
UPDATE U SET U.CustomerId = CU.CustomerId from Users U INNER JOIN CustomerUsers CU on U.Id = CU.UserId
GO
ALTER TABLE dbo.Users
ALTER COLUMN CustomerId UNIQUEIDENTIFIER NOT NULL;
DROP TABLE dbo.CustomerUsers
GO
DROP TABLE dbo.SiteUsers
GO
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'IsSuperUser'
          AND Object_ID = Object_ID(N'dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users
    DROP COLUMN [IsSuperUser];
END
GO
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'IsCustomerUser'
          AND Object_ID = Object_ID(N'dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users
    DROP COLUMN [IsCustomerUser];
END
GO
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'IsSiteUser'
          AND Object_ID = Object_ID(N'dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users
    DROP COLUMN [IsSiteUser];
END
GO
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'DeviceId'
          AND Object_ID = Object_ID(N'dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users
    DROP COLUMN [DeviceId];
END
GO
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'BuildingAddress'
          AND Object_ID = Object_ID(N'dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users
    DROP COLUMN [BuildingAddress];
END

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Gender'
          AND Object_ID = Object_ID(N'dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users
    DROP COLUMN [Gender];
END
GO
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'AvatarUrl'
          AND Object_ID = Object_ID(N'dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users
    DROP COLUMN AvatarUrl;
END
GO
ALTER TABLE dbo.Users
ADD AvatarId UNIQUEIDENTIFIER NULL;
GO
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'AvatarUrl'
          AND Object_ID = Object_ID(N'dbo.Supervisors'))
BEGIN
    ALTER TABLE dbo.Supervisors
    DROP COLUMN AvatarUrl;
END
GO
ALTER TABLE dbo.Supervisors
ADD AvatarId UNIQUEIDENTIFIER NULL;
GO