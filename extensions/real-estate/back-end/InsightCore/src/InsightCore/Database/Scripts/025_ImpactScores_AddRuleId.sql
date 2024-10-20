IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'RuleId'
          AND Object_ID = Object_ID(N'dbo.ImpactScores'))
BEGIN
    ALTER TABLE dbo.ImpactScores ADD RuleId NVARCHAR(450) NULL;
END
GO

UPDATE s SET s.RuleId = i.RuleId
FROM ImpactScores s 
JOIN Insights i ON s.InsightId = i.Id

