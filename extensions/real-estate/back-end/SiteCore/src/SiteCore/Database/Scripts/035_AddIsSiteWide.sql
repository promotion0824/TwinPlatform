IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'IsSiteWide'
          AND Object_ID = Object_ID(N'dbo.Floors'))
BEGIN
    ALTER TABLE [dbo].Floors ADD IsSiteWide BIT NOT NULL DEFAULT (0)
END