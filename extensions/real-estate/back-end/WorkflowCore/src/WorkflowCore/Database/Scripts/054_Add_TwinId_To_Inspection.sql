IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'TwinId'
          AND Object_ID = Object_ID(N'dbo.WF_Inspections'))
BEGIN
    ALTER TABLE dbo.WF_Inspections ADD TwinId [nvarchar](250) NULL;
END
GO
