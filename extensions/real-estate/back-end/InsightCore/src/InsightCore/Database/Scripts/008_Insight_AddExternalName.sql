IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ExternalName'
          AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights ADD ExternalName [nvarchar](128) NULL;
END
GO	

