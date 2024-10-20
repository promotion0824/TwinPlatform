IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'TemperatureUnit'
          AND Object_ID = Object_ID(N'dbo.CustomerUserPreferences'))
BEGIN
    ALTER TABLE [dbo].[CustomerUserPreferences]
    ADD [TemperatureUnit] int NOT NULL DEFAULT 0
END
