-- New VergeSense connector type
IF NOT EXISTS (SELECT * FROM [dbo].[Schema] WHERE Id = 'e51b3861-4d11-4cef-9db6-d0189030178a')
BEGIN
	INSERT INTO [dbo].[Schema] ([Id], [Name]) VALUES ('e51b3861-4d11-4cef-9db6-d0189030178a','DefaultVergeSensePointMetadata')
END

IF NOT EXISTS (SELECT * FROM [dbo].[Schema] WHERE Id = '5ec236a7-ddd7-468b-9bd9-a7ff0b6929c7')
BEGIN
	INSERT INTO [dbo].[Schema] ([Id], [Name]) VALUES ('5ec236a7-ddd7-468b-9bd9-a7ff0b6929c7','DefaultVergeSenseDeviceMetadata')
END

IF NOT EXISTS (SELECT * FROM [dbo].[Schema] WHERE Id = '30b72f42-e586-4ce1-a5f0-6f5c4a6d79e6')
BEGIN
	INSERT INTO [dbo].[Schema] ([Id], [Name]) VALUES ('30b72f42-e586-4ce1-a5f0-6f5c4a6d79e6','DefaultVergeSenseConnectorConfiguration')
END

IF NOT EXISTS (SELECT * FROM [dbo].[ConnectorType] WHERE Id = '6527155b-909f-4c2b-b7eb-b619b7164814')
BEGIN
	INSERT INTO [dbo].[ConnectorType] ([Id],[Name],[ConnectorConfigurationSchemaId],[DeviceMetadataSchemaId],[PointMetadataSchemaId])
	VALUES ('6527155b-909f-4c2b-b7eb-b619b7164814','DefaultVergeSenseConnector','30b72f42-e586-4ce1-a5f0-6f5c4a6d79e6','5ec236a7-ddd7-468b-9bd9-a7ff0b6929c7','e51b3861-4d11-4cef-9db6-d0189030178a')
END