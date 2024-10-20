IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'CustomProperties'
          AND Object_ID = Object_ID(N'dbo.WF_Ticket'))
BEGIN
    ALTER TABLE dbo.WF_Ticket ADD CustomProperties [nvarchar](max) NULL;
END
GO
