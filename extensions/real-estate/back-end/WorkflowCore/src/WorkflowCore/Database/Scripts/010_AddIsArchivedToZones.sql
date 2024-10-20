ALTER TABLE [dbo].[WF_Zones]
ADD [IsArchived] bit NOT NULL CONSTRAINT IsArchivedDefault DEFAULT 'False';
GO

ALTER TABLE [dbo].[WF_Zones]
    DROP IsArchivedDefault
GO