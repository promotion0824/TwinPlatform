IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = N'NotificationsUsers')
BEGIN
    CREATE TABLE [dbo].[NotificationsUsers] (
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [NotificationId] UNIQUEIDENTIFIER NOT NULL,
        [State] INT NOT NULL,
        [ClearedDateTime] DATETIME2 NULL,
        CONSTRAINT PK_NotificationsUsers PRIMARY KEY (UserId, NotificationId),
        CONSTRAINT FK_Notifications_NotificationsUsers FOREIGN KEY (NotificationId) REFERENCES Notifications(Id)

    )
END
