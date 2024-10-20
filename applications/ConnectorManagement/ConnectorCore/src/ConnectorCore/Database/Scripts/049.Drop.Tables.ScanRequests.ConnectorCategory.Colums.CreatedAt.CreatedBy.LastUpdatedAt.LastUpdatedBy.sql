DROP TABLE [dbo].[ScanRequests];
GO

DROP INDEX [Idx_ConnectorType_ConnectorCategoryId] 
	ON [dbo].[ConnectorType]
GO

ALTER TABLE [dbo].[ConnectorType] 
	DROP CONSTRAINT [FK_ConnectorType_ConnectorCategory]
GO

ALTER TABLE [dbo].[ConnectorType] 
	DROP COLUMN [ConnectorCategoryId];
GO

DROP TABLE [dbo].[ConnectorCategory];
GO

ALTER TABLE [dbo].[Connector]
	DROP COLUMN [CreatedAt], [CreatedBy], [LastUpdatedAt], [LastUpdatedBy];
GO

ALTER TABLE [dbo].[Point]
	DROP COLUMN [CreatedAt], [CreatedBy], [LastUpdatedAt], [LastUpdatedBy];
GO

ALTER TABLE [dbo].[Equipment]
	DROP COLUMN [CreatedAt], [CreatedBy], [LastUpdatedAt], [LastUpdatedBy];
GO

ALTER TABLE [dbo].[Device]
	DROP COLUMN [CreatedAt], [CreatedBy], [LastUpdatedAt], [LastUpdatedBy];
GO