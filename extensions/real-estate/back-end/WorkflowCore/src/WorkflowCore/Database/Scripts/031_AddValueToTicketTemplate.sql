ALTER TABLE [dbo].[WF_TicketTemplate]
    ADD [DataValue] [nvarchar](MAX) NOT NULL CONSTRAINT DataValueDefault DEFAULT '{}';
GO

ALTER TABLE [dbo].[WF_TicketTemplate]
    DROP DataValueDefault
GO