ALTER TABLE [dbo].[WF_Ticket] 
ADD [CreatorId] [uniqueidentifier] NOT NULL CONSTRAINT CreatorIdDefault DEFAULT '00000000-0000-0000-0000-000000000000';
GO

ALTER TABLE [dbo].[WF_Ticket]
    DROP CreatorIdDefault
GO
