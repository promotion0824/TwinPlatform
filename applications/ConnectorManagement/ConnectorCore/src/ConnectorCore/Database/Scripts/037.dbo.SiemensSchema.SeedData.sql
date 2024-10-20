INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('4F0E0D97-3DDF-49DB-B114-B55BFB077FA4','DefaultSiemensPointMetadata',NULL,NULL)
INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('0F086C54-6E01-4962-87E6-9769AF7AD83A','DefaultSiemensDeviceMetadata',NULL,NULL)
INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('E73E8E7A-BD79-4813-ACFF-51C8E51500A3','DefaultSiemensConnectorConfiguration',NULL,NULL)

INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('13A85958-E53D-4AC1-ABDC-1AA662368B2C','NodeId',1,'String','4F0E0D97-3DDF-49DB-B114-B55BFB077FA4')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('269779E9-A4CE-4955-8E38-3FE20686BAFB','PollingInterval',1,'String','4F0E0D97-3DDF-49DB-B114-B55BFB077FA4')

INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('B0CEA359-8084-49D0-A80C-8070E3F61255','ThreadsPerNetwork',1,'String','E73E8E7A-BD79-4813-ACFF-51C8E51500A3')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('7B1D54F4-0684-4BBC-B8D4-0BB725B1C8E1','Password',1,'String','E73E8E7A-BD79-4813-ACFF-51C8E51500A3')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('393F2FAE-BBEB-49F8-9F77-E636761E8356','Timeout',1,'String','E73E8E7A-BD79-4813-ACFF-51C8E51500A3')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('BE0F6147-FB14-480D-9EEB-F7181DA2740D','MaxDevicesPerThread',1,'String','E73E8E7A-BD79-4813-ACFF-51C8E51500A3')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('711194A4-8A50-4464-B0FE-1BCEA6205B65','Url',1,'String','E73E8E7A-BD79-4813-ACFF-51C8E51500A3')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('E158CCED-C35F-4375-BD4B-1DDD3F407DD1','MaxRetry',1,'String','E73E8E7A-BD79-4813-ACFF-51C8E51500A3')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('7D1A24A4-1938-431D-83B3-7E04700AB719','InitDelay',1,'String','E73E8E7A-BD79-4813-ACFF-51C8E51500A3')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('3DF6A51D-19C1-4AB8-8D32-19B9C5E0A2AD','Username',1,'String','E73E8E7A-BD79-4813-ACFF-51C8E51500A3')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('6CA7EB9E-7F39-4D70-927B-8B292E2DFACB','MaxNumberThreads',1,'String','E73E8E7A-BD79-4813-ACFF-51C8E51500A3')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('2013F939-62EA-4F45-8D2E-299867EF4CD3','Port',1,'String','E73E8E7A-BD79-4813-ACFF-51C8E51500A3')

INSERT INTO [dbo].[ConnectorCategory] ([Id], [Name]) VALUES ('6FF8BAEF-B853-432F-B660-C127063EEBA0','DefaultSiemens')

INSERT INTO [dbo].[ConnectorType]([Id], [Name], [ConnectorConfigurationSchemaId], [DeviceMetadataSchemaId], [PointMetadataSchemaId], [ConnectorCategoryId]) VALUES ('E9FFF2DE-414F-48FB-BD0C-64133E4BC757','DefaultSiemensConnector','E73E8E7A-BD79-4813-ACFF-51C8E51500A3','0F086C54-6E01-4962-87E6-9769AF7AD83A','4F0E0D97-3DDF-49DB-B114-B55BFB077FA4','6FF8BAEF-B853-432F-B660-C127063EEBA0')
