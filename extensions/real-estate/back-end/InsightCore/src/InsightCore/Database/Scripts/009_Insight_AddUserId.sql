IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'CreatedUserId'
          AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights ADD CreatedUserId uniqueidentifier NULL;
END
GO	

