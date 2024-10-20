SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TagCategory](
	[TagId] [uniqueidentifier] NOT NULL,
	[CategoryId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_TagCategory] PRIMARY KEY CLUSTERED 
(
	[TagId] ASC,
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[TagCategory]  WITH CHECK ADD  CONSTRAINT [FK_TagCategory_Tag] FOREIGN KEY([TagId])
REFERENCES [dbo].[Tag] ([Id])
GO
ALTER TABLE [dbo].[TagCategory] CHECK CONSTRAINT [FK_TagCategory_Tag]
GO
ALTER TABLE [dbo].[TagCategory]  WITH CHECK ADD  CONSTRAINT [FK_TagCategory_Category] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Category] ([Id])
GO
ALTER TABLE [dbo].[TagCategory] CHECK CONSTRAINT [FK_TagCategory_Category]
GO