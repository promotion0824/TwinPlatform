IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'SpaceTwinId'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE dbo.WF_Ticket ADD SpaceTwinId [nvarchar](250) NULL;
END
GO
IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'JobType'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE dbo.WF_Ticket ADD JobType int NULL;
END
Go
IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'SubStatus'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE dbo.WF_Ticket ADD SubStatus int NULL;
END
Go
