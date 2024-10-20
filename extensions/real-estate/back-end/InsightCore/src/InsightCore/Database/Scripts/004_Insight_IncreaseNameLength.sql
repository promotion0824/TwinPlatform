IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Name'
          AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights
    ALTER COLUMN Name [nvarchar](256) NOT NULL;
END
GO	