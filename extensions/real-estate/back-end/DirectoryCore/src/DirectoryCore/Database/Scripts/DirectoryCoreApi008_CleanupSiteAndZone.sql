IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Address'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [Address];
END
GO

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'State'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [State];
END
GO

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Postcode'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [Postcode];
END
GO

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Country'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [Country];
END
GO


IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'NumberOfFloors'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [NumberOfFloors];
END
GO

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Contact'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [Contact];
END
GO

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ContactNumber'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [ContactNumber];
END
GO

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ContactEmail'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [ContactEmail];
END
GO

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ImageUrl'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [ImageUrl];
END
GO

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Introduction'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [Introduction];
END
GO

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Latitude'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [Latitude];
END
GO

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Longitude'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [Longitude];
END
GO

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Timezone'
          AND Object_ID = Object_ID(N'dbo.Sites'))
BEGIN
    ALTER TABLE dbo.Sites
    DROP COLUMN [Timezone];
END
GO

DROP TABLE Zones
GO
