SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Widgets](
	[Id] [uniqueidentifier] NOT NULL,
	[Type] [int] NOT NULL,
	[Metadata] [nvarchar](1024) NOT NULL
	 CONSTRAINT [PK_Widgets] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SiteWidgets](
	[SiteId] [uniqueidentifier] NOT NULL,
	[WidgetId] [uniqueidentifier] NOT NULL,
	[Position] [nvarchar](512) NOT NULL,
 CONSTRAINT [PK_SiteWidgets] PRIMARY KEY CLUSTERED 
(
	[SiteId] ASC,
	[WidgetId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SiteWidgets]  WITH CHECK ADD  CONSTRAINT [FK_SiteWidgets_Widgets] FOREIGN KEY([WidgetId])
REFERENCES [dbo].[Widgets] ([Id])
GO

ALTER TABLE [dbo].[SiteWidgets] CHECK CONSTRAINT [FK_SiteWidgets_Widgets]
GO

ALTER TABLE [dbo].[SiteWidgets]  WITH CHECK ADD  CONSTRAINT [FK_SiteWidgets_Sites] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[SiteWidgets] CHECK CONSTRAINT [FK_SiteWidgets_Sites]
GO
