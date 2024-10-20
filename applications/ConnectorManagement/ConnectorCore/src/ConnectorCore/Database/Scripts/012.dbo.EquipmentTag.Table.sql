SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EquipmentTag](
	[EquipmentId] [uniqueidentifier] NOT NULL,
	[TagId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_EquipmentTag] PRIMARY KEY CLUSTERED 
(
	[EquipmentId] ASC,
	[TagId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EquipmentTag]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentTag_Equipment] FOREIGN KEY([EquipmentId])
REFERENCES [dbo].[Equipment] ([Id])
GO
ALTER TABLE [dbo].[EquipmentTag] CHECK CONSTRAINT [FK_EquipmentTag_Equipment]
GO
ALTER TABLE [dbo].[EquipmentTag]  WITH CHECK ADD  CONSTRAINT [FK_EquipmentTag_Tag] FOREIGN KEY([TagId])
REFERENCES [dbo].[Tag] ([Id])
GO
ALTER TABLE [dbo].[EquipmentTag] CHECK CONSTRAINT [FK_EquipmentTag_Tag]
GO
