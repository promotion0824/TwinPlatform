Insert into [dbo].[StatusLog] (Id,InsightId,UserId,SourceType,SourceId,Status,Priority,OccurrenceCount, CreatedDateTime) 
SELECT NewID(),ins.id,ins.CreatedUserId,ins.SourceType,
ins.SourceId,ins.Status, ins.Priority,ins.OccurrenceCount, ins.UpdatedDate
FROM insights ins
LEFT JOIN [dbo].[StatusLog] ON [dbo].[StatusLog].insightid = ins.id
WHERE [dbo].[StatusLog].insightid IS NULL
