ALTER TABLE [dbo].[Category]  WITH CHECK ADD  CONSTRAINT [FK_Category_ParentId] FOREIGN KEY([ParentId])
REFERENCES [dbo].[Category] ([Id])
ON DELETE NO ACTION
ON UPDATE NO ACTION
GO

ALTER TABLE [dbo].[Category] CHECK CONSTRAINT [FK_Category_ParentId]
GO

ALTER TABLE [dbo].[Category] drop column [HasChildren]
GO