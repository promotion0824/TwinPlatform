-- Floors
ALTER TABLE [dbo].[Floors] DROP COLUMN [CreatedOn];
ALTER TABLE [dbo].[Floors] DROP COLUMN [UpdatedOn];
ALTER TABLE [dbo].[Floors] DROP COLUMN [NetLettableArea];
ALTER TABLE [dbo].[Floors] DROP COLUMN [Area];

-- LayerEquipment
ALTER TABLE [dbo].[LayerEquipment] DROP COLUMN [CreatedOn];
ALTER TABLE [dbo].[LayerEquipment] DROP COLUMN [UpdatedOn];

-- LayerGroups
ALTER TABLE [dbo].[LayerGroups] DROP COLUMN [UpdatedOn];

-- Layers
ALTER TABLE [dbo].[Layers] DROP COLUMN [CreatedOn];
ALTER TABLE [dbo].[Layers] DROP COLUMN [UpdatedOn];

-- Modules
ALTER TABLE [dbo].[Modules] DROP COLUMN [CreatedOn];
ALTER TABLE [dbo].[Modules] DROP COLUMN [UpdatedOn];

-- ModuleTypes
ALTER TABLE [dbo].[ModuleTypes] DROP COLUMN [CreatedOn];
ALTER TABLE [dbo].[ModuleTypes] DROP COLUMN [UpdatedOn];