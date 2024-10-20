-- Category

insert into [dbo].[ConnectorCategory]([Id], [Name])
select 'a8ee6b43-99da-4ad6-8f26-365cdbfded5d', 'DefaultPointGrab'
where not exists(select 1 from [dbo].[ConnectorCategory] cc where cc.[Id] = 'a8ee6b43-99da-4ad6-8f26-365cdbfded5d');
GO


-- Schemas

insert into [dbo].[Schema]([Id], [Name])
select 'd95c7d29-639c-4593-b5de-bc4af13ab650', 'DefaultPointgrabConnectorConfiguration'
where not exists (select 1 from [dbo].[Schema] s where s.[Id] = 'd95c7d29-639c-4593-b5de-bc4af13ab650');
GO

insert into [dbo].[Schema]([Id], [Name])
select 'c5d57621-eaa6-4151-b956-900ff514c4ef', 'DefaultPointgrabDeviceMetadata'
where not exists (select 1 from [dbo].[Schema] s where s.[Id] = 'c5d57621-eaa6-4151-b956-900ff514c4ef');
GO

insert into [dbo].[Schema]([Id], [Name])
select '3299f0db-865d-4737-9f1e-4879586b5f1e', 'DefaultPointgrabPointMetadata'
where not exists (select 1 from [dbo].[Schema] s where s.[Id] = '3299f0db-865d-4737-9f1e-4879586b5f1e');
GO


-- Connector schema columns

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '64939a23-f397-4e5c-9e20-69c9b3e94116', 'Url', 1, 'string', 'd95c7d29-639c-4593-b5de-bc4af13ab650'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '64939a23-f397-4e5c-9e20-69c9b3e94116');
GO

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '69aabeee-53c8-426d-8a63-1b84bea70a63', 'Username', 1, 'string', 'd95c7d29-639c-4593-b5de-bc4af13ab650'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '69aabeee-53c8-426d-8a63-1b84bea70a63');
GO

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '4234f558-6c39-4c23-bcc8-e66718943a06', 'Password', 1, 'string', 'd95c7d29-639c-4593-b5de-bc4af13ab650'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '4234f558-6c39-4c23-bcc8-e66718943a06');
GO


-- Device schema columns

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '3171c942-a245-4500-af98-5e5f9c8ba1bb', 'FwVersion', 0, 'string', 'c5d57621-eaa6-4151-b956-900ff514c4ef'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '3171c942-a245-4500-af98-5e5f9c8ba1bb');
GO

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '57f9b5af-44c6-4a7e-a97b-4ec93e88ca53', 'GeoPosition', 0, 'string', 'c5d57621-eaa6-4151-b956-900ff514c4ef'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '57f9b5af-44c6-4a7e-a97b-4ec93e88ca53');
GO

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '9d1c77c1-3355-4089-ad49-d84e4cf015e7', 'MetricPosition', 0, 'string', 'c5d57621-eaa6-4151-b956-900ff514c4ef'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '9d1c77c1-3355-4089-ad49-d84e4cf015e7');
GO

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '7a561cd7-05d8-40c6-8b0d-454e8c3f83d5', 'Height', 0, 'string', 'c5d57621-eaa6-4151-b956-900ff514c4ef'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '7a561cd7-05d8-40c6-8b0d-454e8c3f83d5');
GO

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '20429035-23de-482a-8129-d074740e34b1', 'Rotation', 0, 'string', 'c5d57621-eaa6-4151-b956-900ff514c4ef'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '20429035-23de-482a-8129-d074740e34b1');
GO

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '991cb1bd-c279-48ad-b43b-6f3f02f1ad9d', 'SerialNo', 0, 'string', 'c5d57621-eaa6-4151-b956-900ff514c4ef'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '991cb1bd-c279-48ad-b43b-6f3f02f1ad9d');
GO

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '2bc51d11-72d6-45c3-b77e-6fa5742dde59', 'AttachmentState', 0, 'string', 'c5d57621-eaa6-4151-b956-900ff514c4ef'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '2bc51d11-72d6-45c3-b77e-6fa5742dde59');
GO

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '4aa7a38c-0541-400e-b4b8-50bd8e9d1ab9', 'ConnectionStatus', 0, 'string', 'c5d57621-eaa6-4151-b956-900ff514c4ef'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '4aa7a38c-0541-400e-b4b8-50bd8e9d1ab9');
GO

-- Point schema columns

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '39150a9c-63cb-4bbd-92e4-8e15e77188e6', 'AreaId', 1, 'string', '3299f0db-865d-4737-9f1e-4879586b5f1e'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '39150a9c-63cb-4bbd-92e4-8e15e77188e6');
GO

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select '5a70100e-612e-4bd9-b968-e6f9f0512803', 'DeviceId', 1, 'string', '3299f0db-865d-4737-9f1e-4879586b5f1e'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = '5a70100e-612e-4bd9-b968-e6f9f0512803');
GO

insert into [dbo].[SchemaColumn]([Id], [Name], [IsRequired], [DataType], [SchemaId])
select 'ee370e87-91b4-4dcb-b208-29afc5086804', 'FieldName', 1, 'string', '3299f0db-865d-4737-9f1e-4879586b5f1e'
where not exists (select 1 from [dbo].[SchemaColumn] sc where sc.[Id] = 'ee370e87-91b4-4dcb-b208-29afc5086804');
GO

-- ConnectorType

insert into [dbo].[ConnectorType]([Id], [Name], [ConnectorConfigurationSchemaId], [DeviceMetadataSchemaId], [PointMetadataSchemaId], [ConnectorCategoryId])
select 'C9E2914A-2440-4AF4-99FB-5235A1EF994B', 'DefaultPointGrabConnector', 'd95c7d29-639c-4593-b5de-bc4af13ab650', 'c5d57621-eaa6-4151-b956-900ff514c4ef', '3299f0db-865d-4737-9f1e-4879586b5f1e', 'a8ee6b43-99da-4ad6-8f26-365cdbfded5d'
where not exists(select 1 from [dbo].[ConnectorType] ct where ct.[Id] = 'C9E2914A-2440-4AF4-99FB-5235A1EF994B');
GO