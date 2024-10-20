CREATE TABLE [dbo].[SiteBuildingMapping](
	[SiteId] [uniqueidentifier] NOT NULL,
	[BuildingId] [int] NOT NULL,	
 CONSTRAINT [PK_SiteBuildingMapping] PRIMARY KEY CLUSTERED 
(
	[SiteId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE UNIQUE INDEX AK_SiteBuildingMapping_BuildingId   
   ON [dbo].[SiteBuildingMapping] ([BuildingId]);   
GO  

CREATE TABLE [dbo].[FloorMapping](
	[FloorId] [uniqueidentifier] NOT NULL,
	[BuildingId] [int] NOT NULL,
	[FloorCode] [nvarchar](128) NOT NULL,	
 CONSTRAINT [PK_FloorMapping] PRIMARY KEY CLUSTERED 
(
	[FloorId] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE UNIQUE INDEX AK_FloorMapping_BuildingIdFloorCode  
   ON [dbo].[FloorMapping] ([BuildingId], [FloorCode]);   
GO  
