IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Description'
          AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights
    ALTER COLUMN Description [nvarchar](1024) NOT NULL;
END
GO	