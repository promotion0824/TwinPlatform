UPDATE [dbo].[ConnectorType] set [Name] = 'DefaultSiemensConnector(deprecated)' where [Id] = 'E9FFF2DE-414F-48FB-BD0C-64133E4BC757';
GO

INSERT INTO [dbo].[ConnectorType]([Id], [Name], [ConnectorConfigurationSchemaId], [DeviceMetadataSchemaId], [PointMetadataSchemaId], [ConnectorCategoryId]) 
VALUES ('872C0D45-40D9-4B74-8A59-FA4B9F657337','DefaultSiemensConnector','E73E8E7A-BD79-4813-ACFF-51C8E51500A3','0F086C54-6E01-4962-87E6-9769AF7AD83A','4F0E0D97-3DDF-49DB-B114-B55BFB077FA4','6FF8BAEF-B853-432F-B660-C127063EEBA0');
GO
