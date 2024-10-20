IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = N'Notifications')
BEGIN
    CREATE TABLE [dbo].[Notifications] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [CreatedDateTime] DATETIME2 NOT NULL,
        [Source] INT NOT NULL,
        [Title] NVARCHAR(512) NOT NULL,
        [PropertyBagJson] NVARCHAR(MAX) NOT NULL,
        [TriggerIdsJson] NVARCHAR(MAX) NOT NULL,
    )
END
