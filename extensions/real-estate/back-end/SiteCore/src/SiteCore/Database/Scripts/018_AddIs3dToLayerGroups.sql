ALTER TABLE [dbo].[LayerGroups] ADD [Is3D] [bit] NOT NULL CONSTRAINT Is3DDefault DEFAULT 'False'
GO

ALTER TABLE [dbo].[LayerGroups] DROP Is3DDefault
GO