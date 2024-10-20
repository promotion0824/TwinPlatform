IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = N'WF_ExternalProfiles')
BEGIN
    CREATE TABLE [dbo].[WF_ExternalProfiles] (
        [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(250) NOT NULL,
        [Email] NVARCHAR(100) NOT NULL,
        [Phone] NVARCHAR(32)  NULL,
        [Company] NVARCHAR(250) NULL
    )

    CREATE UNIQUE INDEX UX_WF_ExternalProfiles_Email ON [dbo].[WF_ExternalProfiles]([Email])
END
