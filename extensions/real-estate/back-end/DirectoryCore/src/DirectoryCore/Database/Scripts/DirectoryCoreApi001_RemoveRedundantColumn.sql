-- Update schema: Drop column 'HidAccessFlag' 
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'HidAccessFlag'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN HidAccessFlag;
END
GO

-- Update schema: Update column size
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Timezone'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    ALTER COLUMN Timezone [nvarchar](64) NOT NULL;
END
GO
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Code'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    ALTER COLUMN Code [nvarchar](20) NOT NULL;
END
GO
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Email'
          AND Object_ID = Object_ID(N'dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users
    ALTER COLUMN Email [nvarchar](100) NOT NULL;
END
GO	
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'EmailConfirmationToken'
          AND Object_ID = Object_ID(N'dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users
    ALTER COLUMN EmailConfirmationToken [nvarchar](32) NOT NULL;
END
GO	
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'AvatarUrl'
          AND Object_ID = Object_ID(N'dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users
    ALTER COLUMN AvatarUrl [nvarchar](250) NOT NULL;
END
GO	