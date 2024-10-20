SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PortfolioWidgets](
	[PortfolioId] [uniqueidentifier] NOT NULL,
	[WidgetId] [uniqueidentifier] NOT NULL,
	[Position] [int] NOT NULL,
 CONSTRAINT [PK_PortfolioWidgets] PRIMARY KEY CLUSTERED 
(
	[PortfolioId] ASC,
	[WidgetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PortfolioWidgets] ADD  CONSTRAINT [DF_PortfolioWidgets_Position]  DEFAULT ((0)) FOR [Position]
GO
ALTER TABLE [dbo].[PortfolioWidgets]  WITH CHECK ADD  CONSTRAINT [FK_PortfolioWidgets_Widgets] FOREIGN KEY([WidgetId])
REFERENCES [dbo].[Widgets] ([Id])
GO
ALTER TABLE [dbo].[PortfolioWidgets] CHECK CONSTRAINT [FK_PortfolioWidgets_Widgets]
GO