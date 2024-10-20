drop table [dbo].[DisciplineImages]
GO
drop table [dbo].[ImageTypes]
GO
CREATE TABLE [dbo].[Modules](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[FloorId] [uniqueidentifier] NOT NULL,
	[ModuleTypeId] [uniqueidentifier] NOT NULL,
	[VisualId] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[UpdatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Modules] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[ModuleTypes](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Prefix] [nvarchar](100) NOT NULL,
	[ModuleGroup] [nvarchar](100) NOT NULL,
	[SortOrder] [int] NOT NULL,
	[Is3D] [bit] NOT NULL,
	[CanBeDeleted] [bit] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[UpdatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_ModuleTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Modules]  WITH CHECK ADD  CONSTRAINT [FK_Modules_ModuleTypes] FOREIGN KEY([ModuleTypeId])
REFERENCES [dbo].[ModuleTypes] ([Id])
GO

