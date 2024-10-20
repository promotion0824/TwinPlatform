delete from [AssetEquipmentMapping];
GO
drop INDEX IDX_AssetEquipmentMapping_EquipmentId ON [dbo].[AssetEquipmentMapping];
GO  
create unique INDEX AK_AssetEquipmentMapping_EquipmentId  
   ON [dbo].[AssetEquipmentMapping] ([EquipmentId])
GO