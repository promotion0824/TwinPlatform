IF NOT EXISTS
(
  SELECT 1 FROM sys.indexes
  WHERE name='IX_Insights_OccurrenceCount'  AND object_id = OBJECT_ID('Insights')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Insights_OccurrenceCount 
    ON  Insights (OccurrenceCount) WITH (ONLINE = ON);
END
