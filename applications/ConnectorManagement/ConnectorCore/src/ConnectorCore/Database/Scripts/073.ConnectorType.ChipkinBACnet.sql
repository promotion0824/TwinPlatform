-- New Chipkin BACnet Connector type
-- Reuse existing DeviceMetadata and PointMetadata schema for now
IF NOT EXISTS (SELECT * FROM [dbo].[Schema] WHERE Id = '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7')
    BEGIN
        INSERT INTO [dbo].[Schema] ([Id], [Name]) VALUES ('8E3DE645-3C5C-443F-AFD8-27E90F64C9A7','DefaultChipkinConnectorConfiguration')
    END

IF NOT EXISTS (SELECT * FROM [dbo].[ConnectorType] WHERE Id = '9EF2AE5C-2536-42F9-9E5E-4B9313EEECA8')
    BEGIN
        INSERT INTO [dbo].[ConnectorType] ([Id],[Name],[ConnectorConfigurationSchemaId],[DeviceMetadataSchemaId],[PointMetadataSchemaId])
        VALUES ('9EF2AE5C-2536-42F9-9E5E-4B9313EEECA8', 'DefaultChipkinBACnetConnector', '8E3DE645-3C5C-443F-AFD8-27E90F64C9A7', '0D5608FA-E06F-4BEB-AC13-8A5F83FC3A5A', '86CB421A-BD47-4D09-B78E-3ED88976D9B9')
    END