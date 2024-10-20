ALTER TABLE [dbo].[WF_CheckRecords]
    ADD [Attachments] [nvarchar](MAX) NOT NULL CONSTRAINT AttachmentsDefault DEFAULT '[]';
GO

ALTER TABLE [dbo].[WF_CheckRecords]
    DROP AttachmentsDefault
GO