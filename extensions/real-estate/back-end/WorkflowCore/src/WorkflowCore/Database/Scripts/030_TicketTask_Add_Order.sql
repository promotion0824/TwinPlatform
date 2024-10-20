ALTER TABLE [dbo].[WF_TicketTask]
ADD [Order] int NOT NULL CONSTRAINT OrderDefault DEFAULT 1;
GO

ALTER TABLE [dbo].[WF_TicketTask]
    DROP OrderDefault
GO