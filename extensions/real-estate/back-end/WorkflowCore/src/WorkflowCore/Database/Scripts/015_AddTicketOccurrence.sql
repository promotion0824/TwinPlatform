-- Add CategoryId
ALTER TABLE [dbo].[WF_Ticket]
  ADD [IsTemplate] bit NOT NULL CONSTRAINT IsTemplateDefault DEFAULT 0;
GO

ALTER TABLE [dbo].[WF_Ticket]
  ADD [Occurrence] int NOT NULL CONSTRAINT OccurrenceDefault DEFAULT 0;
GO

