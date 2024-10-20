alter table [dbo].[LayerGroups] drop column [BackgroundImageId];
GO
alter table [dbo].[Modules] add [ImageWidth] [integer] NULL;
GO
alter table [dbo].[Modules] add [ImageHeight] [integer] NULL;
GO