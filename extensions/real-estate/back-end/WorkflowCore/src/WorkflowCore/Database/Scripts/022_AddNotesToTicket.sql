ALTER TABLE [dbo].[WF_Ticket] 
    ADD [Notes] [nvarchar](1000) NOT NULL CONSTRAINT NotesDefault DEFAULT '';
GO

ALTER TABLE [dbo].[WF_Ticket]
    DROP NotesDefault
GO