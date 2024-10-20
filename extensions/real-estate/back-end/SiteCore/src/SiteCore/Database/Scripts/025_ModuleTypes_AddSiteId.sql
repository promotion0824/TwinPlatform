ALTER TABLE [dbo].[ModuleTypes] ADD [SiteId] [uniqueidentifier]
GO

ALTER TABLE [dbo].[ModuleTypes]
ADD FOREIGN KEY (SiteId) REFERENCES Sites(Id)
GO