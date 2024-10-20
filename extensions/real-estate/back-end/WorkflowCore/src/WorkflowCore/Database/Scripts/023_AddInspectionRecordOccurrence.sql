ALTER TABLE [dbo].[WF_InspectionRecords]
  ADD [Occurrence] int NOT NULL CONSTRAINT InspectionRecordOccurrenceDefault DEFAULT 0;
GO

