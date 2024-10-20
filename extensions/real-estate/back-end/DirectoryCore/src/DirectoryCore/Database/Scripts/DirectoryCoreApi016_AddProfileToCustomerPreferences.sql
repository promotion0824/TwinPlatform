ALTER TABLE [dbo].[CustomerUserPreferences]
ADD [Profile] NVARCHAR(MAX) NOT NULL CONSTRAINT ProfileDefault DEFAULT '';
GO

ALTER TABLE [dbo].[CustomerUserPreferences]
    DROP ProfileDefault
GO