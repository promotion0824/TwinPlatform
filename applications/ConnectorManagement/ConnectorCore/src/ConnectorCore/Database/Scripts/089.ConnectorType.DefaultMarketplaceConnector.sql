IF NOT EXISTS (SELECT * FROM [dbo].[ConnectorType] WHERE [Name] = 'DefaultMarketplaceConnector')
BEGIN

	IF NOT EXISTS (SELECT * FROM [dbo].[Schema] WHERE [Name] = 'DefaultMarketplaceConnectorConfiguration')
	BEGIN
		INSERT INTO [dbo].[Schema] ([Id], [Name]) VALUES (NEWID(), 'DefaultMarketplaceConnectorConfiguration')
	END

	IF NOT EXISTS (SELECT * FROM [dbo].[Schema] WHERE [Name] = 'DefaultMarketplaceConnectorDeviceMetadata')
	BEGIN
		INSERT INTO [dbo].[Schema] ([Id], [Name]) VALUES (NEWID(), 'DefaultMarketplaceConnectorDeviceMetadata')
	END

	IF NOT EXISTS (SELECT * FROM [dbo].[Schema] WHERE [Name] = 'DefaultMarketplacePointMetadata')
	BEGIN
		INSERT INTO [dbo].[Schema] ([Id], [Name]) VALUES (NEWID(), 'DefaultMarketplacePointMetadata')
	END

	IF NOT EXISTS (SELECT * FROM [dbo].[Schema] WHERE [Name] = 'DefaultMarketplaceScanConfiguration')
	BEGIN
		INSERT INTO [dbo].[Schema] ([Id], [Name]) VALUES (NEWID(), 'DefaultMarketplaceScanConfiguration')
	END
	
	DECLARE @ConnectorConfigurationSchemaId uniqueidentifier = (SELECT TOP 1 [Id] FROM [dbo].[Schema] WHERE [Name] = 'DefaultMarketplaceConnectorConfiguration')
	DECLARE @DeviceMetadataSchemaId uniqueidentifier = (SELECT TOP 1 [Id] FROM [dbo].[Schema] WHERE [Name] = 'DefaultMarketplaceConnectorDeviceMetadata')
	DECLARE @PointMetadataSchemaId uniqueidentifier = (SELECT TOP 1 [Id] FROM [dbo].[Schema] WHERE [Name] = 'DefaultMarketplacePointMetadata')
	DECLARE @ScanConfigurationSchemaId uniqueidentifier = (SELECT TOP 1 [Id] FROM [dbo].[Schema] WHERE [Name] = 'DefaultMarketplaceScanConfiguration')

	INSERT INTO [dbo].[ConnectorType] ([Id], [Name], [ConnectorConfigurationSchemaId], [DeviceMetadataSchemaId], [PointMetadataSchemaId], [ScanConfigurationSchemaId])
	VALUES (NEWID(), 'DefaultMarketplaceConnector', @ConnectorConfigurationSchemaId, @DeviceMetadataSchemaId, @PointMetadataSchemaId, @ScanConfigurationSchemaId)

END
