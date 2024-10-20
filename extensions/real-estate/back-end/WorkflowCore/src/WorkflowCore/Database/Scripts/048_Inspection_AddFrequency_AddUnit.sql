IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Frequency'
          AND Object_ID = Object_ID(N'dbo.WF_Inspections'))
BEGIN
    ALTER TABLE [dbo].[WF_Inspections] ADD [Frequency] INT NOT NULL CONSTRAINT FrequencyDefault DEFAULT 0; 
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'FrequencyUnit'
          AND Object_ID = Object_ID(N'dbo.WF_Inspections'))
BEGIN
    ALTER TABLE [dbo].[WF_Inspections] ADD [FrequencyUnit] varchar(20) NOT NULL CONSTRAINT FrequencyUnitDefault DEFAULT 'Hours'; 
END

ALTER TABLE [dbo].[WF_Inspections]
ADD CONSTRAINT FrequencyInHours_Defaults
DEFAULT 0 FOR FrequencyInHours;