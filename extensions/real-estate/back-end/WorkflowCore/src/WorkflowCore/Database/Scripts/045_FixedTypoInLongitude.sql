
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Longitute'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE [dbo].[WF_Ticket] DROP COLUMN [Longitute] 
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Longitude'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE [dbo].[WF_Ticket] ADD [Longitude] NUMERIC(9, 6) NULL; 
END