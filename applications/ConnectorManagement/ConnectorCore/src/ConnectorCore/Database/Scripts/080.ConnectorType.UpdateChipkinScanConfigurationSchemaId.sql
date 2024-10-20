-- Update ChipkinScanConfigurationSchemaId

INSERT INTO [dbo].[Schema] (Id, Name, ClientId, Type)
VALUES ('7c2807f4-8868-4327-a960-4805edd775e9', 'DefaultChipkinScanConfiguration', NULL, NULL);
GO

UPDATE [dbo].[ConnectorType]
SET ScanConfigurationSchemaId = '7c2807f4-8868-4327-a960-4805edd775e9'
WHERE [dbo].[ConnectorType].[Name] = 'DefaultChipkinBACnetConnector';
GO