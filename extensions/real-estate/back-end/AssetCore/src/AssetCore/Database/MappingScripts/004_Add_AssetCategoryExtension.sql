/****** Object:  Table [dbo].[AssetCategoryExtension]   ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AssetCategoryExtension](
	[SiteId] [uniqueidentifier] NOT NULL,
	[CategoryId] [uniqueidentifier] NOT NULL,
	[ModuleTypePrefix] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_AssetCategoryExtension] PRIMARY KEY CLUSTERED 
(
	[SiteId] ASC,
	[CategoryId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO