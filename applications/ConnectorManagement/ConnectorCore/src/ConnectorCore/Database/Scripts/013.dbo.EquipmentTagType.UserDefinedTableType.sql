CREATE TYPE [dbo].[EquipmentTagType] AS TABLE(
	[EquipmentId] [uniqueidentifier] NOT NULL,
	[TagId] [uniqueidentifier] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[EquipmentId] ASC,
	[TagId] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO
