SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquipmentPoint](
	[EquipmentId] [uniqueidentifier] NOT NULL,
	[PointEntityId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_EquipmentPoint] PRIMARY KEY CLUSTERED 
(
	[EquipmentId] ASC,
	[PointEntityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EquipmentPoint]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentPoint_Equipment] FOREIGN KEY([EquipmentId])
REFERENCES [dbo].[Equipment] ([Id])
GO
ALTER TABLE [dbo].[EquipmentPoint] CHECK CONSTRAINT [FK_EquipmentPoint_Equipment]
GO
ALTER TABLE [dbo].[EquipmentPoint]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentPoint_Point] FOREIGN KEY([PointEntityId])
REFERENCES [dbo].[Point] ([EntityId])
GO
ALTER TABLE [dbo].[EquipmentPoint] CHECK CONSTRAINT [FK_EquipmentPoint_Point]
GO
ALTER TABLE [dbo].[EquipmentPoint]  WITH CHECK ADD  CONSTRAINT [UNIQUE_PointEntityId] UNIQUE ([PointEntityId])
GO
