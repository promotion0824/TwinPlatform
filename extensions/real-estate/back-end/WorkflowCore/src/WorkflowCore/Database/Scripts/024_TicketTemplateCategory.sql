
/*************************************************/
ALTER TABLE [dbo].[WF_TicketTemplate]
  ADD [CategoryName] [nvarchar](1024) NULL;
GO

ALTER TABLE [dbo].[WF_TicketTemplate]
  ADD [CategoryId] uniqueidentifier NULL;
GO


