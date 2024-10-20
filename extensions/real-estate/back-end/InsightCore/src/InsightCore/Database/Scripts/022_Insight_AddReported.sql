IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Reported'
          AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights ADD Reported [bit] NOT NULL Default (0);
END
GO	

