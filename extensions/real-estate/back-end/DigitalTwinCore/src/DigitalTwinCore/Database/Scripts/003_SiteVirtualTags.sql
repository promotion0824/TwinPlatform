﻿SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[DT_SiteVirtualTags](
	[Id] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[Tag] [nvarchar](128) NOT NULL,
	[MatchModelId] [nvarchar](256) NULL,
	[MatchTagList] [nvarchar](1024) NULL,
 CONSTRAINT [PK_DT_SiteVirtualTags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DT_SiteVirtualTags]  WITH CHECK ADD  CONSTRAINT [FK_DT_SiteVirtualTags_DT_Tags] FOREIGN KEY([Tag])
REFERENCES [dbo].[DT_Tags] ([Name])
GO

ALTER TABLE [dbo].[DT_SiteVirtualTags] CHECK CONSTRAINT [FK_DT_SiteVirtualTags_DT_Tags]
GO
