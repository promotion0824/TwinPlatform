IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'TwinName'
          AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights ADD TwinName [nvarchar](256) NULL;
END
GO	

