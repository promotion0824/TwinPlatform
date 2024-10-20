IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = N'WF_TicketSubStatus')
BEGIN
    CREATE TABLE [dbo].[WF_TicketSubStatus] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(500) NOT NULL,
    )

    CREATE UNIQUE INDEX UX_WF_TicketSubStatus_Name ON [dbo].[WF_TicketSubStatus]([Name])
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'SubStatusId'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE dbo.WF_Ticket
    ADD SubStatusId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Ticket_SubStatus FOREIGN KEY ( SubStatusId) REFERENCES WF_TicketSubStatus(Id);
END


IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'SubStatus'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE dbo.WF_Ticket
    DROP COLUMN SubStatus
   
END
