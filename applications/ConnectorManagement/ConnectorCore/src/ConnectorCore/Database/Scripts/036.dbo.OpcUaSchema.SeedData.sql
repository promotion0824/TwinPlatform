INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('203C392F-BB84-456C-ACA9-2AB7AF7F6595','DefaultOpcUaPointMetadata',NULL,NULL)
INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('6FA46E15-AF91-4564-ADFD-ED90869377AA','DefaultOpcUaDeviceMetadata',NULL,NULL)
INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('41CFE979-4B56-40F4-8AAB-0292D2F96BB2','DefaultOpcUaConnectorConfiguration',NULL,NULL)

INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('EE3DAAA2-61CD-4E03-8A5B-0BB4C9D2DF94','NodeId',1,'String','203C392F-BB84-456C-ACA9-2AB7AF7F6595')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('5176F3C7-CD36-4AD3-9623-292223F1B787','PollingInterval',1,'String','203C392F-BB84-456C-ACA9-2AB7AF7F6595')

INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('3196C10A-CAEF-47B6-A6A8-69B213BD9F91','ThreadsPerNetwork',1,'String','41CFE979-4B56-40F4-8AAB-0292D2F96BB2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('3632DF09-46FE-4E80-B6FB-78BE379FEF3D','Password',1,'String','41CFE979-4B56-40F4-8AAB-0292D2F96BB2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('714AD0EF-91C8-4D56-B3C5-7A419A6C1D70','Timeout',1,'String','41CFE979-4B56-40F4-8AAB-0292D2F96BB2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('5292E636-C142-4659-AFE4-6B1EA13DB46C','MaxDevicesPerThread',1,'String','41CFE979-4B56-40F4-8AAB-0292D2F96BB2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('49D346B4-A294-44DA-A028-C992FBF6E42A','Ip',1,'String','41CFE979-4B56-40F4-8AAB-0292D2F96BB2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('AEB7FDBF-66D9-4195-92FE-C4FA1DBBDC8B','MaxRetry',1,'String','41CFE979-4B56-40F4-8AAB-0292D2F96BB2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('91BE29B2-1CBA-4C7D-9ADA-D35922661608','InitDelay',1,'String','41CFE979-4B56-40F4-8AAB-0292D2F96BB2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('04AB9E6F-E48F-4D68-98E1-F9FA9399D260','Username',1,'String','41CFE979-4B56-40F4-8AAB-0292D2F96BB2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('5F7FDB76-73C0-4716-8BD2-8C4CEA5E0DE1','MaxNumberThreads',1,'String','41CFE979-4B56-40F4-8AAB-0292D2F96BB2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('21730086-4441-4AAA-9009-9186C3A3D768','Port',1,'String','41CFE979-4B56-40F4-8AAB-0292D2F96BB2')

INSERT INTO [dbo].[ConnectorCategory] ([Id], [Name]) VALUES ('AC24A46D-A0CF-4DD6-84D4-381D653A747E','DefaultOpcUa')

INSERT INTO [dbo].[ConnectorType]([Id], [Name], [ConnectorConfigurationSchemaId], [DeviceMetadataSchemaId], [PointMetadataSchemaId], [ConnectorCategoryId]) VALUES ('0F4D657A-A908-4F69-B214-79C0FF5B1E95','DefaultOpcUaConnector','41CFE979-4B56-40F4-8AAB-0292D2F96BB2','6FA46E15-AF91-4564-ADFD-ED90869377AA','203C392F-BB84-456C-ACA9-2AB7AF7F6595','AC24A46D-A0CF-4DD6-84D4-381D653A747E')
