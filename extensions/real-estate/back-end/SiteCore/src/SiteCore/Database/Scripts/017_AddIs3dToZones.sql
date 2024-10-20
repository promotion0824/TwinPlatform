ALTER TABLE [dbo].[Zones] DROP COLUMN [CreatedOn]
GO

ALTER TABLE [dbo].[Zones] DROP COLUMN [UpdatedOn]
GO

ALTER TABLE [dbo].[Zones] ADD [Is3D] [bit] NOT NULL CONSTRAINT Is3DDefault DEFAULT 'False'
GO

ALTER TABLE [dbo].[Zones] DROP Is3DDefault
GO