/****** Object:  Table [dbo].[AST_Assets]    Script Date: 26/04/2020 4:38:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AST_Assets](
	[Id] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[FloorCode] [nvarchar](32) NOT NULL,
	[AssetTypeId] [uniqueidentifier] NOT NULL,
	[ParentId] [uniqueidentifier] NULL,
	[RootId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](64) NOT NULL,
	[Description] [nvarchar](512) NOT NULL,
	[ExtraPropertiesJson] [nvarchar](max) NOT NULL,
	[IsArchived] [bit] NOT NULL,
	[ExternalId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_AST_Asset] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AST_AssetTypes]    Script Date: 26/04/2020 4:38:34 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AST_AssetTypes](
	[Id] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[ParentId] [uniqueidentifier] NULL,
	[Name] [nvarchar](64) NOT NULL,
	[ColumnsJson] [nvarchar](2048) NOT NULL,
	[Archived] [bit] NOT NULL,
	[ExternalId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_AST_AssetType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[AST_Assets]  WITH CHECK ADD  CONSTRAINT [FK_AST_Assets_AST_AssetTypes] FOREIGN KEY([AssetTypeId])
REFERENCES [dbo].[AST_AssetTypes] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AST_Assets] CHECK CONSTRAINT [FK_AST_Assets_AST_AssetTypes]
GO
