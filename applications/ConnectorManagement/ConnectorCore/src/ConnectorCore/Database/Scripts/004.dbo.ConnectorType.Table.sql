SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConnectorType](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[ConnectorConfigurationSchemaId] [uniqueidentifier] NOT NULL,
	[DeviceMetadataSchemaId] [uniqueidentifier] NOT NULL,
	[PointMetadataSchemaId] [uniqueidentifier] NOT NULL,
	[ConnectorCategoryId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_ConnectorType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ConnectorType]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorType_ConnectorCategory] FOREIGN KEY([ConnectorCategoryId])
REFERENCES [dbo].[ConnectorCategory] ([Id])
GO
ALTER TABLE [dbo].[ConnectorType] CHECK CONSTRAINT [FK_ConnectorType_ConnectorCategory]
GO
ALTER TABLE [dbo].[ConnectorType]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorType_ConnectorConfigurationSchemaId_Schema] FOREIGN KEY([ConnectorConfigurationSchemaId])
REFERENCES [dbo].[Schema] ([Id])
GO
ALTER TABLE [dbo].[ConnectorType] CHECK CONSTRAINT [FK_ConnectorType_ConnectorConfigurationSchemaId_Schema]
GO
ALTER TABLE [dbo].[ConnectorType]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorType_DeviceMetadataSchemaId_Schema] FOREIGN KEY([DeviceMetadataSchemaId])
REFERENCES [dbo].[Schema] ([Id])
GO
ALTER TABLE [dbo].[ConnectorType] CHECK CONSTRAINT [FK_ConnectorType_DeviceMetadataSchemaId_Schema]
GO
ALTER TABLE [dbo].[ConnectorType]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorType_PointMetadataSchemaId_Schema] FOREIGN KEY([PointMetadataSchemaId])
REFERENCES [dbo].[Schema] ([Id])
GO
ALTER TABLE [dbo].[ConnectorType] CHECK CONSTRAINT [FK_ConnectorType_PointMetadataSchemaId_Schema]
GO
