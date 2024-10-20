
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'AssignedTo'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE [dbo].[WF_Ticket] DROP COLUMN [AssignedTo]
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'AssigneeName'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE [dbo].[WF_Ticket] ADD [AssigneeName] NVARCHAR (100) NULL; -- In DirectoryCoreDb, FirstName and LastName are nvarchar (100) each
END
