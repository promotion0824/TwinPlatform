ALTER TABLE [dbo].[WF_CheckRecords]  DROP CONSTRAINT [FK_WF_CheckRecords_WF_InspectionRecords] 
GO

ALTER TABLE [dbo].[WF_CheckRecords]  WITH CHECK ADD CONSTRAINT [FK_WF_CheckRecords_WF_InspectionRecords] FOREIGN KEY([InspectionRecordId])
REFERENCES [dbo].[WF_InspectionRecords] ([Id]) ON DELETE CASCADE
GO
