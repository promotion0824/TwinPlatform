ALTER TABLE [dbo].[WF_Checks]
ADD [CanGenerateInsight] bit NOT NULL CONSTRAINT CanGenerateInsightDefault DEFAULT 1;
GO

ALTER TABLE [dbo].[WF_Checks]
    DROP CanGenerateInsightDefault
GO