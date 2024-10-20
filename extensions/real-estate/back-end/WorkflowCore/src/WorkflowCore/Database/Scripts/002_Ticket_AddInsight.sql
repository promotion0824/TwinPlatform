ALTER TABLE [dbo].[WF_Ticket] 
ADD [InsightId] [uniqueidentifier] NULL;
GO

ALTER TABLE [dbo].[WF_Ticket] 
ADD [InsightName] [nvarchar](128) NOT NULL CONSTRAINT InsightNameDefault DEFAULT '';
GO

ALTER TABLE [dbo].[WF_Ticket]
    DROP InsightNameDefault
GO
