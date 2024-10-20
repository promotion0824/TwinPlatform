ALTER TABLE [dbo].[WF_Inspections]
ADD [SortOrder] int NOT NULL CONSTRAINT SortOrderDefault DEFAULT 0;
GO

ALTER TABLE [dbo].[WF_Inspections]
    DROP SortOrderDefault
GO