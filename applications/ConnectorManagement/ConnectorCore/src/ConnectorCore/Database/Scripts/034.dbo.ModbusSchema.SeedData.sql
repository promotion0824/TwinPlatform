INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('3D0DFD33-2954-42AB-B8A4-2FADDD185C22','DefaultModbusPointMetadata',NULL,NULL)
INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('D4952C64-73C5-4E67-893F-B5227C73FE2B','DefaultModbusDeviceMetadata',NULL,NULL)
INSERT INTO [dbo].[Schema] ([Id], [Name], [ClientId], [Type]) VALUES ('21E52A57-1DCF-4C13-A0F8-C65B2F2B99D2','DefaultModbusConnectorConfiguration',NULL,NULL)

INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('379A7DDD-ECA3-44EB-84D5-AABE14A35F96','EndianBig',1,'String','3D0DFD33-2954-42AB-B8A4-2FADDD185C22')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('01FBC602-5BD0-4718-9ED9-3C4189B2E586','RegisterAddress',1,'String','3D0DFD33-2954-42AB-B8A4-2FADDD185C22')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('9928ED64-B25C-4ACD-8743-9568BDC85B2E','SlaveId',1,'String','3D0DFD33-2954-42AB-B8A4-2FADDD185C22')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('A933A8E2-1327-4EA1-A243-3D9E17D56C60','Swap',1,'String','3D0DFD33-2954-42AB-B8A4-2FADDD185C22')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('0DE4C336-3425-45C1-80C2-3556AA464A6F','DataType',1,'String','3D0DFD33-2954-42AB-B8A4-2FADDD185C22')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('FF8AC561-AE80-4A99-8F18-F58E71DC0185','Scale',1,'String','3D0DFD33-2954-42AB-B8A4-2FADDD185C22')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('D01B40F8-2EA8-4A39-9157-4ABF532B186A','PollingInterval',1,'String','3D0DFD33-2954-42AB-B8A4-2FADDD185C22')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('9362655A-1433-4993-BB73-E3EE72B46544','RegisterType',1,'String','3D0DFD33-2954-42AB-B8A4-2FADDD185C22')

INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('F1EA6CA2-6116-49F6-BA8D-DEAA74058BD3','IpAddress',1,'String','D4952C64-73C5-4E67-893F-B5227C73FE2B')

INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('9C305624-324B-45EC-BF19-3D4CA606D66F','MaxRetry',1,'String','21E52A57-1DCF-4C13-A0F8-C65B2F2B99D2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('0CD7417C-356C-48A6-B23F-462F46C7B8F3','MaxNumberThreads',1,'String','21E52A57-1DCF-4C13-A0F8-C65B2F2B99D2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('28D7C3DA-CB24-4310-B3FC-22AD1CC5BCBC','MaxDevicesPerThread',1,'String','21E52A57-1DCF-4C13-A0F8-C65B2F2B99D2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('F8DDAC53-D640-4408-B245-2BA3DA45F771','Timeout',1,'String','21E52A57-1DCF-4C13-A0F8-C65B2F2B99D2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('B978CC14-9EF6-4AAA-83DA-3179D767D26C','ThreadsPerNetwork',1,'String','21E52A57-1DCF-4C13-A0F8-C65B2F2B99D2')
INSERT INTO [dbo].[SchemaColumn] ([Id], [Name], [IsRequired], [DataType] ,[SchemaId]) VALUES ('41772D03-2ACB-4BF2-B67A-566F887561A5','InitDelay',1,'String','21E52A57-1DCF-4C13-A0F8-C65B2F2B99D2')

INSERT INTO [dbo].[ConnectorCategory] ([Id], [Name]) VALUES ('240300E9-5EAF-4706-A4E6-04118137B323','DefaultModbus')

INSERT INTO [dbo].[ConnectorType]([Id], [Name], [ConnectorConfigurationSchemaId], [DeviceMetadataSchemaId], [PointMetadataSchemaId], [ConnectorCategoryId]) VALUES ('028D6FCD-DF1A-4F6D-BDE1-4617A7F7A96B','DefaultModbusConnector','21E52A57-1DCF-4C13-A0F8-C65B2F2B99D2','D4952C64-73C5-4E67-893F-B5227C73FE2B','3D0DFD33-2954-42AB-B8A4-2FADDD185C22','240300E9-5EAF-4706-A4E6-04118137B323')
