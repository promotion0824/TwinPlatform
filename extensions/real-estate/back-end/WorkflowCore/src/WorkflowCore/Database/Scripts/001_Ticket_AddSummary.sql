ALTER TABLE [dbo].[WF_Ticket] 
ADD [Summary] [nvarchar](512) NOT NULL CONSTRAINT SummaryDefault DEFAULT '';
GO

ALTER TABLE [dbo].[WF_Ticket]
    DROP SummaryDefault
GO

ALTER TABLE [dbo].[WF_Ticket] 
ADD [AssigneeType] [int] NOT NULL CONSTRAINT AssigneeTypeDefault DEFAULT 0;
GO

ALTER TABLE [dbo].[WF_Ticket]
    DROP AssigneeTypeDefault
GO

EXEC sp_rename 'dbo.WF_Ticket.AssigneeContractorId', 'AssigneeId', 'COLUMN';
GO
