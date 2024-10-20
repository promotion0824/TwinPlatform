IF NOT EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'ScopeId'
          AND Object_ID = Object_ID(N'dbo.SitePreferences'))
BEGIN
    ALTER TABLE [dbo].[SitePreferences]
    ADD [ScopeId] NVARCHAR(200) NOT NULL DEFAULT ''
END

ALTER TABLE  [dbo].[SitePreferences]
DROP CONSTRAINT [PK_SitePreferences]
GO
ALTER TABLE [dbo].[SitePreferences] ADD CONSTRAINT [PK_SitePreferences] PRIMARY KEY CLUSTERED 
(
	[SiteId] ASC,
	[ScopeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
