IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ReporterName'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE [dbo].[WF_Ticket] ALTER COLUMN [ReporterName] [nvarchar](500) NOT NULL;
END

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Name'
          AND Object_ID = Object_ID(N'dbo.WF_Reporter'))
BEGIN
    ALTER TABLE [dbo].[WF_Reporter] ALTER COLUMN [Name] [nvarchar](500) NOT NULL;
END

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ReporterName'
          AND Object_ID = Object_ID(N'dbo.WF_TicketTemplate'))
BEGIN
    ALTER TABLE [dbo].[WF_TicketTemplate] ALTER COLUMN [ReporterName] [nvarchar](500) NOT NULL;
END



