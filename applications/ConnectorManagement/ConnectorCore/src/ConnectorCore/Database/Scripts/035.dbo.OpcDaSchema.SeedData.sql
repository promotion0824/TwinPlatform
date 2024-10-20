INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('B6270AA7-294C-411C-94C8-FF2CD1C8268F','DefaultOpcDaPointMetadata',NULL,NULL)
INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('CC26F6CE-25F9-426F-8617-EF98EAC0BA03','DefaultOpcDaDeviceMetadata',NULL,NULL)
INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('8E5F3305-4BF9-4B14-B510-30713FC8A10B','DefaultOpcDaConnectorConfiguration',NULL,NULL)

INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('B81FDA60-0857-4431-8BAC-64DBAF9B71DC','NodeId',1,'String','B6270AA7-294C-411C-94C8-FF2CD1C8268F')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('6430D85E-670E-4C39-B46A-CF8293D413FF','PollingInterval',1,'String','B6270AA7-294C-411C-94C8-FF2CD1C8268F')

INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('F3C9EC7C-E0AE-44F8-A447-9D3984F670E7','ThreadsPerNetwork',1,'String','8E5F3305-4BF9-4B14-B510-30713FC8A10B')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('896385B0-07EF-4582-99C8-6879868DEB27','Password',1,'String','8E5F3305-4BF9-4B14-B510-30713FC8A10B')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('4B37A22B-40CF-4B1B-996F-D62C67697330','Timeout',1,'String','8E5F3305-4BF9-4B14-B510-30713FC8A10B')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('0BEC7014-AE61-4A3C-A6CB-E7D22973A3A7','MaxDevicesPerThread',1,'String','8E5F3305-4BF9-4B14-B510-30713FC8A10B')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('2CE65654-F7E7-4206-B9C7-98DDFADDABA4','Ip',1,'String','8E5F3305-4BF9-4B14-B510-30713FC8A10B')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('EFE6BBAD-1232-4A02-8ABF-60414F8BAB2A','MaxRetry',1,'String','8E5F3305-4BF9-4B14-B510-30713FC8A10B')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('A5EC8C7E-5263-4266-A857-82F83970E2EF','InitDelay',1,'String','8E5F3305-4BF9-4B14-B510-30713FC8A10B')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('49A74C36-FEBB-46FD-AAAC-1D43FB8C19F4','Username',1,'String','8E5F3305-4BF9-4B14-B510-30713FC8A10B')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('138C1878-7D81-4CD9-9B49-5B39974E4D11','MaxNumberThreads',1,'String','8E5F3305-4BF9-4B14-B510-30713FC8A10B')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('AB5342FB-BB43-4674-878B-0A3A55620BA8','Port',1,'String','8E5F3305-4BF9-4B14-B510-30713FC8A10B')

INSERT INTO [dbo].[ConnectorCategory] ([Id], [Name]) VALUES ('F30A20F4-F355-41E0-A330-DE1F9A02EF02','DefaultOpcDa')

INSERT INTO [dbo].[ConnectorType]([Id], [Name], [ConnectorConfigurationSchemaId], [DeviceMetadataSchemaId], [PointMetadataSchemaId], [ConnectorCategoryId]) VALUES ('2112FC9C-F72F-4F5F-A342-9E4A499C452E','DefaultOpcDaConnector','8E5F3305-4BF9-4B14-B510-30713FC8A10B','CC26F6CE-25F9-426F-8617-EF98EAC0BA03','B6270AA7-294C-411C-94C8-FF2CD1C8268F','F30A20F4-F355-41E0-A330-DE1F9A02EF02')
