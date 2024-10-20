-- New Stream Analytics connector type
IF NOT EXISTS (SELECT * FROM [dbo].[Schema] WHERE Id = '82928B99-10F6-4AD7-BA20-6E20A21291AF')
    BEGIN
        INSERT INTO [dbo].[Schema] ([Id], [Name]) VALUES ('82928B99-10F6-4AD7-BA20-6E20A21291AF','DefaultStreamAnalyticsPointMetadata')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[Schema] WHERE Id = '429549F2-EBBE-44A0-B4CE-5CFE070C1258')
    BEGIN
        INSERT INTO [dbo].[Schema] ([Id], [Name]) VALUES ('429549F2-EBBE-44A0-B4CE-5CFE070C1258','DefaultStreamAnalyticsDeviceMetadata')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[Schema] WHERE Id = '5435C70D-4706-4A06-90D8-7198C215AEB9')
    BEGIN
        INSERT INTO [dbo].[Schema] ([Id], [Name]) VALUES ('5435C70D-4706-4A06-90D8-7198C215AEB9','DefaultStreamAnalyticsConnectorConfiguration')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[ConnectorType] WHERE Id = 'D9D36F2D-7F5B-4C70-8B9D-E87E6B071F6E')
    BEGIN
        INSERT INTO [dbo].[ConnectorType] ([Id],[Name],[ConnectorConfigurationSchemaId],[DeviceMetadataSchemaId],[PointMetadataSchemaId])
        VALUES ('D9D36F2D-7F5B-4C70-8B9D-E87E6B071F6E','DefaultStreamAnalyticsConnector','5435C70D-4706-4A06-90D8-7198C215AEB9','429549F2-EBBE-44A0-B4CE-5CFE070C1258','82928B99-10F6-4AD7-BA20-6E20A21291AF')
    END