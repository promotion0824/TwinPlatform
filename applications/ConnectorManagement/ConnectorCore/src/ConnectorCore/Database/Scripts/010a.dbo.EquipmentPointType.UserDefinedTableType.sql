CREATE TYPE [dbo].[EquipmentPointType] AS TABLE(
	[EquipmentId] [uniqueidentifier] NOT NULL,
	[PointEntityId] [uniqueidentifier] NOT NULL,
	PRIMARY KEY CLUSTERED 
(
	[EquipmentId] ASC,
	[PointEntityId] ASC
)WITH (IGNORE_DUP_KEY = OFF)
)
GO