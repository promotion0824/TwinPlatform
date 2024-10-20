-- Add ScanConfigurationSchemaId

ALTER TABLE [dbo].[ConnectorType]
ADD ScanConfigurationSchemaId uniqueidentifier;
GO

ALTER TABLE [dbo].[ConnectorType]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorType_ScanConfigurationSchemaId_Schema] FOREIGN KEY([ScanConfigurationSchemaId])
REFERENCES [dbo].[Schema] ([Id])
GO

ALTER TABLE [dbo].[ConnectorType] CHECK CONSTRAINT [FK_ConnectorType_ScanConfigurationSchemaId_Schema]
GO

CREATE NONCLUSTERED INDEX [Idx_ConnectorType_ScanConfigurationSchemaId]
    ON [dbo].[ConnectorType]([ScanConfigurationSchemaId] ASC);
GO