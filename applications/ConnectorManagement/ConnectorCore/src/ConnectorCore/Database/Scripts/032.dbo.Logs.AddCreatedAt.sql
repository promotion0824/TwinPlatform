ALTER TABLE [dbo].[Logs] ADD [CreatedAt] [datetime2](7) NULL;
GO
update [dbo].[Logs] set [CreatedAt] = [StartTime];
GO
ALTER TABLE [dbo].[Logs] ALTER COLUMN [CreatedAt] [datetime2](7) NOT NULL;
GO