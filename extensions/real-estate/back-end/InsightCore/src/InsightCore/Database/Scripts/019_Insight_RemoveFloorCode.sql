IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'FloorCode'
          AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights Drop  COLUMN  FloorCode
END
GO	
