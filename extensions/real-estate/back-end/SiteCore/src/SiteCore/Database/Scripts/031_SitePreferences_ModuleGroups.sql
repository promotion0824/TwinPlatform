ALTER TABLE [dbo].[SitePreferences]
ADD ModuleGroups NVARCHAR(MAX) NOT NULL CONSTRAINT ModuleGroupsDefault DEFAULT '{}';