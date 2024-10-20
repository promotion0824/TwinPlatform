IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'IsActive'
          AND Object_ID = Object_ID(N'dbo.WF_TicketCategory'))
BEGIN
    ALTER TABLE dbo.WF_TicketCategory ADD IsActive [bit] NOT NULL DEFAULT 1;
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'LastUpdate'
          AND Object_ID = Object_ID(N'dbo.WF_TicketCategory'))
BEGIN
    ALTER TABLE dbo.WF_TicketCategory ADD LastUpdate [datetime2] NOT NULL DEFAULT GETUTCDATE();
END
GO


IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'CategoryId'
          AND Object_ID = Object_ID(N'dbo.WF_ServiceNeeded'))
BEGIN
    ALTER TABLE dbo.WF_ServiceNeeded ADD CategoryId [UNIQUEIDENTIFIER] NULL;
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'IsActive'
          AND Object_ID = Object_ID(N'dbo.WF_ServiceNeeded'))
BEGIN
    ALTER TABLE dbo.WF_ServiceNeeded ADD IsActive [bit] NOT NULL DEFAULT 1;
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'LastUpdate'
          AND Object_ID = Object_ID(N'dbo.WF_ServiceNeeded'))
BEGIN
    ALTER TABLE dbo.WF_ServiceNeeded ADD LastUpdate [datetime2] NOT NULL DEFAULT GETUTCDATE();
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'IsActive'
          AND Object_ID = Object_ID(N'dbo.WF_JobTypes'))
BEGIN
    ALTER TABLE dbo.WF_JobTypes ADD IsActive [bit] NOT NULL DEFAULT 1;
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'LastUpdate'
          AND Object_ID = Object_ID(N'dbo.WF_JobTypes'))
BEGIN
    ALTER TABLE dbo.WF_JobTypes ADD LastUpdate [datetime2] NOT NULL DEFAULT GETUTCDATE();
END
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'LastUpdate'
          AND Object_ID = Object_ID(N'dbo.WF_ServiceNeededSpaceTwin'))
BEGIN
    ALTER TABLE dbo.WF_ServiceNeededSpaceTwin ADD LastUpdate [datetime2] NOT NULL DEFAULT GETUTCDATE();
END
GO

