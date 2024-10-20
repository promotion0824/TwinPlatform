
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = N'WF_JobTypes')
BEGIN
    CREATE TABLE [dbo].[WF_JobTypes] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(500) NOT NULL,
    )

    CREATE UNIQUE INDEX UX_WF_JobTypes_Name ON [dbo].[WF_JobTypes]([Name])
END

IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'JobTypeId'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE dbo.WF_Ticket
    ADD JobTypeId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Ticket_JobType FOREIGN KEY (JobTypeId) REFERENCES WF_JobTypes(Id);
END


IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'JobType'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE dbo.WF_Ticket
    DROP COLUMN JobType
   
END
