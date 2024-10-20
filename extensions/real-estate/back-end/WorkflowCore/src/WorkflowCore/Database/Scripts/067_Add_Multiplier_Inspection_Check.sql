IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'Multiplier'
          AND Object_ID = Object_ID(N'dbo.WF_Checks'))
BEGIN
    ALTER TABLE dbo.WF_Checks ADD Multiplier [float] NOT NULL CONSTRAINT TypeDefault DEFAULT 1;
END
GO
