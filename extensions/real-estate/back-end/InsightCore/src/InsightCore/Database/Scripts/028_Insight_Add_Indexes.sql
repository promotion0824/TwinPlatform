IF NOT EXISTS
(
  SELECT 1 FROM sys.indexes
  WHERE name='IX_Dependencies_FromInsightId'  AND object_id = OBJECT_ID('Dependencies')
)
BEGIN

    CREATE NONCLUSTERED INDEX IX_Dependencies_FromInsightId 
    ON  Dependencies (FromInsightId DESC) Include (Relationship, ToInsightId)
END

IF NOT EXISTS
(
  SELECT 1 FROM sys.indexes
  WHERE name='IX_InsightOccurrences_InsightId'  AND object_id = OBJECT_ID('InsightOccurrences')
)
BEGIN

    CREATE NONCLUSTERED INDEX IX_InsightOccurrences_InsightId 
    ON  InsightOccurrences (InsightId DESC) Include (Ended, IsFaulted, IsValid, OccurrenceId, Started, Text)
END



