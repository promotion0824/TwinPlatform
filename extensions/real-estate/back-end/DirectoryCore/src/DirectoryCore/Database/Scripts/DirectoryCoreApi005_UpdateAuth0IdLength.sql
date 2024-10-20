
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Auth0UserId'
          AND Object_ID = Object_ID(N'dbo.Users'))
BEGIN
    ALTER TABLE dbo.Users
    ALTER COLUMN Auth0UserId [nvarchar](100) NOT NULL;
END
GO	
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Auth0UserId'
          AND Object_ID = Object_ID(N'dbo.Supervisors'))
BEGIN
    ALTER TABLE dbo.Supervisors
    ALTER COLUMN Auth0UserId [nvarchar](250) NOT NULL;
END
GO
