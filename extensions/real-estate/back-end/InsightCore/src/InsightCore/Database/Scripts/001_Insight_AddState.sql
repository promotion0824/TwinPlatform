ALTER TABLE [dbo].[Insights] 
ADD [State] int NOT NULL CONSTRAINT StateDefault DEFAULT 0;
ALTER TABLE [dbo].[Insights]
    DROP StateDefault
GO