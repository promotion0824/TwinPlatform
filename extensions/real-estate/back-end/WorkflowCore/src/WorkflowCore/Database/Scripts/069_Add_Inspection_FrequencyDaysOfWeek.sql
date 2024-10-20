IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'FrequencyDaysOfWeekJson'
          AND Object_ID = Object_ID(N'dbo.WF_Inspections'))
BEGIN
    ALTER TABLE [dbo].[WF_Inspections] ADD [FrequencyDaysOfWeekJson]  nvarchar(100) NULL; 
END

