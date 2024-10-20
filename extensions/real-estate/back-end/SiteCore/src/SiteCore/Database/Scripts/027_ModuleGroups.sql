CREATE TABLE [dbo].[ModuleGroups](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[SortOrder] [int] NOT NULL,
	CONSTRAINT [FK_PersonOrder] FOREIGN KEY (SiteId) REFERENCES Sites(Id),
	CONSTRAINT [PK_ModuleGroups] PRIMARY KEY CLUSTERED ( [Id] ASC ) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
	) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ModuleTypes] ADD [ModuleGroupId] [uniqueidentifier]
GO

ALTER TABLE [dbo].[ModuleTypes]
ADD FOREIGN KEY (ModuleGroupId) REFERENCES ModuleGroups(Id)
GO

