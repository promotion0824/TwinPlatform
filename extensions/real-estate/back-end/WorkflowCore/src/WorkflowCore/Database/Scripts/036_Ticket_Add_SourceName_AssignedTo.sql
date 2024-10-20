-- sp_help WF_TICKET

ALTER TABLE [dbo].[WF_Ticket] ADD [ComputedCreatedDate] AS (IIF(LastUpdatedByExternalSource = 1, ISNULL(ExternalCreatedDate, CreatedDate), CreatedDate))  -- CreatedDate = model.LastUpdatedByExternalSource ? model.ExternalCreatedDate ?? model.CreatedDate : model.CreatedDate,
ALTER TABLE [dbo].[WF_Ticket] ADD [ComputedUpdatedDate] AS (IIF(LastUpdatedByExternalSource = 1, ISNULL(ExternalUpdatedDate, UpdatedDate), UpdatedDate)) 

ALTER TABLE [dbo].[WF_Ticket] ADD [SourceName] NVARCHAR (64) NULL; -- In Marketplace Db, App Name is navarchar(128)
ALTER TABLE [dbo].[WF_Ticket] ADD [AssigneeName] NVARCHAR (100) NULL; -- In DirectoryCoreDb, FirstName and LastName nvarchar (100) each

/* 
ALTER TABLE [dbo].[WF_Ticket] DROP COLUMN [ComputedCreatedDate]
ALTER TABLE [dbo].[WF_Ticket] DROP COLUMN [ComputedUpdatedDate]
ALTER TABLE [dbo].[WF_Ticket] DROP COLUMN [SourceName] 
ALTER TABLE [dbo].[WF_Ticket] DROP COLUMN [AssignedTo]
*/