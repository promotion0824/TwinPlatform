IF NOT EXISTS
(
  SELECT 1 FROM sys.indexes
  WHERE name='IX_ImpactScores_InsightId'  AND object_id = OBJECT_ID('ImpactScores')
)
BEGIN

    CREATE NONCLUSTERED INDEX IX_ImpactScores_InsightId 
    ON  ImpactScores (InsightId DESC) Include (FieldId, Name, Unit, Value)
END

IF NOT EXISTS
(
  SELECT 1 FROM sys.indexes
  WHERE name='IX_StatusLog_InsightId'  AND object_id = OBJECT_ID('StatusLog')
)
BEGIN

    CREATE NONCLUSTERED INDEX IX_StatusLog_InsightId 
    ON  StatusLog (InsightId DESC)
END

IF NOT EXISTS
(
  SELECT 1 FROM sys.indexes
  WHERE name='IX_InsightOccurrences_InsightId'  AND object_id = OBJECT_ID('InsightOccurrences')
)
BEGIN

    CREATE NONCLUSTERED INDEX IX_InsightOccurrences_InsightId 
    ON  InsightOccurrences (InsightId DESC)
END

IF NOT EXISTS
(
  SELECT 1 FROM sys.indexes
  WHERE name='IX_Insights_SiteId_Status'  AND object_id = OBJECT_ID('Insights')
)
BEGIN

    CREATE NONCLUSTERED INDEX IX_Insights_SiteId_Status 
    ON  Insights (SiteId,Status)
END


