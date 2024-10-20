IF (NOT EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'Locations'))
BEGIN
CREATE TABLE [dbo].[Locations] (
    [NotificationTriggerId] [uniqueidentifier] NOT NULL,
    [Id] NVARCHAR(250) NOT NULL,
    CONSTRAINT [FK_Location_NotificationTrigger] FOREIGN KEY (NotificationTriggerId) REFERENCES NotificationTriggers(Id),
    CONSTRAINT [PK_Location] PRIMARY KEY CLUSTERED ([NotificationTriggerId],[Id]) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
END

IF (Not EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'NotificationTriggerTwinCategories'))
BEGIN
CREATE TABLE [dbo].[NotificationTriggerTwinCategories] (
    NotificationTriggerId UNIQUEIDENTIFIER NOT NULL,
    CategoryId NVARCHAR(250) NOT NULL,
    CONSTRAINT PK_NotificationTriggerTwinCategories PRIMARY KEY (NotificationTriggerId, CategoryId),
    CONSTRAINT FK_NotificationTriggerTwinCategories_NotificationTriggerId FOREIGN KEY (NotificationTriggerId) REFERENCES NotificationTriggers(Id)
)
End

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = N'NotificationTriggers' AND COLUMN_NAME = N'ChannelJson')
BEGIN
    ALTER TABLE NotificationTriggers
    ADD ChannelJson NVARCHAR(250) NOT NULL 
END

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = N'NotificationTriggers' AND COLUMN_NAME = N'PriorityJson')
BEGIN
    ALTER TABLE NotificationTriggers
    ADD PriorityJson  NVARCHAR(250) NULL
END

IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = N'NotificationTriggers' AND COLUMN_NAME = N'LocationJson')
BEGIN
    ALTER TABLE NotificationTriggers
    DROP COLUMN LocationJson
END

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'NotificationTriggerPriorities'))
BEGIN
Drop TABLE [dbo].[NotificationTriggerPriorities]
End

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'NotificationTriggerChannels'))
BEGIN
Drop TABLE [dbo].[NotificationTriggerChannels]
End

IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_SCHEMA = 'dbo' 
                 AND  TABLE_NAME = 'NotificationTriggerCategories'))
BEGIN
EXEC sp_rename 'dbo.NotificationTriggerCategories' ,  'NotificationTriggerSkillCategories'
End

IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'CanpUserDisableNotification'
          AND Object_ID = Object_ID(N'dbo.NotificationTriggers'))
BEGIN
   EXEC sp_rename 'dbo.NotificationTriggers.CanpUserDisableNotification', 'CanUserDisableNotification', 'COLUMN';
END

