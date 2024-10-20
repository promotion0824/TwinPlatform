IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = N'NotificationTriggers' AND COLUMN_NAME = N'IsMuted')
BEGIN
    ALTER TABLE NotificationTriggers
    ADD IsMuted [bit] NOT NULL Default (0)
END

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = N'NotificationSubscriptionOverrides' AND COLUMN_NAME = N'IsMuted')
BEGIN
    ALTER TABLE NotificationSubscriptionOverrides
    ADD IsMuted [bit] NOT NULL Default (0)
END

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = N'NotificationTriggerTwins' AND COLUMN_NAME = N'TwinName')
BEGIN
    ALTER TABLE NotificationTriggers
    ADD TwinName NVARCHAR(MAX) NULL
END
