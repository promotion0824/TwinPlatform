SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Floors](
	[Id] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Code] [nvarchar](10) NULL,
	[Area] [float] NULL,
	[NetLettableArea] [float] NULL,
	[SortOrder] [int] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[UpdatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Floors] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Layers](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[LayerGroupId] [uniqueidentifier] NOT NULL,
	[TagName] [nvarchar](500) NULL,
	[SortOrder] [int] NOT NULL,	
	[CreatedOn] [datetime2](7) NOT NULL,
	[UpdatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Layers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sites](
	[Id] [uniqueidentifier] NOT NULL,
	[CustomerId] [uniqueidentifier] NOT NULL,
	[PortfolioId] [uniqueidentifier] NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Code] [nvarchar](100) NOT NULL,
	[Address] [nvarchar](250) NOT NULL,
	[State] [nvarchar](50) NOT NULL,
	[Postcode] [nvarchar](20) NOT NULL,
	[Country] [nvarchar](50) NOT NULL,
	[NumberOfFloors] [int] NOT NULL,
	[Area] [float] NOT NULL,
	[NetLettableArea] [float] NOT NULL,
	[LogoId] [uniqueidentifier] NULL,
	[Contact] [nvarchar](100) NOT NULL,
	[ContactNumber] [nvarchar](100) NOT NULL,
	[ContactEmail] [nvarchar](100) NOT NULL,
	[ImageUrl] [varchar](500) NOT NULL,
	[Introduction] [nvarchar](4000) NOT NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
	[TimezoneId] [varchar](32) NOT NULL,
	[Status] [int] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[UpdatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Sites] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LayerGroups](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[SortOrder] [int] NOT NULL,
	[FloorId] [uniqueidentifier] NOT NULL,
	[ZIndex] [int] NOT NULL,
	[BackgroundImageId] [uniqueidentifier] NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[UpdatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_LayerGroups] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Zones](
	[Id] [uniqueidentifier] NOT NULL,
	[LayerGroupId] [uniqueidentifier] NOT NULL,
	[Geometry] [nvarchar](max) NOT NULL,
	[ZIndex] [int] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[UpdatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Zones] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] 
GO
CREATE TABLE [dbo].[LayerEquipment](
	[LayerGroupId] [uniqueidentifier] NOT NULL,
	[EquipmentId] [uniqueidentifier] NOT NULL,
	[ZoneId] [uniqueidentifier] NOT NULL,
	[Geometry] [nvarchar](max) NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[UpdatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_LayerEquipment] PRIMARY KEY CLUSTERED 
(
	[LayerGroupId], [EquipmentId]
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DisciplineImages](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[SortOrder] [int] NOT NULL,
	[FloorId] [uniqueidentifier] NOT NULL,
	[ImageTypeId] [uniqueidentifier] NOT NULL,
	[ImageId] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[UpdatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_DisciplineImages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [dbo].[ImageTypes](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Prefix] [nvarchar](100) NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[ImageGroup] [nvarchar](100) NOT NULL,
	[CanBeDeleted] [bit] NOT NULL,
	[CreatedOn] [datetime2](7) NOT NULL,
	[UpdatedOn] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_ImageTypes] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[DisciplineImages]  WITH CHECK ADD  CONSTRAINT [FK_DisciplineImages_ImageTypes] FOREIGN KEY([ImageTypeId])
REFERENCES [dbo].[ImageTypes] ([Id])
GO
ALTER TABLE [dbo].[DisciplineImages] CHECK CONSTRAINT [FK_DisciplineImages_ImageTypes]
GO
ALTER TABLE [dbo].[ImageTypes]  WITH CHECK ADD  CONSTRAINT [FK_ImageTypes_Sites] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([Id])
GO
ALTER TABLE [dbo].[ImageTypes] CHECK CONSTRAINT [FK_ImageTypes_Sites]
GO
ALTER TABLE [dbo].[LayerGroups] ADD  CONSTRAINT [DF_LayerGroups_ZIndex]  DEFAULT ((0)) FOR [ZIndex]
GO
ALTER TABLE [dbo].[Zones] ADD  CONSTRAINT [DF_Zones_ZIndex]  DEFAULT ((0)) FOR [ZIndex]
GO
ALTER TABLE [dbo].[Floors]  WITH CHECK ADD  CONSTRAINT [FK_Floors_Sites] FOREIGN KEY([SiteId])
REFERENCES [dbo].[Sites] ([Id])
GO
ALTER TABLE [dbo].[Floors] CHECK CONSTRAINT [FK_Floors_Sites]
GO
ALTER TABLE [dbo].[Layers]  WITH CHECK ADD  CONSTRAINT [FK_Layers_LayerGroups] FOREIGN KEY([LayerGroupId])
REFERENCES [dbo].[LayerGroups] ([Id])
GO
ALTER TABLE [dbo].[Layers] CHECK CONSTRAINT [FK_Layers_LayerGroups]
GO
ALTER TABLE [dbo].[LayerGroups]  WITH CHECK ADD  CONSTRAINT [FK_LayerGroups_Floors] FOREIGN KEY([FloorId])
REFERENCES [dbo].[Floors] ([Id])
GO
ALTER TABLE [dbo].[LayerGroups] CHECK CONSTRAINT [FK_LayerGroups_Floors]
GO
ALTER TABLE [dbo].[Zones]  WITH CHECK ADD  CONSTRAINT [FK_Zones_LayerGroups] FOREIGN KEY([LayerGroupId])
REFERENCES [dbo].[LayerGroups] ([Id])
GO
ALTER TABLE [dbo].[Zones] CHECK CONSTRAINT [FK_Zones_LayerGroups]
GO
ALTER TABLE [dbo].[LayerEquipment]  WITH CHECK ADD  CONSTRAINT [FK_LayerEquipment_LayerGroups] FOREIGN KEY([LayerGroupId])
REFERENCES [dbo].[LayerGroups] ([Id])
GO
ALTER TABLE [dbo].[LayerEquipment] CHECK CONSTRAINT [FK_LayerEquipment_LayerGroups]
GO
ALTER TABLE [dbo].[LayerEquipment]  WITH CHECK ADD  CONSTRAINT [FK_LayerEquipment_Zones] FOREIGN KEY([ZoneId])
REFERENCES [dbo].[Zones] ([Id])
GO
ALTER TABLE [dbo].[LayerEquipment] CHECK CONSTRAINT [FK_LayerEquipment_Zones]
GO
ALTER TABLE [dbo].[DisciplineImages]  WITH CHECK ADD  CONSTRAINT [FK_DisciplineImages_Floors] FOREIGN KEY([FloorId])
REFERENCES [dbo].[Floors] ([Id])
GO
ALTER TABLE [dbo].[DisciplineImages] CHECK CONSTRAINT [FK_DisciplineImages_Floors]
GO
