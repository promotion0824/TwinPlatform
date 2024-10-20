IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Latitude'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE [dbo].[WF_Ticket] ADD [Latitude] NUMERIC(9, 6) NULL; 
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Longitute'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE [dbo].[WF_Ticket] ADD [Longitute] NUMERIC(9, 6) NULL; 
END