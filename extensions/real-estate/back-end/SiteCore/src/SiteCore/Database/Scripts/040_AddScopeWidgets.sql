SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ScopeWidgets](
	[Id] [uniqueidentifier] NOT NULL,
	[ScopeId] NVARCHAR(250) NULL,
	[WidgetId] [uniqueidentifier] NOT NULL,
	[Position] [nvarchar](512) NOT NULL,
	 CONSTRAINT [PK_ScopedWidgets] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ScopeWidgets]  WITH CHECK ADD CONSTRAINT [FK_ScopeWidgets_Widgets] FOREIGN KEY([WidgetId])
REFERENCES [dbo].[Widgets] ([Id])
GO

ALTER TABLE [dbo].[ScopeWidgets] CHECK CONSTRAINT [FK_ScopeWidgets_Widgets]
GO
