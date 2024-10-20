SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Point](
	[Id] [uniqueidentifier] NOT NULL,
	[EntityId] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](64) NULL,
	[ClientId] [uniqueidentifier] NOT NULL,
	[SiteId] [uniqueidentifier] NOT NULL,
	[Unit] [nvarchar](64) NOT NULL,
	[Type] [int] NOT NULL,
	[ExternalPointId] [nvarchar](max) NOT NULL,
	[Category] [nvarchar](max) NULL,
	[Metadata] [nvarchar](max) NOT NULL,
	[IsDetected] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[CreatedBy] [uniqueidentifier] NOT NULL,
	[LastUpdatedAt] [datetime2](7) NULL,
	[LastUpdatedBy] [uniqueidentifier] NULL,
	[DeviceId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Point] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
 CONSTRAINT [AK_Entity] UNIQUE NONCLUSTERED 
(
	[EntityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Point]  WITH CHECK ADD  CONSTRAINT [FK_Point_Device] FOREIGN KEY([DeviceId])
REFERENCES [dbo].[Device] ([Id])
GO
ALTER TABLE [dbo].[Point] CHECK CONSTRAINT [FK_Point_Device]
GO
ALTER TABLE [dbo].[Point]  WITH CHECK ADD  CONSTRAINT [FK_Point_PointType] FOREIGN KEY([Type])
REFERENCES [dbo].[PointType] ([Id])
GO
ALTER TABLE [dbo].[Point] CHECK CONSTRAINT [FK_Point_PointType]
GO
