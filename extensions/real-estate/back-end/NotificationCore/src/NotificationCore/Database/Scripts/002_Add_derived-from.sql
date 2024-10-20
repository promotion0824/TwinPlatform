

IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = N'NotificationTriggers' AND COLUMN_NAME = N'IsDefault')
BEGIN
    ALTER TABLE NotificationTriggers
    ADD IsDefault [bit] NOT NULL Default (0)
END


IF NOT EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
              WHERE TABLE_NAME = N'NotificationTriggers' AND COLUMN_NAME = N'DerivedFrom')
BEGIN
    ALTER TABLE NotificationTriggers
    ADD DerivedFrom UNIQUEIDENTIFIER NULL CONSTRAINT [FK_DerivedFrom_NotificationTriggerId] FOREIGN KEY(DerivedFrom) REFERENCES NotificationTriggers(id)
END

