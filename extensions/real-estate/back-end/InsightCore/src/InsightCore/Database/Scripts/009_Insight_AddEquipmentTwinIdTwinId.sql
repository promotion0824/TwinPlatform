IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'EquipmentTwinId'
          AND Object_ID = Object_ID(N'dbo.Insights'))
BEGIN
    ALTER TABLE dbo.Insights ADD EquipmentTwinId [nvarchar](250) NULL;
END
GO