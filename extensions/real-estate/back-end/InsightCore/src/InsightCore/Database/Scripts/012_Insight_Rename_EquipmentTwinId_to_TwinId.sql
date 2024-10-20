IF ( NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'TwinId' AND Object_ID = Object_ID(N'dbo.Insights'))
     AND EXISTS(SELECT * FROM sys.columns WHERE Name = N'EquipmentTwinId' AND Object_ID = Object_ID(N'dbo.Insights')) )
BEGIN
    EXEC sp_rename 'dbo.Insights.EquipmentTwinId', 'TwinId', 'COLUMN';
END
