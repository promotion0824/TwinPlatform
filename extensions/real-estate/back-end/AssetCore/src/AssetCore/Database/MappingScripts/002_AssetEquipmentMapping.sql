CREATE TABLE [dbo].[AssetEquipmentMapping](
	[AssetRegisterId] [int] NOT NULL,
	[EquipmentId] [uniqueidentifier] NOT NULL,		
 CONSTRAINT [PK_AssetEquipmentMapping] PRIMARY KEY CLUSTERED 
(
	[AssetRegisterId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE INDEX IDX_AssetEquipmentMapping_EquipmentId  
   ON [dbo].[AssetEquipmentMapping] ([EquipmentId]);   
GO  