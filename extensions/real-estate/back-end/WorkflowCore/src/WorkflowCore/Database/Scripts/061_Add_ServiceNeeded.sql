
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = N'WF_ServiceNeeded')
BEGIN
    CREATE TABLE [dbo].[WF_ServiceNeeded] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(500) NOT NULL,
    )

    CREATE UNIQUE INDEX UX_WF_ServiceNeeded_Name ON [dbo].[WF_ServiceNeeded]([Name])
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = N'WF_ServiceNeededSpaceTwin')
BEGIN
    CREATE TABLE [dbo].[WF_ServiceNeededSpaceTwin] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [ServiceNeededId] UNIQUEIDENTIFIER NOT NULL,
        [SpaceTwinId] NVARCHAR(250) NOT NULL,
        CONSTRAINT [FK_ServiceNeededSpaceTwin_ServiceNeeded] FOREIGN KEY (ServiceNeededId) REFERENCES WF_ServiceNeeded(Id)
    )
     
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ServiceNeededId'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE dbo.WF_Ticket
    ADD ServiceNeededId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Ticket_ServiceNeeded FOREIGN KEY (ServiceNeededId) REFERENCES WF_ServiceNeeded(Id);
END
