IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'SyncedDate'
          AND Object_ID = Object_ID(N'dbo.WF_CheckRecords'))
BEGIN
    ALTER TABLE [dbo].[WF_CheckRecords] ADD [SyncedDate] DATETIME NULL; 
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'SyncedSiteLocalDate'
          AND Object_ID = Object_ID(N'dbo.WF_CheckRecords'))
BEGIN
    ALTER TABLE [dbo].[WF_CheckRecords] ADD [SyncedSiteLocalDate] DATETIME NULL; 
END