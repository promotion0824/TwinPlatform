IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'TypeValue'
          AND Object_ID = Object_ID(N'dbo.WF_CheckRecords'))
BEGIN
    ALTER TABLE [dbo].[WF_CheckRecords] ADD [TypeValue]  nvarchar(512) NULL; 
END

