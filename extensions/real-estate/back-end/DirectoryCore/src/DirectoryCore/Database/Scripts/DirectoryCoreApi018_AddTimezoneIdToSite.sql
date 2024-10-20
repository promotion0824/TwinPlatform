ALTER TABLE [dbo].[Sites]
ADD [TimezoneId] NVARCHAR(32) NOT NULL CONSTRAINT TimezoneIdDefault DEFAULT '';
GO

ALTER TABLE [dbo].[Sites]
    DROP TimezoneIdDefault
GO