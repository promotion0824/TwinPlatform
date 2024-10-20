ALTER TABLE [dbo].[Insights] 
ADD [OccurrenceCount] int NOT NULL CONSTRAINT OccurrenceCountDefault DEFAULT 1;

ALTER TABLE [dbo].[Insights] DROP CONSTRAINT OccurrenceCountDefault;

EXEC sp_rename 'Insights.OccurredDate', 'LastOccurredDate', 'COLUMN';