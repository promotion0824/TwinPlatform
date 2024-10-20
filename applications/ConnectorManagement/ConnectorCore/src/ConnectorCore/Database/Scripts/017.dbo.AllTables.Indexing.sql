ALTER TABLE [Point] ALTER COLUMN [ExternalPointId] nvarchar(256) NOT NULL;
GO
create index [Idx_Connector_SiteId] on [Connector]([SiteId]);
GO
create index [Idx_Connector_ConnectorTypeId] on [Connector]([ConnectorTypeId]);
GO
create index [Idx_ConnectorType_ConnectorConfigurationSchemaId] on [ConnectorType]([ConnectorConfigurationSchemaId]);
GO
create index [Idx_ConnectorType_DeviceMetadataSchemaId] on [ConnectorType]([DeviceMetadataSchemaId]);
GO
create index [Idx_ConnectorType_PointMetadataSchemaId] on [ConnectorType]([PointMetadataSchemaId]);
GO
create index [Idx_ConnectorType_ConnectorCategoryId] on [ConnectorType]([ConnectorCategoryId]);
GO
create index [Idx_Device_SiteId] on [Device]([SiteId]);
GO
create index [Idx_Device_ConnectorId] on [Device]([ConnectorId]);
GO
create index [Idx_Equipment_SiteId] on [Equipment]([SiteId]);
GO
create index [Idx_Logs_ConnectorId] on [Logs]([ConnectorId]);
GO
create index [Idx_Point_SiteId] on [Point]([SiteId]);
GO
create index [Idx_Point_ExternalPointId] on [Point]([ExternalPointId]);
GO
create index [Idx_Point_DeviceId] on [Point]([DeviceId]);
GO
create index [Idx_Point_Type] on [Point]([Type]);
GO
create index [Idx_SchemaColumn_SchemaId] on [SchemaColumn]([SchemaId]);
GO